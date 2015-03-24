using Flooder.Event;
using Flooder.Utility;
using MsgPack.Serialization;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Flooder.Transfer
{
    public class FluentMessageBroker : IMessageBroker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly SerializationContext MsgPackDefaultContext = new SerializationContext();
        private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        private readonly BlockingCollection<byte[]> _queue;
        private readonly TimeSpan _interval;
        private readonly int _retryMaxCount, _extraction;
        private readonly TcpManager _tcp;

        public int Count { get { return _queue.Count; } }

        public FluentMessageBroker(TcpManager tcp, TimeSpan? interval = null, int retryCount = 3, int capacity = 10000, int extraction = 10)
        {
            _queue         = new BlockingCollection<byte[]>(capacity);
            _tcp           = tcp;
            _interval      = interval ?? TimeSpan.FromSeconds(1);
            _retryMaxCount = retryCount;
            _extraction    = extraction;
        }

        public void Publish(string tag, Dictionary<string, object> payload)
        {
            this.Publish(tag, new []{ payload });
        }

        public void Publish(string tag, Dictionary<string, object>[] payloads)
        {
            if (!_tcp.HasConnection) return; //skip.

            var timestamp = DateTime.Now.ToUnixTime();

            using (var ms = new MemoryStream())
            {
                var packer = MsgPack.Packer.Create(ms);

                foreach (var payload in payloads)
                {
                    //["tag", timestamp, payload]
                    packer.PackArrayHeader(3);
                    packer.PackString(tag);
                    packer.Pack(timestamp);
                    packer.PackMapHeader(payload);

                    foreach (var column in payload)
                    {
                        packer.PackString(column.Key);

                        var type = column.Value != null ? column.Value.GetType() : typeof(string);

                        MsgPackDefaultContext
                            .GetSerializer(type)
                            .PackTo(packer, column.Value);
                    }

                }

                var arraySegment = new ArraySegment<byte>(ms.ToArray(), 0, (int) ms.Length);
                var bytes = arraySegment.ToArray();

#if DEBUG
                Logger.Trace("{0} {1} {2}", timestamp, tag, JsonSerializer.Serialize(payloads));
#endif

                Publish(bytes);
            }
        }

        public void Publish(byte[] bytes)
        {
            var retryCount = 0;

            Observable.Create<TimeSpan>(x =>
            {
                if (_queue.TryAdd(bytes))
                {
                    x.OnCompleted();
                    return Disposable.Empty;
                }

                x.OnError(new InvalidOperationException(string.Format("Queue overflow. retryCount:{0}, sleepTime:{1}", ++retryCount, _interval)));
                x.OnNext(_interval);
                return Disposable.Empty;
            })
            .Retry(_retryMaxCount)
            .Subscribe(
                Thread.Sleep,
                ex => Logger.ErrorException("FluentMessageBroker#Publish.", ex),
                () => Logger.Trace("FluentMessageBroker#Publish success.")
            );
        }

        public IDisposable Subscribe()
        {
            return Observable.Start(() =>
            {
                while (true)
                {
                    var takeCount = this._extraction; //initialize.

                    if (_queue.Count > 0)
                    {
                        if (_queue.Count < takeCount)
                        {
                            takeCount = _queue.Count;
                        }

                        var items = _queue.GetConsumingEnumerable()
                            .Take(takeCount)
                            .ToArray();

                        if (_tcp.HasConnection && items.Length > 0)
                        {
                            Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = takeCount }, item =>
                            {
                                _tcp.Transfer(item);
                            });

                            continue;
                        }
                    }

                    Thread.Sleep(_interval);
                }
            })
            .Subscribe(
                x => { },
                ex => Logger.ErrorException("FluentMessageBroker#Subscribe", ex),
                () => Logger.Fatal("FluentMessageBroker#Subscribe stoped.")
            );
        }
    }
}

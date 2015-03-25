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
using Flooder.Event;
using Flooder.Utility;
using MsgPack.Serialization;
using NLog;

namespace Flooder.Worker
{
    public class FluentMessageBroker : IMessageBroker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly SerializationContext MsgPackDefaultContext = new SerializationContext();
        private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        private readonly BlockingCollection<byte[]> _queue;
        private readonly TcpManager _tcp;
        private readonly WorkerOption _option;

        public int Count { get { return _queue.Count; } }

        public FluentMessageBroker(WorkerOption option)
        {
            _queue  = new BlockingCollection<byte[]>(option.Capacity);
            _tcp    = new TcpManager(option.Hosts);
            _option = option;
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

//#if DEBUG
//                Logger.Trace("{0}, {1} {2}", tag, timestamp, JsonSerializer.Serialize(payloads));
//#endif

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

                x.OnError(new InvalidOperationException(string.Format(
                    "Queue overflow. retryCount:{0}, sleepTime:{1}", ++retryCount, _option.Interval)));

                x.OnNext(_option.Interval);
                return Disposable.Empty;
            })
            .Retry(_option.RetryMaxCount)
            .Subscribe(
                Thread.Sleep,
                ex => Logger.ErrorException("FluentMessageBroker#Publish.", ex),
                () => Logger.Trace("FluentMessageBroker#Publish success.")
            );
        }

        public IEnumerable<IDisposable> Subscribe()
        {
            var healthCheck = _tcp.HealthCheck();

            var messageBroker = Observable.Start(() =>
            {
                while (true)
                {
                    var takeCount = _option.Extraction; //initialize.

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
                            if (items.Length == 1)
                            {
                                foreach (var item in items)
                                {
                                    _tcp.Transfer(item);
                                }
                            }
                            else
                            {
                                Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = takeCount }, item =>
                                {
                                    _tcp.Transfer(item);
                                });
                            }

                            continue;
                        }
                    }

                    Thread.Sleep(_option.Interval);
                }
            })
            .Subscribe(
                x => { },
                ex => Logger.ErrorException("FluentMessageBroker#Subscribe", ex),
                () => Logger.Fatal("FluentMessageBroker#Subscribe stoped.")
            );

            return new[] { healthCheck, messageBroker };
        }
    }
}

using Flooder.Event;
using Flooder.Utility;
using MsgPack.Serialization;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
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
        private readonly int _retryMaxCount;
        private readonly TcpManager _tcp;

        public FluentMessageBroker(TcpManager tcp, TimeSpan interval, int retryCount)
        {
            _queue         = new BlockingCollection<byte[]>(1000);
            _tcp           = tcp;
            _interval      = interval;
            _retryMaxCount = retryCount;
        }

        public void Publish(string tag, IDictionary<string, object> payload)
        {
            if (!_tcp.HasConnection) return; //skip.

            var timestamp = DateTime.Now.ToUnixTime();

            using (var ms = new MemoryStream())
            {
                var packer = MsgPack.Packer.Create(ms);

                //["tag", timestamp, payload]
                packer.PackArrayHeader(3);
                packer.PackString(tag);
                packer.Pack(timestamp);
                packer.PackMapHeader(payload);

                foreach (var column in payload)
                {
                    packer.PackString(column.Key);

                    var type = column.Value != null ? column.Value.GetType() : typeof (string);

                    MsgPackDefaultContext
                        .GetSerializer(type)
                        .PackTo(packer, column.Value);
                }

                var arraySegment = new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
                var bytes = arraySegment.ToArray();

#if DEBUG
                Logger.Trace("{0} {1} {2}", timestamp, tag, JsonSerializer.Serialize(payload));
#endif

                Publish(bytes);
            }
        }

        public void Publish(string tag, IDictionary<string, object>[] payload)
        {
            //TODO:MsgPack
            var obj = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(obj);
            this.Publish(bytes);
        }

        public void Publish(byte[] bytes)
        {
            var retryCount = 0;

            Observable.Create<int>(x =>
            {
                try
                {
                    if (_queue.TryAdd(bytes))
                    {
                        x.OnCompleted();
                        return Disposable.Empty;
                    }

                    x.OnNext(retryCount++);
                }
                catch (Exception ex)
                {
                    x.OnError(ex);
                    x.OnCompleted();
                }

                return Disposable.Empty;
            })
            .Retry(_retryMaxCount)
            .Subscribe(
                cnt =>
                {
                    Logger.Debug("[OnNext] FluentMessageBroker#Publish. retryCount:{0}, interval:{1}", cnt, _interval);
                    Thread.Sleep(_interval);
                },
                ex => Logger.ErrorException("[OnError] FluentMessageBroker#Publish.", ex),
                () => Logger.Debug("[OnComplete] FluentMessageBroker#Publish.")
            );
        }

        public IDisposable Subscribe()
        {
            return Observable.Create<Unit>(x =>
            {
                try
                {
                    byte[] item;
                    if (_tcp.HasConnection && _queue.TryTake(out item))
                    {
                        _tcp.Transfer(item);
                        return Disposable.Empty;
                    }

                    x.OnNext(Unit.Default);
                }
                catch (Exception ex)
                {
                    x.OnError(ex);
                    x.OnCompleted();
                }

                return Disposable.Empty;
            })
            .Retry()
            .Subscribe(
                _ =>
                {
                    Logger.Debug("[OnNext] FluentMessageBroker#Subscribe.");
                    Thread.Sleep(_interval);
                },
                ex => Logger.ErrorException("[OnError] FluentMessageBroker#Subscribe.", ex),
                () => Logger.Fatal("[OnComplete] FluentMessageBroker#Subscribe.")
            );
        }
    }
}

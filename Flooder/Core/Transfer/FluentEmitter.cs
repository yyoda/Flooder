using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using Flooder.Core.RetryPolicy;
using Flooder.Core.Utility;
using MsgPack.Serialization;
using NLog;

namespace Flooder.Core.Transfer
{
    public class FluentEmitter : IEmitter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly SerializationContext DefaultContext = new SerializationContext();
        private readonly IRetryPolicy _retryPolicy = new FixedInterval(TimeSpan.FromSeconds(1), 3);
        private readonly TcpConnectionStateStore _tcp;

        public FluentEmitter(TcpConnectionStateStore tcp)
        {
            _tcp = tcp;
        }

        public void Emit(string tag, IDictionary<string, object> payload)
        {
            if (!_tcp.HasConnection) return; //skip.

            var timestamp = DateTime.Now.ToUnixTime();

            using (var ms = new MemoryStream())
            {
                var packer = MsgPack.Packer.Create(ms);

                packer.PackArrayHeader(3); // ["tag", timestamp, obj]
                packer.PackString(tag);
                packer.Pack(timestamp);
                packer.PackMapHeader(payload);

                foreach (var row in payload)
                {
                    packer.PackString(row.Key);

                    var type = row.Value != null ? row.Value.GetType() : typeof (string);

                    new SerializationContext()
                        .GetSerializer(type, DefaultContext)
                        .PackTo(packer, row.Value);
                }

                var arraySegment = new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
                var bytes = arraySegment.ToArray();

#if DEBUG
                Logger.Debug("{0} {1} {2}", timestamp, tag, new JavaScriptSerializer().Serialize(payload));
#endif

                Emit(bytes);
            }
        }

        public void Emit(byte[] bytes)
        {
            if (!_tcp.HasConnection) return; //skip.

            while (true)
            {
                TimeSpan interval;

                try
                {
                    _tcp.Transfer(bytes);
                    _retryPolicy.Reset(out interval);
                    break;
                }
                catch (Exception ex)
                {
                    if (_retryPolicy.TryGetNext(out interval))
                    {
                        Logger.WarnException(string.Format("Emit failed. wait for {0} second.", interval.TotalSeconds), ex);
                        Thread.Sleep(interval);
                        continue;
                    }

                    Logger.WarnException("Has encountered a problem, skip.", ex);
                    break;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}

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
        private readonly TcpCore _tcp;

        public FluentEmitter(TcpCore tcp)
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

                    new SerializationContext()
                        .GetSerializer(row.Value.GetType(), DefaultContext)
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
                try
                {
                    _tcp.Transfer(bytes);
                    _retryPolicy.Reset();
                    break;
                }
                catch (Exception ex)
                {
                    TimeSpan interval;
                    if (_retryPolicy.TryGetNext(out interval))
                    {
                        Logger.WarnException(string.Format("emit fails. waiting for {0}sec.", interval.TotalSeconds), ex);
                        Thread.Sleep(interval);
                        continue;
                    }

                    Logger.WarnException("give up the emit.", ex);
                    break;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}

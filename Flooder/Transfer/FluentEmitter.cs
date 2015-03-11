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

namespace Flooder.Transfer
{
    public class FluentEmitter : IEmitter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly SerializationContext MsgPackDefaultContext = new SerializationContext();
        private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        private readonly IRetryPolicy _retryPolicy;
        public TcpConnectionManager Tcp { get; private set; }

        public FluentEmitter(TcpConnectionManager tcp)
        {
            _retryPolicy = new FixedInterval(TimeSpan.FromSeconds(1), 3);
            Tcp = tcp;
        }

        public void Emit(string tag, IDictionary<string, object> payload)
        {
            if (!Tcp.HasConnection) return; //skip.

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

                    new SerializationContext()
                        .GetSerializer(type, MsgPackDefaultContext)
                        .PackTo(packer, column.Value);
                }

                var arraySegment = new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
                var bytes = arraySegment.ToArray();

#if DEBUG
                Logger.Trace("{0} {1} {2}", timestamp, tag, JsonSerializer.Serialize(payload));
#endif

                Emit(bytes);
            }
        }

        public void Emit(byte[] bytes)
        {
            if (!Tcp.HasConnection) return; //skip.

            while (true)
            {
                TimeSpan interval;

                try
                {
                    Tcp.Transfer(bytes);
                    _retryPolicy.Reset(out interval);
                    break;
                }
                catch (Exception ex)
                {
                    if (_retryPolicy.TryGetNext(out interval))
                    {
                        Logger.WarnException(string.Format("The failed because the Emit rerun it waits {0} seconds.", interval.TotalSeconds), ex);
                        Thread.Sleep(interval);
                        continue;
                    }

                    Logger.WarnException("Has encountered a problem, skip.", ex);
                    break;
                }
            }
        }
    }
}

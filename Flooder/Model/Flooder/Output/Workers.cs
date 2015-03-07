using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Core.Transfer;

namespace Flooder.Model.Output
{
    public class Workers
    {
        public Workers(string type, IEnumerable<Worker> details)
        {
            Type    = type;
            Details = details;

            var hosts = Details.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();
            Connection = new TcpConnectionManager(hosts);
            Emitter = CreateEmitter();
        }

        public string Type { get; private set; }
        public IEnumerable<Worker> Details { get; private set; }
        public IEmitter Emitter { get; private set; }
        public TcpConnectionManager Connection { get; private set; }

        private IEmitter CreateEmitter()
        {
            switch (Type)
            {
                case "fluentd":
                    return new FluentEmitter(Connection);
                default:
                    throw new NullReferenceException(string.Format("Type[{0}] is not found.", Type));
            }
        }

        public class Worker
        {
            public Worker(string host, int port)
            {
                Host = host;
                Port = port;
            }

            public string Host { get; private set; }
            public int Port { get; private set; }
        }
    }
}

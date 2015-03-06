using Flooder.Core.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.Core.Settings.Out
{
    public class WorkerSettings
    {
        public WorkerSettings(string type, IEnumerable<WorkerSettingsDetail> details)
        {
            Type       = type;
            Details    = details;

            var hosts = Details.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();
            Connection = new TcpConnectionManager(hosts);
            Emitter = GetEmitter();
        }

        public string Type { get; private set; }
        public IEnumerable<WorkerSettingsDetail> Details { get; private set; }
        public IEmitter Emitter { get; private set; }
        public TcpConnectionManager Connection { get; private set; }

        private IEmitter GetEmitter()
        {
            switch (Type)
            {
                case "fluentd":
                    return new FluentEmitter(Connection);
                default:
                    throw new NullReferenceException(string.Format("Type[{0}] is not found.", Type));
            }
        }

        public class WorkerSettingsDetail
        {
            public WorkerSettingsDetail(string host, int port)
            {
                Host = host;
                Port = port;
            }

            public string Host { get; private set; }
            public int Port { get; private set; }
        }
    }
}

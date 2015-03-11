using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.Transfer
{
    public class Worker
    {
        public Worker(string type, IEnumerable<WorkerDetail> details)
        {
            Type    = type;
            Details = details;

            switch (Type)
            {
                case "fluentd":
                    var hosts = Details.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();
                    Connection = new TcpConnectionManager(hosts);
                    Emitter = new FluentEmitter(Connection);
                    break;
                default:
                    throw new NullReferenceException(string.Format("Type[{0}] is not found.", Type));
            }
        }

        public string Type { get; private set; }
        public IEnumerable<WorkerDetail> Details { get; private set; }
        public IEmitter Emitter { get; private set; }
        public TcpConnectionManager Connection { get; private set; }
    }

    public class WorkerDetail
    {
        public WorkerDetail(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string Host { get; private set; }
        public int Port { get; private set; }
    }
}

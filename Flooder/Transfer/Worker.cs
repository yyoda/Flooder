using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Event;

namespace Flooder.Transfer
{
    public class Worker
    {
        private readonly TcpManager _tcp;

        public IMessageBroker MessageBroker { get; private set; }

        public Worker(string type, IEnumerable<WorkerDetail> details)
        {
            _tcp  = new TcpManager(details.Select(x => Tuple.Create(x.Host, x.Port)));

            switch (type)
            {
                case "fluentd":
                    MessageBroker = new FluentMessageBroker(_tcp, TimeSpan.FromSeconds(1), 3);
                    break;
                default:
                    throw new NullReferenceException(string.Format("Type[{0}] is not found.", type));
            }
        }

        public IEnumerable<IDisposable> Subscribe()
        {
            var instances = new List<IDisposable>();

            if (_tcp.Connect())
            {
                instances.Add(_tcp.HealthCheck());
            }

            instances.Add(MessageBroker.Subscribe());

            return instances;
        }
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


using System.Collections.Generic;

namespace Flooder.Core.Settings.Out
{
    public class WorkerSettings
    {
        public WorkerSettings(string type, IEnumerable<WorkerSettingsDetail> details)
        {
            Type = type;
            Details = details;
        }

        public string Type { get; private set; }
        public IEnumerable<WorkerSettingsDetail> Details { get; private set; }

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

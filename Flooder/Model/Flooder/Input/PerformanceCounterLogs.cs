
using System.Collections.Generic;

namespace Flooder.Model.Flooder.Input
{
    public class PerformanceCounterLogs
    {
        public PerformanceCounterLogs(string tag, int interval, IEnumerable<PerformanceCounterLog> details)
        {
            Tag      = tag;
            Interval = interval;
            Details  = details;
        }

        public string Tag { get; private set; }
        public int Interval { get; private set; }
        public IEnumerable<PerformanceCounterLog> Details { get; private set; }

        public class PerformanceCounterLog
        {
            public PerformanceCounterLog(string categoryName, string counterName, string instanceName)
            {
                CategoryName = categoryName;
                CounterName  = counterName;
                InstanceName = instanceName;
            }

            public string CategoryName { get; private set; }
            public string CounterName { get; private set; }
            public string InstanceName { get; private set; }
        }
    }
}

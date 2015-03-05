
using System.Collections.Generic;

namespace Flooder.Core.Settings.In
{
    public class PerformanceCounterSettings
    {
        public PerformanceCounterSettings(string tag, int interval, IEnumerable<PerformanceCounterSettingsDetail> details)
        {
            Tag      = tag;
            Interval = interval;
            Details  = details;
        }

        public string Tag { get; private set; }
        public int Interval { get; private set; }
        public IEnumerable<PerformanceCounterSettingsDetail> Details { get; private set; }

        public class PerformanceCounterSettingsDetail
        {
            public PerformanceCounterSettingsDetail(string categoryName, string counterName, string instanceName)
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

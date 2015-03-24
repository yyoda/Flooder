
using System.Collections.Generic;

namespace Flooder.Event.PerformanceCounter
{
    public class PerformanceCounterDataSource : IDataSource
    {
        public PerformanceCounterDataSource()
        {
            Details = new PerformanceCounterDataSourceDetail[0];
        }

        public PerformanceCounterDataSource(string tag, int interval, IEnumerable<PerformanceCounterDataSourceDetail> details)
        {
            Tag      = tag;
            Interval = interval;
            Details  = details;
        }

        public string Tag { get; private set; }
        public int Interval { get; private set; }
        public IEnumerable<PerformanceCounterDataSourceDetail> Details { get; private set; }
    }

    public class PerformanceCounterDataSourceDetail
    {
        public PerformanceCounterDataSourceDetail(string categoryName, string counterName, string instanceName)
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

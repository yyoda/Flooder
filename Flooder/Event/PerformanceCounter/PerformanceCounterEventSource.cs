
using System.Collections.Generic;

namespace Flooder.Event.PerformanceCounter
{
    public class PerformanceCounterEventSource : IEventSource
    {
        public PerformanceCounterEventSource()
        {
            Details = new[] {new PerformanceCounterEventSourceDetail(),};
        }

        public PerformanceCounterEventSource(string tag, int interval, IEnumerable<PerformanceCounterEventSourceDetail> details)
        {
            Tag      = tag;
            Interval = interval;
            Details  = details;
        }

        public string Tag { get; private set; }
        public int Interval { get; private set; }
        public IEnumerable<PerformanceCounterEventSourceDetail> Details { get; private set; }
    }

    public class PerformanceCounterEventSourceDetail
    {
        public PerformanceCounterEventSourceDetail()
        {
        }

        public PerformanceCounterEventSourceDetail(string categoryName, string counterName, string instanceName)
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

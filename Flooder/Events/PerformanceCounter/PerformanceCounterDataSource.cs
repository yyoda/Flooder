
using System.Collections.Generic;

namespace Flooder.Events.PerformanceCounter
{
    public class PerformanceCounterDataSource : IDataSource
    {
        public PerformanceCounterDataSource()
        {
            Options = new PerformanceCounterDataSourceOption[0];
        }

        public PerformanceCounterDataSource(string tag, int interval, IEnumerable<PerformanceCounterDataSourceOption> options)
        {
            Tag      = tag;
            Interval = interval;
            Options  = options;
        }

        public string Tag { get; private set; }
        public int Interval { get; private set; }
        public IEnumerable<PerformanceCounterDataSourceOption> Options { get; private set; }
    }

    public class PerformanceCounterDataSourceOption
    {
        public PerformanceCounterDataSourceOption(string categoryName, string counterName, string instanceName)
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

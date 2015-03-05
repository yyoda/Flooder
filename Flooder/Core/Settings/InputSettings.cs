using Flooder.Core.Settings.In;

namespace Flooder.Core.Settings
{
    public class InputSettings
    {
        public InputSettings(FileSystemSettings fileSystems, IISSettings iis, EventLogSettings eventLogs, PerformanceCounterSettings performanceCounters)
        {
            FileSystems         = fileSystems;
            IIS                 = iis;
            EventLogs           = eventLogs;
            PerformanceCounters = performanceCounters;
        }

        public FileSystemSettings FileSystems { get; set; }
        public IISSettings IIS { get; set; }
        public EventLogSettings EventLogs { get; private set; }
        public PerformanceCounterSettings PerformanceCounters { get; private set; }
    }
}

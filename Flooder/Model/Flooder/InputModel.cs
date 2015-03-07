using Flooder.Model.Input;

namespace Flooder.Model
{
    public class InputModel
    {
        public InputModel(FileSystemLogs fileSystem, IISLogs iis, EventLogs eventLog, PerformanceCounterLogs performanceCounter)
        {
            FileSystem         = fileSystem;
            IIS                = iis;
            EventLog           = eventLog;
            PerformanceCounter = performanceCounter;
        }

        public FileSystemLogs FileSystem { get; set; }
        public IISLogs IIS { get; set; }
        public EventLogs EventLog { get; private set; }
        public PerformanceCounterLogs PerformanceCounter { get; private set; }
    }
}

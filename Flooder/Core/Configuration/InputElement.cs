using System.Configuration;
using Flooder.Core.Configuration.In;

namespace Flooder.Core.Configuration
{
    public class InputElement : ConfigurationElement
    {
        [ConfigurationProperty("fileSystems", IsRequired = false)]
        [ConfigurationCollection(typeof(FileSystemElementCollection))]
        public FileSystemElementCollection FileSystems { get { return (FileSystemElementCollection)base["fileSystems"]; } }

        [ConfigurationProperty("eventLogs", IsRequired = false)]
        [ConfigurationCollection(typeof(EventLogElementCollection))]
        public EventLogElementCollection EventLogs { get { return (EventLogElementCollection)base["eventLogs"]; } }

        [ConfigurationProperty("performanceCounters", IsRequired = false)]
        [ConfigurationCollection(typeof(PerformanceCounterElementCollection))]
        public PerformanceCounterElementCollection PerformanceCounters { get { return (PerformanceCounterElementCollection)base["performanceCounters"]; } }
    }
}

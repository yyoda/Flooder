using System.Configuration;
using Flooder.Configuration.EventElements;

namespace Flooder.Configuration
{
    public class EventElement : ConfigurationElement
    {
        [ConfigurationProperty("fileSystems", IsRequired = false)]
        [ConfigurationCollection(typeof(FileSystemElementCollection))]
        public FileSystemElementCollection FileSystems { get { return (FileSystemElementCollection)base["fileSystems"]; } }

        [ConfigurationProperty("iis", IsRequired = false)]
        [ConfigurationCollection(typeof(IISElementCollection))]
        public IISElementCollection IIS { get { return (IISElementCollection)base["iis"]; } }

        [ConfigurationProperty("eventLogs", IsRequired = false)]
        [ConfigurationCollection(typeof(EventLogElementCollection))]
        public EventLogElementCollection EventLogs { get { return (EventLogElementCollection)base["eventLogs"]; } }

        [ConfigurationProperty("performanceCounters", IsRequired = false)]
        [ConfigurationCollection(typeof(PerformanceCounterElementCollection))]
        public PerformanceCounterElementCollection PerformanceCounters { get { return (PerformanceCounterElementCollection)base["performanceCounters"]; } }
    }
}

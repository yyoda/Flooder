using System.Configuration;

namespace Flooder.Configuration
{
    public class Section : ConfigurationSection
    {
        [ConfigurationProperty("event", IsRequired = true)]
        public EventElement Event { get { return (EventElement)base["event"]; } }

        [ConfigurationProperty("worker", IsRequired = true)]
        [ConfigurationCollection(typeof(WokerElementCollection))]
        public WokerElementCollection Worker { get { return (WokerElementCollection)base["worker"]; } }
    }
}

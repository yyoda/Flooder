using System.Configuration;
using Flooder.Configuration.Out;

namespace Flooder.Configuration
{
    public class OutputElement : ConfigurationElement
    {
        [ConfigurationProperty("workers", IsRequired = false)]
        [ConfigurationCollection(typeof(WokerElementCollection))]
        public WokerElementCollection Workers { get { return (WokerElementCollection)base["workers"]; } }
    }
}

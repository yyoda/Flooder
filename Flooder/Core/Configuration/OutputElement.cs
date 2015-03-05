using System.Configuration;
using Flooder.Core.Configuration.Out;

namespace Flooder.Core.Configuration
{
    public class OutputElement : ConfigurationElement
    {
        [ConfigurationProperty("workers", IsRequired = false)]
        [ConfigurationCollection(typeof(WokerElementCollection))]
        public WokerElementCollection Workers { get { return (WokerElementCollection)base["workers"]; } }
    }
}

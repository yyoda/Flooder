using System.Configuration;
using Flooder.Core.Configuration.Out;

namespace Flooder.Core.Configuration
{
    public class OutputElement : ConfigurationElement
    {
        [ConfigurationProperty("wokers", IsRequired = false)]
        [ConfigurationCollection(typeof(WokerElementCollection))]
        public WokerElementCollection Wokers { get { return (WokerElementCollection)base["wokers"]; } }
    }
}

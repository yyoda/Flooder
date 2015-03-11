using System.Configuration;

namespace Flooder.Configuration
{
    public class Section : ConfigurationSection
    {
        [ConfigurationProperty("in", IsRequired = true)]
        public InputElement In { get { return (InputElement)base["in"]; } }

        [ConfigurationProperty("out", IsRequired = true)]
        public OutputElement Out { get { return (OutputElement)base["out"]; } }
    }
}

using System.Configuration;

namespace Flooder.Core.Configuration
{
    public class Section : ConfigurationSection
    {
        [ConfigurationProperty("in", IsRequired = true)]
        public InputElement In { get { return (InputElement)base["in"]; } }

        [ConfigurationProperty("out", IsRequired = true)]
        public OutputElement Out { get { return (OutputElement)base["out"]; } }

        public static Section GetSectionFromAppConfig()
        {
            return ConfigurationManager.GetSection("flooder") as Section;
        }
    }
}

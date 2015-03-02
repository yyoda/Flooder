using System;
using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Core.Configuration.Out
{
    public class WokerElement : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host { get { return (string)base["host"]; } }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port { get { return (int)base["port"]; } }
    }

    [ConfigurationCollection(typeof(WokerElement))]
    public class WokerElementCollection : ConfigurationElementCollection, IEnumerable<WokerElement>
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type { get { return (string)base["type"]; } }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var elem = (WokerElement)element;
            return Tuple.Create(elem.Host, elem.Port);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WokerElement();
        }

        public new IEnumerator<WokerElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (WokerElement)e.Current;
            }
        }
    }
}

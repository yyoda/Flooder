using System;
using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Configuration.EventElements
{
    [ConfigurationCollection(typeof(EventLogElement))]
    public class IISElementCollection : ConfigurationElementCollection, IEnumerable<IISElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new IISElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var elem = (IISElement)element;
            return Tuple.Create(elem.Tag, elem.Path);
        }

        public new IEnumerator<IISElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (IISElement)e.Current;
            }
        }
    }

    public class IISElement : ConfigurationElement
    {
        [ConfigurationProperty("tag", IsRequired = true)]
        public string Tag { get { return (string)base["tag"]; } }

        [ConfigurationProperty("path", IsRequired = true)]
        public string Path { get { return (string)base["path"]; } }

        [ConfigurationProperty("interval", IsRequired = false, DefaultValue = 1)]
        public int Interval { get { return (int)base["interval"]; } }
    }
}

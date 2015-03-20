using System;
using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Configuration.EventElements
{
    [ConfigurationCollection(typeof(PerformanceCounterElement))]
    public class PerformanceCounterElementCollection : ConfigurationElementCollection, IEnumerable<PerformanceCounterElement>
    {
        [ConfigurationProperty("tag", IsRequired = true)]
        public string Tag { get { return (string)base["tag"]; } }

        [ConfigurationProperty("interval", IsRequired = false, DefaultValue = 15)]
        public int Interval { get { return (int)base["interval"]; } }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PerformanceCounterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var elem = (PerformanceCounterElement)element;
            return Tuple.Create(elem.CategoryName, elem.CounterName, elem.InstanceName);
        }

        public new IEnumerator<PerformanceCounterElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (PerformanceCounterElement)e.Current;
            }
        }
    }

    public class PerformanceCounterElement : ConfigurationElement
    {
        [ConfigurationProperty("categoryName", IsRequired = true)]
        public string CategoryName { get { return (string)base["categoryName"]; } }

        [ConfigurationProperty("counterName", IsRequired = true)]
        public string CounterName { get { return (string)base["counterName"]; } }

        [ConfigurationProperty("instanceName", IsRequired = false, DefaultValue = "")]
        public string InstanceName { get { return (string)base["instanceName"]; } }
    }
}

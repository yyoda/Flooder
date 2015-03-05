using System;
using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Core.Configuration.Out
{
    public class WorkerElement : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host { get { return (string)base["host"]; } }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port { get { return (int)base["port"]; } }
    }

    [ConfigurationCollection(typeof(WorkerElement))]
    public class WokerElementCollection : ConfigurationElementCollection, IEnumerable<WorkerElement>
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type { get { return (string)base["type"]; } }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var elem = (WorkerElement)element;
            return Tuple.Create(elem.Host, elem.Port);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WorkerElement();
        }

        public new IEnumerator<WorkerElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (WorkerElement)e.Current;
            }
        }
    }
}

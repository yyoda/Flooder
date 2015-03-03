using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Flooder.Core.Configuration.In
{
    public class EventLogElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type { get { return (string)base["type"]; } }

        [ConfigurationProperty("mode", IsRequired = true)]
        public string Mode { get { return (string)base["mode"]; } }

        [ConfigurationProperty("source", IsRequired = true)]
        public string Source { get { return (string)base["source"]; } }

        [ConfigurationProperty("id", IsRequired = true)]
        public string Id { get { return (string)base["id"]; } }
    }

    [ConfigurationCollection(typeof(EventLogElement))]
    public class EventLogElementCollection : ConfigurationElementCollection, IEnumerable<EventLogElement>
    {
        [ConfigurationProperty("tag", IsRequired = true)]
        public string Tag { get { return (string)base["tag"]; } }

        [ConfigurationProperty("scopes", IsRequired = false, DefaultValue = "Application,System")]
        private string ScopesString { get { return (string)base["scopes"]; } }
        public string[] Scopes { get { return ScopesString.Split(','); } }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EventLogElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var elem = (EventLogElement)element;
            return Tuple.Create(elem.Type, elem.Mode, elem.Source, elem.Id);
        }

        public new IEnumerator<EventLogElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (EventLogElement)e.Current;
            }
        }

        public IEnumerable<EventLogElement> GetIncludeInfo()
        {
            return this.Where(x => x.Mode == "include" && x.Type == "info");
        }

        public IEnumerable<EventLogElement> GetIncludeWarn()
        {
            return this.Where(x => x.Mode == "include" && x.Type == "warn");
        }

        public IEnumerable<EventLogElement> GetIncludeError()
        {
            return this.Where(x => x.Mode == "include" && x.Type == "error");
        }

        public IEnumerable<EventLogElement> GetExcludeInfo()
        {
            return this.Where(x => x.Mode == "exclude" && x.Type == "info");
        }

        public IEnumerable<EventLogElement> GetExcludeWarn()
        {
            return this.Where(x => x.Mode == "exclude" && x.Type == "warn");
        }

        public IEnumerable<EventLogElement> GetExcludeError()
        {
            return this.Where(x => x.Mode == "exclude" && x.Type == "error");
        }
    }
}

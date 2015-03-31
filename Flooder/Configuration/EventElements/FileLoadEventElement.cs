using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Configuration.EventElements
{
    [ConfigurationCollection(typeof(FileLoadElement))]
    public class FileLoadElementCollection : ConfigurationElementCollection, IEnumerable<FileLoadElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileLoadElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileLoadElement)element).Path;
        }

        public new IEnumerator<FileLoadElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (FileLoadElement)e.Current;
            }
        }
    }

    public class FileLoadElement : ConfigurationElement
    {
        [ConfigurationProperty("tag", IsRequired = true)]
        public string Tag { get { return (string)base["tag"]; } }

        [ConfigurationProperty("path", IsRequired = true)]
        public string Path { get { return (string)base["path"]; } }

        [ConfigurationProperty("file", IsRequired = false, DefaultValue = "*")]
        public string File { get { return (string)base["file"]; } }

        [ConfigurationProperty("listener", IsRequired = false, DefaultValue = "")]
        public string Listener { get { return (string)base["listener"]; } }

        [ConfigurationProperty("parser", IsRequired = false, DefaultValue = "")]
        public string Parser { get { return (string)base["parser"]; } }

        [ConfigurationProperty("interval", IsRequired = false, DefaultValue = 1)]
        public int Interval { get { return (int)base["interval"]; } }
    }
}

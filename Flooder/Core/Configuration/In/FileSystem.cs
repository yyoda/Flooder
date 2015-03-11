using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Core.Configuration.In
{
    public class FileSystemElement : ConfigurationElement
    {
        [ConfigurationProperty("tag", IsRequired = true)]
        public string Tag { get { return (string)base["tag"]; } }

        [ConfigurationProperty("path", IsRequired = true)]
        public string Path { get { return (string)base["path"]; } }

        [ConfigurationProperty("file", IsRequired = false, DefaultValue = "*")]
        public string File { get { return (string)base["file"]; } }

        [ConfigurationProperty("payload", IsRequired = false, DefaultValue = "")]
        public string Payload { get { return (string)base["payload"]; } }
    }

    [ConfigurationCollection(typeof(FileSystemElement))]
    public class FileSystemElementCollection : ConfigurationElementCollection, IEnumerable<FileSystemElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FileSystemElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FileSystemElement)element).Path;
        }

        public new IEnumerator<FileSystemElement> GetEnumerator()
        {
            var e = base.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (FileSystemElement)e.Current;
            }
        }
    }
}

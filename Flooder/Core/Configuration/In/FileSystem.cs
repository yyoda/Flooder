using System.Collections.Generic;
using System.Configuration;

namespace Flooder.Core.Configuration.In
{
    public class FileSystemElement : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path { get { return (string)base["path"]; } }
    }

    [ConfigurationCollection(typeof(FileSystemElement))]
    public class FileSystemElementCollection : ConfigurationElementCollection, IEnumerable<FileSystemElement>
    {
        [ConfigurationProperty("tag", IsRequired = true)]
        public string Tag { get { return (string)base["tag"]; } }

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

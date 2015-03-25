
using System.Collections.Generic;

namespace Flooder.Event.IIS
{
    public class IISLogDataSource : IDataSource
    {
        public IISLogDataSource()
        {
            Options = new IISLogDataSourceOption[0];
        }

        public IISLogDataSource(IEnumerable<IISLogDataSourceOption> options)
        {
            Options = options;
        }

        public IEnumerable<IISLogDataSourceOption> Options { get; set; }
    }

    public class IISLogDataSourceOption
    {
        public IISLogDataSourceOption(string tag, string path, int interval)
        {
            Tag      = tag;
            Path     = path;
            Interval = interval;
        }

        public string Tag { get; private set; }
        public string Path { get; private set; }
        public int Interval { get; private set; }
    }
}

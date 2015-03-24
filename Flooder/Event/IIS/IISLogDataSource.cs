
using System.Collections.Generic;

namespace Flooder.Event.IIS
{
    public class IISLogDataSource : IDataSource
    {
        public IISLogDataSource()
        {
            Details = new IISLogDataSourceDetail[0];
        }

        public IISLogDataSource(IEnumerable<IISLogDataSourceDetail> details)
        {
            Details = details;
        }

        public IEnumerable<IISLogDataSourceDetail> Details { get; set; }
    }

    public class IISLogDataSourceDetail
    {
        public IISLogDataSourceDetail(string tag, string path, int interval)
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

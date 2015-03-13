
using System.Collections.Generic;

namespace Flooder.Event.IIS
{
    public class IISLogEventSource : IEventSource
    {
        public IISLogEventSource()
        {
            Details = new IISLogEventSourceDetail[0];
        }

        public IISLogEventSource(IEnumerable<IISLogEventSourceDetail> details)
        {
            Details = details;
        }

        public IEnumerable<IISLogEventSourceDetail> Details { get; set; }
    }

    public class IISLogEventSourceDetail
    {
        public IISLogEventSourceDetail(string tag, string path, int interval)
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

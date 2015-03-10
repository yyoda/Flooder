
using System.Collections.Generic;

namespace Flooder.Model.Flooder.Input
{
    public class IISLogs
    {
        public IISLogs(IEnumerable<IISLog> details)
        {
            Details = details;
        }

        public IEnumerable<IISLog> Details { get; set; }

        public class IISLog
        {
            public IISLog(string tag, string path, int interval)
            {
                Tag  = tag;
                Path = path;
                Interval = interval;
            }

            public string Tag { get; private set; }
            public string Path { get; private set; }
            public int Interval { get; private set; }
        }
    }
}

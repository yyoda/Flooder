
using System.Collections.Generic;

namespace Flooder.Core.Settings.In
{
    public class IISSettings
    {
        public IISSettings(IEnumerable<IISSettingsDetail> details)
        {
            Details = details;
        }

        public IEnumerable<IISSettingsDetail> Details { get; set; }

        public class IISSettingsDetail
        {
            public IISSettingsDetail(string tag, string path, int interval)
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

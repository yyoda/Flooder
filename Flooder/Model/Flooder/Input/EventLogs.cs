using System.Collections.Generic;
using System.Linq;

namespace Flooder.Model.Flooder.Input
{
    public class EventLogs
    {
        public EventLogs(string tag, string[] scopes, IEnumerable<EventLog> details)
        {
            Tag     = tag;
            Scopes  = scopes;
            Details = details;
        }

        public string Tag { get; private set; }
        public string[] Scopes { get; private set; }
        public IEnumerable<EventLog> Details { get; private set; }

        public class EventLog
        {
            public EventLog(string type, string mode, string source, string id)
            {
                Type   = type;
                Mode   = mode;
                Source = source;
                Id     = id;
            }

            public string Type { get; private set; }
            public string Mode { get; private set; }
            public string Source { get; private set; }
            public string Id { get; private set; }

            public override string ToString()
            {
                return string.Format("{{ Type:{0}, Mode:{1}, Source:{2}, Id:{3} }}", Type, Mode, Source, Id);
            }
        }

        public IEnumerable<EventLog> GetIncludeInfo()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "info");
        }

        public IEnumerable<EventLog> GetIncludeWarn()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "warn");
        }

        public IEnumerable<EventLog> GetIncludeError()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "error");
        }

        public IEnumerable<EventLog> GetExcludeInfo()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "info");
        }

        public IEnumerable<EventLog> GetExcludeWarn()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "warn");
        }

        public IEnumerable<EventLog> GetExcludeError()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "error");
        }
    }
}

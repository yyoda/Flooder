using System.Collections.Generic;
using System.Linq;

namespace Flooder.Event.EventLog
{
    public class EventLogDataSource : IDataSource
    {
        public EventLogDataSource()
        {
            Scopes  = new string[0];
            Details = new EventLogDataSourceDetail[0];
        }

        public EventLogDataSource(string tag, string[] scopes, IEnumerable<EventLogDataSourceDetail> details)
        {
            Tag     = tag;
            Scopes  = scopes;
            Details = details;
        }

        public string Tag { get; private set; }
        public string[] Scopes { get; private set; }

        public IEnumerable<EventLogDataSourceDetail> Details { get; private set; }

        public IEnumerable<EventLogDataSourceDetail> GetIncludeInfo()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "info");
        }

        public IEnumerable<EventLogDataSourceDetail> GetIncludeWarn()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "warn");
        }

        public IEnumerable<EventLogDataSourceDetail> GetIncludeError()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "error");
        }

        public IEnumerable<EventLogDataSourceDetail> GetExcludeInfo()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "info");
        }

        public IEnumerable<EventLogDataSourceDetail> GetExcludeWarn()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "warn");
        }

        public IEnumerable<EventLogDataSourceDetail> GetExcludeError()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "error");
        }
    }

    public class EventLogDataSourceDetail
    {
        public EventLogDataSourceDetail(string type, string mode, string source, string id)
        {
            Type = type;
            Mode = mode;
            Source = source;
            Id = id;
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
}

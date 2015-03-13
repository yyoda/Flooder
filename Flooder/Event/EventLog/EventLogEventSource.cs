using System.Collections.Generic;
using System.Linq;

namespace Flooder.Event.EventLog
{
    public class EventLogEventSource : IEventSource
    {
        public EventLogEventSource()
        {
            Scopes  = new string[0];
            Details = new[] {new EventLogEventSourceDetail(),};
        }

        public EventLogEventSource(string tag, string[] scopes, IEnumerable<EventLogEventSourceDetail> details)
        {
            Tag     = tag;
            Scopes  = scopes;
            Details = details;
        }

        public string Tag { get; private set; }
        public string[] Scopes { get; private set; }

        public IEnumerable<EventLogEventSourceDetail> Details { get; private set; }

        public IEnumerable<EventLogEventSourceDetail> GetIncludeInfo()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "info");
        }

        public IEnumerable<EventLogEventSourceDetail> GetIncludeWarn()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "warn");
        }

        public IEnumerable<EventLogEventSourceDetail> GetIncludeError()
        {
            return Details.Where(x => x.Mode == "include" && x.Type == "error");
        }

        public IEnumerable<EventLogEventSourceDetail> GetExcludeInfo()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "info");
        }

        public IEnumerable<EventLogEventSourceDetail> GetExcludeWarn()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "warn");
        }

        public IEnumerable<EventLogEventSourceDetail> GetExcludeError()
        {
            return Details.Where(x => x.Mode == "exclude" && x.Type == "error");
        }
    }

    public class EventLogEventSourceDetail
    {
        public EventLogEventSourceDetail()
        {
        }

        public EventLogEventSourceDetail(string type, string mode, string source, string id)
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

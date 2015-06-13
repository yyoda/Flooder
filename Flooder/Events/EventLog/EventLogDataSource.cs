using System.Collections.Generic;
using System.Linq;

namespace Flooder.Events.EventLog
{
    public class EventLogDataSource : IDataSource
    {
        public EventLogDataSource()
        {
            Scopes  = new string[0];
            Options = new EventLogDataSourceOption[0];
        }

        public EventLogDataSource(string tag, string[] scopes, IEnumerable<EventLogDataSourceOption> options)
        {
            Tag     = tag;
            Scopes  = scopes;
            Options = options;
        }

        public string Tag { get; private set; }
        public string[] Scopes { get; private set; }

        public IEnumerable<EventLogDataSourceOption> Options { get; private set; }

        public IEnumerable<EventLogDataSourceOption> GetIncludeInfo()
        {
            return Options.Where(x => x.Mode == "include" && x.Type == "info");
        }

        public IEnumerable<EventLogDataSourceOption> GetIncludeWarn()
        {
            return Options.Where(x => x.Mode == "include" && x.Type == "warn");
        }

        public IEnumerable<EventLogDataSourceOption> GetIncludeError()
        {
            return Options.Where(x => x.Mode == "include" && x.Type == "error");
        }

        public IEnumerable<EventLogDataSourceOption> GetExcludeInfo()
        {
            return Options.Where(x => x.Mode == "exclude" && x.Type == "info");
        }

        public IEnumerable<EventLogDataSourceOption> GetExcludeWarn()
        {
            return Options.Where(x => x.Mode == "exclude" && x.Type == "warn");
        }

        public IEnumerable<EventLogDataSourceOption> GetExcludeError()
        {
            return Options.Where(x => x.Mode == "exclude" && x.Type == "error");
        }
    }

    public class EventLogDataSourceOption
    {
        public EventLogDataSourceOption(string type, string mode, string source, string id)
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

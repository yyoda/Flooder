using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.EventLog
{
    public class EventLogToServerEvent : EventBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public EventLogToServerEvent(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as EventLogDataSource ?? new EventLogDataSource();

            return source.Scopes.Select(scope =>
            {
                var includeInfo  = source.GetIncludeInfo().ToArray();
                var includeWarn  = source.GetIncludeWarn().ToArray();
                var includeError = source.GetIncludeError().ToArray();
                var excludeInfo  = source.GetExcludeInfo().ToArray();
                var excludeWarn  = source.GetExcludeWarn().ToArray();
                var excludeError = source.GetExcludeError().ToArray();

                var observer = new EventLogEventListener(source.Tag, base.MessageBroker)
                {
                    IncludeInfo  = new HashSet<Tuple<string, string>>(includeInfo.Select(x => Tuple.Create(x.Source, x.Id))),
                    IncludeWarn  = new HashSet<Tuple<string, string>>(includeWarn.Select(x => Tuple.Create(x.Source, x.Id))),
                    IncludeError = new HashSet<Tuple<string, string>>(includeError.Select(x => Tuple.Create(x.Source, x.Id))),
                    ExcludeInfo  = new HashSet<Tuple<string, string>>(excludeInfo.Select(x => Tuple.Create(x.Source, x.Id))),
                    ExcludeWarn  = new HashSet<Tuple<string, string>>(excludeWarn.Select(x => Tuple.Create(x.Source, x.Id))),
                    ExcludeError = new HashSet<Tuple<string, string>>(excludeError.Select(x => Tuple.Create(x.Source, x.Id))),
                };

                var subscribe = new System.Diagnostics.EventLog
                {
                    Log = scope,
                    EnableRaisingEvents = true
                }
                .EntryWrittenAsObservable()
                .Subscribe(observer);

                Logger.Info("EventLogListener start. tag:{0}, scope:{1}", source.Tag, scope);
                Logger.Trace("EventLogListener IncludeInfo:[{0}], IncludeWarn:[{1}], IncludeError:[{2}], ExcludeInfo:[{3}], ExcludeWarn:[{4}], ExcludeError:[{5}]",
                    string.Join(",", includeInfo.Select(x => x.ToString())),
                    string.Join(",", includeWarn.Select(x => x.ToString())),
                    string.Join(",", includeError.Select(x => x.ToString())),
                    string.Join(",", excludeInfo.Select(x => x.ToString())),
                    string.Join(",", excludeWarn.Select(x => x.ToString())),
                    string.Join(",", excludeError.Select(x => x.ToString()))
                );

                return subscribe;
            })
            .ToArray();
        }
    }
}

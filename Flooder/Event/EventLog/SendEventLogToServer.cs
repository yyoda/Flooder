using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Model;
using NLog;

namespace Flooder.Event.EventLog
{
    public class SendEventLogToServer : SendEventSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly EventLogEventSource _eventSource;

        public SendEventLogToServer(FlooderObject obj) : base(obj)
        {
            _eventSource = obj.Events.OfType<EventLogEventSource>().First();
        }

        public override IDisposable[] Subscribe()
        {
            if (_eventSource.Scopes.Any())
            {
                return _eventSource.Scopes.Select(scope =>
                {
                    var includeInfo  = _eventSource.GetIncludeInfo().ToArray();
                    var includeWarn  = _eventSource.GetIncludeWarn().ToArray();
                    var includeError = _eventSource.GetIncludeError().ToArray();
                    var excludeInfo  = _eventSource.GetExcludeInfo().ToArray();
                    var excludeWarn  = _eventSource.GetExcludeWarn().ToArray();
                    var excludeError = _eventSource.GetExcludeError().ToArray();

                    var observer = new EventLogEventListener(_eventSource.Tag, base.FlooderObject)
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

                    Logger.Info("EventLogListener start. tag:{0}, scope:{1}", _eventSource.Tag, scope);
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

            return new IDisposable[0];
        }
    }
}

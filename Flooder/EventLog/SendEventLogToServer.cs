using Flooder.Core.Configuration.In;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Flooder.Core.Settings;
using Flooder.Core.Settings.In;

namespace Flooder.EventLog
{
    public class SendEventLogToServer : IObservable<EntryWrittenEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _logName;

        public SendEventLogToServer(string logName)
        {
            _logName = logName;
        }

        public IDisposable Subscribe(IObserver<EntryWrittenEventArgs> observer)
        {
            return new System.Diagnostics.EventLog
            {
                Log                 = _logName,
                EnableRaisingEvents = true
            }
            .EntryWrittenAsObservable()
            .Subscribe(observer);
        }
 
        public static IDisposable[] Start(Settings settings, IEmitter emitter)
        {
            var eventLog = settings.In.EventLogs;

            if (eventLog.Scopes.Any())
            {
                return eventLog.Scopes.Select(scope =>
                {
                    var tag = eventLog.Tag;
                    var subject = new SendEventLogToServer(scope);

                    var includeInfo  = eventLog.GetIncludeInfo().ToArray();
                    var includeWarn  = eventLog.GetIncludeWarn().ToArray();
                    var includeError = eventLog.GetIncludeError().ToArray();
                    var excludeInfo  = eventLog.GetExcludeInfo().ToArray();
                    var excludeWarn  = eventLog.GetExcludeWarn().ToArray();
                    var excludeError = eventLog.GetExcludeError().ToArray();

                    var subscribe = subject.Subscribe(new EventLogListener(tag, emitter)
                    {
                        IncludeInfo  = new HashSet<Tuple<string, string>>(includeInfo.Select(x => Tuple.Create(x.Source, x.Id))),
                        IncludeWarn  = new HashSet<Tuple<string, string>>(includeWarn.Select(x => Tuple.Create(x.Source, x.Id))),
                        IncludeError = new HashSet<Tuple<string, string>>(includeError.Select(x => Tuple.Create(x.Source, x.Id))),
                        ExcludeInfo  = new HashSet<Tuple<string, string>>(excludeInfo.Select(x => Tuple.Create(x.Source, x.Id))),
                        ExcludeWarn  = new HashSet<Tuple<string, string>>(excludeWarn.Select(x => Tuple.Create(x.Source, x.Id))),
                        ExcludeError = new HashSet<Tuple<string, string>>(excludeError.Select(x => Tuple.Create(x.Source, x.Id))),
                    });

                    Logger.Info("EventLogListener start. tag:{0}", tag);
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

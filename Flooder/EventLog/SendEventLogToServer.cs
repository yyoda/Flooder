using Flooder.Core.Configuration.In;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

         public static IDisposable[] Start(EventLogElementCollection config, IEmitter emitter)
        {
            if (config.Any())
            {
                return config.Scopes.Select(scope =>
                {
                    var tag = config.Tag;
                    var subject = new SendEventLogToServer(scope);

                    var includeInfo  = config.GetIncludeInfo().Select(x => Tuple.Create(x.Source, x.Id)).ToArray();
                    var includeWarn  = config.GetIncludeWarn().Select(x => Tuple.Create(x.Source, x.Id)).ToArray();
                    var includeError = config.GetIncludeError().Select(x => Tuple.Create(x.Source, x.Id)).ToArray();
                    var excludeInfo  = config.GetExcludeInfo().Select(x => Tuple.Create(x.Source, x.Id)).ToArray();
                    var excludeWarn  = config.GetExcludeWarn().Select(x => Tuple.Create(x.Source, x.Id)).ToArray();
                    var excludeError = config.GetExcludeError().Select(x => Tuple.Create(x.Source, x.Id)).ToArray();

                    var subscribe = subject.Subscribe(new EventLogListener(tag, emitter)
                    {
                        IncludeInfo  = new HashSet<Tuple<string, string>>(includeInfo),
                        IncludeWarn  = new HashSet<Tuple<string, string>>(includeWarn),
                        IncludeError = new HashSet<Tuple<string, string>>(includeError),
                        ExcludeInfo  = new HashSet<Tuple<string, string>>(excludeInfo),
                        ExcludeWarn  = new HashSet<Tuple<string, string>>(excludeWarn),
                        ExcludeError = new HashSet<Tuple<string, string>>(excludeError),
                    });

                    Logger.Info("EventLogListener start. tag:{0}", tag);
                    Logger.Debug("EventLogListener IncludeInfo:[{0}], IncludeWarn:[{1}], IncludeError:[{2}], ExcludeInfo:[{3}], ExcludeWarn:[{4}], ExcludeError:[{5}]",
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

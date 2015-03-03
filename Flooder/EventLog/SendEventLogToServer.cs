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
                    var subscribe = subject.Subscribe(new EventLogListener(tag, emitter)
                    {
                        TrapInfomations = new HashSet<Tuple<string, string>>(config.GetIncludeInfo().Select(x => Tuple.Create(x.Source, x.Id))),
                        TrapWarnings    = new HashSet<Tuple<string, string>>(config.GetIncludeWarn().Select(x => Tuple.Create(x.Source, x.Id))),
                        SkipErrors      = new HashSet<Tuple<string, string>>(config.GetExcludeError().Select(x => Tuple.Create(x.Source, x.Id))),
                    });

                    Logger.Info("EventLogListener start. tag:{0}", tag);
                    return subscribe;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }
    }
}

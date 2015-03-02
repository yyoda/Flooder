using Flooder.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Flooder.Core.Configuration;
using Flooder.Core.Configuration.In;
using Flooder.Core.Transfer;
using Flooder.FileSystem;
using NLog;

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
            return new System.Diagnostics.EventLog(_logName)
                .EntryWrittenAsObservable()
                .Subscribe(observer);
        }

        public static IDisposable[] Start(EventLogElementCollection config, IEmitter emitter)
        {
            if (config.Any())
            {
                return config.Scopes.Select(scope =>
                {
                    var subject = new SendEventLogToServer(scope);
                    return subject.Subscribe(new EventLogListener(config.Tag, emitter)
                    {
                        TrapInfomations = new HashSet<Tuple<string, string>>(config.GetTrapInfomations().Select(x => Tuple.Create(x.Source, x.Id))),
                        TrapWarnings    = new HashSet<Tuple<string, string>>(config.GetTrapWarnings().Select(x => Tuple.Create(x.Source, x.Id))),
                        SkipErrors      = new HashSet<Tuple<string, string>>(config.GetSkipErrors().Select(x => Tuple.Create(x.Source, x.Id))),
                    });
                })
                .ToArray();
            }

            return new IDisposable[0];
        }
    }
}

using Flooder.Core;
using Flooder.Core.Settings;
using Flooder.Core.Settings.In;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.EventLog
{
    public class SendEventLogToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly EventLogSettings _settings;
        private readonly IEmitter _emitter;

        public SendEventLogToServer(Settings settings)
        {
            _settings = settings.In.EventLogs;
            _emitter  = settings.Out.Worker.Emitter;
        }

        public IDisposable[] Subscribe()
        {
            if (_settings.Scopes.Any())
            {
                return _settings.Scopes.Select(scope =>
                {
                    var includeInfo  = _settings.GetIncludeInfo().ToArray();
                    var includeWarn  = _settings.GetIncludeWarn().ToArray();
                    var includeError = _settings.GetIncludeError().ToArray();
                    var excludeInfo  = _settings.GetExcludeInfo().ToArray();
                    var excludeWarn  = _settings.GetExcludeWarn().ToArray();
                    var excludeError = _settings.GetExcludeError().ToArray();

                    var observer = new EventLogListener(_settings.Tag, _emitter)
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

                    Logger.Info("EventLogListener start. tag:{0}", _settings.Tag);
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

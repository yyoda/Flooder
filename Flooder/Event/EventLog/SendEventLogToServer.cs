using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Core.Transfer;
using Flooder.Model;
using Flooder.Model.Flooder.Input;
using NLog;

namespace Flooder.Event.EventLog
{
    public class SendEventLogToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly EventLogs _model;
        private readonly IEmitter _emitter;

        public SendEventLogToServer(FlooderModel flooderModel)
        {
            _model = flooderModel.Input.EventLog;
            _emitter  = flooderModel.Output.Workers.Emitter;
        }

        public IDisposable[] Subscribe()
        {
            if (_model.Scopes.Any())
            {
                return _model.Scopes.Select(scope =>
                {
                    var includeInfo  = _model.GetIncludeInfo().ToArray();
                    var includeWarn  = _model.GetIncludeWarn().ToArray();
                    var includeError = _model.GetIncludeError().ToArray();
                    var excludeInfo  = _model.GetExcludeInfo().ToArray();
                    var excludeWarn  = _model.GetExcludeWarn().ToArray();
                    var excludeError = _model.GetExcludeError().ToArray();

                    var observer = new EventLogListener(_model.Tag, _emitter)
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

                    Logger.Info("EventLogListener start. tag:{0}, scope:{1}", _model.Tag, scope);
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

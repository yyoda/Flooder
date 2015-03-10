using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core.Transfer;
using Flooder.Model;
using Flooder.Model.Flooder.Input;
using NLog;

namespace Flooder.Event.IIS
{
    public class SendIISLogToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IISLogs _model;
        private readonly IEmitter _emitter;

        public SendIISLogToServer(FlooderModel flooderModel)
        {
            _model   = flooderModel.Input.IIS;
            _emitter = flooderModel.Output.Workers.Emitter;
        }

        public IDisposable[] Subscribe()
        {
            var enable = _model.Details.Where(x =>
            {
                if (Directory.Exists(x.Path)) return true;
                Logger.Debug("[{0}] will be skipped because it does not exist.", x.ToString());
                return false;
            })
            .Any();

            if (!enable) return new IDisposable[0];

            return _model.Details.Select(x =>
            {
                var observer = new IISLogListener(x.Tag, x.Path, _emitter);
                observer.OnInitAction();

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(x.Interval))
                    .Subscribe(observer);

                Logger.Info("IISLogListener start. tag:{0}, path:{1}, interval:{2}", x.Tag, x.Path, x.Interval);

                return subscribe;
            })
            .ToArray();
        }
    }
}

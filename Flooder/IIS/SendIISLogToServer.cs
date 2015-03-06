using Flooder.Core;
using Flooder.Core.Settings;
using Flooder.Core.Settings.In;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Flooder.IIS
{
    public class SendIISLogToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IISSettings _settings;
        private readonly IEmitter _emitter;

        public SendIISLogToServer(Settings settings)
        {
            _settings = settings.In.IIS;
            _emitter  = settings.Out.Worker.Emitter;
        }

        public IDisposable[] Subscribe()
        {
            var enable = _settings.Details.Where(x =>
            {
                if (Directory.Exists(x.Path)) return true;
                Logger.Debug("[{0}] will be skipped because it does not exist.", x.ToString());
                return false;
            })
            .Any();

            if (!enable) return new IDisposable[0];

            return _settings.Details.Select(x =>
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

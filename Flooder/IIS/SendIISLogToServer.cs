using Flooder.Core.Settings;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using NLog.Config;

namespace Flooder.IIS
{
    public class SendIISLogToServer : IObservable<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly double _interval;

        public SendIISLogToServer(double interval = 5)
        {
            _interval = interval;
        }

        public IDisposable Subscribe(IObserver<long> observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(_interval))
                .Subscribe(observer);
        }

        public static IDisposable[] Start(Settings settings, IEmitter emitter)
        {
            var iis = settings.In.IIS.Details.ToArray();

            var enable = iis.Where(x =>
            {
                if (Directory.Exists(x.Path)) return true;
                Logger.Warn("[{0}] will be skipped because it does not exist.", x.ToString());
                return false;
            }).Any();

            if (!enable) return new IDisposable[0];

            return iis.Select(x =>
            {
                var subject = new SendIISLogToServer(x.Interval);
                var observer = new IISLogListener(x.Tag, x.Path, emitter);
                observer.OnInitAction();
                var subscribe = subject.Subscribe(observer);

                Logger.Info("IISLogListener start. tag:{0}, path:{1}, interval:{2}", x.Tag, x.Path, x.Interval);

                return subscribe;
            })
            .ToArray();
        }
    }
}

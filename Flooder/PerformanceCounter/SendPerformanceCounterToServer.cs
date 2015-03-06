using Flooder.Core.Settings;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core;
using Flooder.Core.Settings.In;

namespace Flooder.PerformanceCounter
{
    public class SendPerformanceCounterToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly PerformanceCounterSettings _settings;
        private readonly IEmitter _emitter;

        public SendPerformanceCounterToServer(Settings settings)
        {
            _settings = settings.In.PerformanceCounters;
            _emitter  = settings.Out.Worker.Emitter;
        }

        public IDisposable Subscribe(IObserver<long> observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(_settings.Interval))
                .Subscribe(observer);
        }

        public IDisposable[] Subscribe()
        {
            var details = _settings.Details
                .Select(x => new PerformanceCounterListener.Setting(x.CategoryName, x.CounterName, x.InstanceName))
                .ToArray();

            var subscribe = Observable
                .Interval(TimeSpan.FromSeconds(_settings.Interval))
                .Subscribe(new PerformanceCounterListener(_settings.Tag, details, _emitter));

            Logger.Info("PerformanceCounterListener start. tag:{0}, interval:{1}", _settings.Tag, _settings.Interval);
            Logger.Trace("PerformanceCounterListener settings:[{0}]", string.Join(",", details.Select(x => x.ToString())));

            return new []{ subscribe };
        }
    }
}

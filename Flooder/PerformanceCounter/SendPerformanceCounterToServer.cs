using Flooder.Core.Settings;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Flooder.PerformanceCounter
{
    public class SendPerformanceCounterToServer : IObservable<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly double _interval;

        public SendPerformanceCounterToServer(double interval = 60)
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
            var performanceCounter = settings.In.PerformanceCounters;

            var details = performanceCounter.Details
                .Select(x => new PerformanceCounterListener.Setting(x.CategoryName, x.CounterName, x.InstanceName))
                .ToArray();

            var subject = new SendPerformanceCounterToServer(performanceCounter.Interval);
            var subscribe = subject.Subscribe(new PerformanceCounterListener(performanceCounter.Tag, details, emitter));

            Logger.Info("PerformanceCounterListener start. tag:{0}", performanceCounter.Tag);
            Logger.Trace("PerformanceCounterListener settings:[{0}]", string.Join(",", details.Select(x => x.ToString())));

            return new []{ subscribe };
        }
    }
}

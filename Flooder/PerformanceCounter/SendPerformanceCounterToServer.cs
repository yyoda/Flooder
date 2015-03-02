using System;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core.Configuration.In;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.PerformanceCounter
{
    public class SendPerformanceCounterToServer : IObservable<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly double _interval;

        public SendPerformanceCounterToServer(double interval = 60000)
        {
            _interval = interval;
        }

        public IDisposable Subscribe(IObserver<long> observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(_interval))
                .Subscribe(observer);
        }

        public static IDisposable Start(PerformanceCounterElementCollection config, IEmitter emitter)
        {
            var settings = config
                .Select(x => new PerformanceCounterListener.Setting(x.CategoryName, x.CounterName, x.InstanceName))
                .ToArray();

            var subject = new SendPerformanceCounterToServer(config.Interval);
            return subject.Subscribe(new PerformanceCounterListener(config.Tag, settings, emitter));
        }
    }
}

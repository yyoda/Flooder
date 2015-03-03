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

        public static IDisposable Start(PerformanceCounterElementCollection config, IEmitter emitter)
        {
            var tag = config.Tag;

            var settings = config
                .Select(x => new PerformanceCounterListener.Setting(x.CategoryName, x.CounterName, x.InstanceName))
                .ToArray();

            var subject = new SendPerformanceCounterToServer(config.Interval);
            var subscribe = subject.Subscribe(new PerformanceCounterListener(tag, settings, emitter));

            Logger.Info("PerformanceCounterListener start. tag:{0}, settings:[{1}]", tag, string.Join(",", settings.Select(x => x.ToString())));
            return subscribe;
        }
    }
}

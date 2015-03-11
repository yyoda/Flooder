using System;
using System.Linq;
using System.Reactive.Linq;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class SendPerformanceCounterToServer : SendEventSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly PerformanceCounterEventSource _eventSource;

        public SendPerformanceCounterToServer(FlooderObject obj) : base(obj)
        {
            _eventSource = obj.Events.OfType<PerformanceCounterEventSource>().First();
        }

        public IDisposable Subscribe(IObserver<long> observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(_eventSource.Interval))
                .Subscribe(observer);
        }

        public override IDisposable[] Subscribe()
        {
            var details = _eventSource.Details
                .Select(x => new PerformanceCounterEventListener.InternalValueObject(x.CategoryName, x.CounterName, x.InstanceName))
                .ToArray();

            var subscribe = Observable
                .Interval(TimeSpan.FromSeconds(_eventSource.Interval))
                .Subscribe(new PerformanceCounterEventListener(_eventSource.Tag, details, base.FlooderObject));

            Logger.Info("PerformanceCounterListener start. tag:{0}, interval:{1}", _eventSource.Tag, _eventSource.Interval);
            Logger.Trace("PerformanceCounterListener settings:[{0}]", string.Join(",", details.Select(x => x.ToString())));

            return new []{ subscribe };
        }
    }
}

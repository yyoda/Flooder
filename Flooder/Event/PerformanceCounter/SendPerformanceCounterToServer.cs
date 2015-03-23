using System;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Transfer;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class SendPerformanceCounterToServer : SendEventSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendPerformanceCounterToServer(IEventSource eventSource, IMessageBroker messageBroker)
            : base(eventSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.EventSource as PerformanceCounterEventSource ?? new PerformanceCounterEventSource();

            if (source.Details.Any())
            {
                var details = source.Details
                    .Select(x => new PerformanceCounterEventListener.InternalValueObject(x.CategoryName, x.CounterName, x.InstanceName))
                    .ToArray();

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(source.Interval))
                    .Subscribe(new PerformanceCounterEventListener(source.Tag, base.MessageBroker, details));

                Logger.Info("PerformanceCounterListener start. tag:{0}, interval:{1}", source.Tag, source.Interval);
                Logger.Trace("PerformanceCounterListener settings:[{0}]", string.Join(",", details.Select(x => x.ToString())));

                return new[] { subscribe };
            }

            return new IDisposable[0];
        }
    }
}

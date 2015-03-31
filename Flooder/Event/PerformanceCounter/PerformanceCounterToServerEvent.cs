using System;
using System.Linq;
using System.Reactive.Linq;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class PerformanceCounterToServerEvent : EventBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public PerformanceCounterToServerEvent(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as PerformanceCounterDataSource ?? new PerformanceCounterDataSource();

            if (source.Options.Any())
            {
                var options = source.Options
                    .Select(x => new PerformanceCounterEventListener.InternalValueObject(x.CategoryName, x.CounterName, x.InstanceName))
                    .ToArray();

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(source.Interval))
                    .Subscribe(new PerformanceCounterEventListener(source.Tag, base.MessageBroker, options));

                Logger.Info("PerformanceCounterListener start. tag:{0}, interval:{1}", source.Tag, source.Interval);
                Logger.Trace("PerformanceCounterListener settings:[{0}]", string.Join(",", options.Select(x => x.ToString())));

                return new[] { subscribe };
            }

            return new IDisposable[0];
        }
    }
}

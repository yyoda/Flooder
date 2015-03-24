using System;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Transfer;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class SendPerformanceCounterToServer : SendDataSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendPerformanceCounterToServer(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as PerformanceCounterDataSource ?? new PerformanceCounterDataSource();

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

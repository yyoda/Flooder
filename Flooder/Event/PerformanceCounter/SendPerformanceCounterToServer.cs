using System;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core.Transfer;
using Flooder.Model;
using Flooder.Model.Flooder.Input;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class SendPerformanceCounterToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly PerformanceCounterLogs _model;
        private readonly IEmitter _emitter;

        public SendPerformanceCounterToServer(FlooderModel flooderModel)
        {
            _model = flooderModel.Input.PerformanceCounter;
            _emitter  = flooderModel.Output.Workers.Emitter;
        }

        public IDisposable Subscribe(IObserver<long> observer)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(_model.Interval))
                .Subscribe(observer);
        }

        public IDisposable[] Subscribe()
        {
            var details = _model.Details
                .Select(x => new PerformanceCounterListener.InternalValueObject(x.CategoryName, x.CounterName, x.InstanceName))
                .ToArray();

            var subscribe = Observable
                .Interval(TimeSpan.FromSeconds(_model.Interval))
                .Subscribe(new PerformanceCounterListener(_model.Tag, details, _emitter));

            Logger.Info("PerformanceCounterListener start. tag:{0}, interval:{1}", _model.Tag, _model.Interval);
            Logger.Trace("PerformanceCounterListener settings:[{0}]", string.Join(",", details.Select(x => x.ToString())));

            return new []{ subscribe };
        }
    }
}

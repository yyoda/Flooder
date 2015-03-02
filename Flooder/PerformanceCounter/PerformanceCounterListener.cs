using System;
using System.Threading.Tasks;
using Flooder.Core.Logging;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.PerformanceCounter
{
    internal class PerformanceCounterListener : IObserver<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _appName;
        private readonly IEmitter _emitter;

        public PerformanceCounterListener(string appName, IEmitter emitter)
        {
            _appName = appName;
            _emitter = emitter;
        }

        public void OnNext(long value)
        {
            var tag = _appName + ".performance_counter.log";
            var payload = PerformanceCounterUtility.GetPayload();

            Task.Factory.StartNew(() => _emitter.Emit(tag, payload));
        }

        public void OnError(Exception error)
        {
            Logger.ErrorException("PerformanceCounterListener", error);
        }

        public void OnCompleted()
        {
            Logger.Debug("PerformanceCounterListener#OnCompleted");
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.PerformanceCounter
{
    internal class PerformanceCounterListener : IObserver<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _tag;
        private readonly Setting[] _settings;
        private readonly IEmitter _emitter;

        public PerformanceCounterListener(string tag, Setting[] settings, IEmitter emitter)
        {
            _tag      = tag;
            _settings = settings;
            _emitter  = emitter;
        }

        public void OnNext(long value)
        {
            var tag = _tag + ".log";

            var payload = _settings.SelectMany(setting =>
            {
                return new PerformanceCounterCategory(setting.CategoryName)
                    .GetInstanceNames()
                    .Where(instanceName =>
                    {
                        if (string.IsNullOrEmpty(setting.InstanceName))
                        {
                            return true;
                        }

                        return setting.InstanceName == instanceName;
                    })
                    .Select(instanceName =>
                    {
                        var perf = new System.Diagnostics.PerformanceCounter(setting.CategoryName, setting.CounterName, instanceName);

                        var path = string.IsNullOrEmpty(perf.InstanceName)
                            ? string.Format("{0}\\{1}", perf.CategoryName, perf.CounterName)
                            : string.Format("{0}({1})\\{2}", perf.CategoryName, perf.InstanceName, perf.CounterName);

                        try
                        {
                            return new { Path = path, CookedValue = perf.NextValue() };
                        }
                        catch (Exception e)
                        {
                            Logger.DebugException(string.Format("Skip because an error has occurred. path:{0}", path), e);
                            return null;
                        }
                    })
                    .Where(x => x != null);
            })
            .ToDictionary(x => x.Path, x => (object)x.CookedValue);

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

        public class Setting
        {
            public string CategoryName { get; private set; }
            public string CounterName { get; private set; }
            public string InstanceName { get; private set; }

            public Setting(string categoryName, string counterName, string instanceName = null)
            {
                CategoryName = categoryName;
                CounterName  = counterName;
                InstanceName = instanceName;
            }
        }
    }
}

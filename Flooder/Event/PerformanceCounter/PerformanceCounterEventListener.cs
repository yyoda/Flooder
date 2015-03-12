using System;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class PerformanceCounterEventListener : EventListenerBase, IObserver<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly InternalValueObject[] _internalValueObjects;

        public PerformanceCounterEventListener(string tag, InternalValueObject[] internalValueObjects, FlooderObject obj)
            : base(tag, obj)
        {
            _internalValueObjects = internalValueObjects;
        }

        public void OnNext(long value)
        {
            var payload = _internalValueObjects.SelectMany(o =>
            {
                return new PerformanceCounterCategory(o.CategoryName)
                    .GetInstanceNames()
                    .Where(instanceName =>
                    {
                        if (string.IsNullOrEmpty(o.InstanceName))
                        {
                            return true;
                        }

                        return o.InstanceName == instanceName;
                    })
                    .Select(instanceName =>
                    {
                        using (var perf = new System.Diagnostics.PerformanceCounter(o.CategoryName, o.CounterName, instanceName))
                        {
                            var path = string.IsNullOrEmpty(perf.InstanceName)
                                ? string.Format("{0}\\{1}", perf.CategoryName, perf.CounterName)
                                : string.Format("{0}({1})\\{2}", perf.CategoryName, perf.InstanceName, perf.CounterName);

                            try
                            {
                                return new { Path = path, CookedValue = perf.NextValue() };
                            }
                            catch (Exception ex)
                            {
                                Logger.DebugException(string.Format("Skip because an error has occurred. path:{0}", path), ex);
                                return null;
                            }
                        }
                    })
                    .Where(x => x != null);
            })
            .ToDictionary(x => x.Path, x => (object)x.CookedValue);

            base.Emit(payload);
        }

        public void OnError(Exception error)
        {
            Logger.FatalException("PerformanceCounterListener", error);
        }

        public void OnCompleted()
        {
            Logger.Fatal("PerformanceCounterListener#OnCompleted");
        }

        public class InternalValueObject
        {
            public string CategoryName { get; private set; }
            public string CounterName { get; private set; }
            public string InstanceName { get; private set; }

            public InternalValueObject(string categoryName, string counterName, string instanceName = null)
            {
                CategoryName = categoryName;
                CounterName  = counterName;
                InstanceName = instanceName;
            }

            public override string ToString()
            {
                return string.Format("{{ CategoryName:{0}, CounterName:{1}, InstanceName:{2} }}", CategoryName, CounterName, InstanceName);
            }
        }
    }
}

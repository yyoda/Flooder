using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Flooder.Event.PerformanceCounter
{
    public class PerformanceCounterEventListener : EventListenerBase<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly System.Diagnostics.PerformanceCounter[] _performanceCounters;

        public PerformanceCounterEventListener(string tag, IMessageBroker messageBroker, InternalValueObject[] internalValueObjects)
            : base(tag, messageBroker)
        {
            _performanceCounters  = this.GetPerformanceCounters(internalValueObjects);
        }

        private System.Diagnostics.PerformanceCounter[] GetPerformanceCounters(IEnumerable<InternalValueObject> source)
        {
            return source.SelectMany(o =>
            {
                var categories = new PerformanceCounterCategory(o.CategoryName);

                switch (categories.CategoryType)
                {
                    case PerformanceCounterCategoryType.SingleInstance:
                        return categories.CounterExists(o.CounterName)
                            ? categories.GetCounters().Where(x => x.CounterName == o.CounterName)
                            : Enumerable.Empty<System.Diagnostics.PerformanceCounter>();
                    case PerformanceCounterCategoryType.MultiInstance:
                        return categories.GetInstanceNames().Where(instanceName =>
                        {
                            if (string.IsNullOrEmpty(o.InstanceName))
                            {
                                return true;
                            }

                            return base.IsLike(instanceName, o.InstanceName);
                        })
                        .Select(instanceName => new System.Diagnostics.PerformanceCounter(o.CategoryName, o.CounterName, instanceName));
                    default:
                        return Enumerable.Empty<System.Diagnostics.PerformanceCounter>();
                }
            })
            .Where(perf =>
            {
                var path = "";

                try
                {
                    path = BuildPath(perf);
                    perf.NextValue();  //init.
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.DebugException(string.Format("Skip1 because an error has occurred. path:{0}", path), ex);
                    return false;
                }
            })
            .ToArray();
        }

        private static string BuildPath(System.Diagnostics.PerformanceCounter performanceCounter)
        {
            return string.IsNullOrEmpty(performanceCounter.InstanceName)
                ? string.Format("{0}\\{1}", performanceCounter.CategoryName, performanceCounter.CounterName)
                : string.Format("{0}({1})\\{2}", performanceCounter.CategoryName, performanceCounter.InstanceName, performanceCounter.CounterName);
        }

        public override void OnNext(long value)
        {
            try
            {
                var payload = _performanceCounters.Select(p =>
                {
                    var path = "";
                    float cookedValue;

                    try
                    {
                        path = BuildPath(p);
                        cookedValue = p.NextValue();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("が指定されたカテゴリにありません。", StringComparison.Ordinal) <= 0)
                        {
                            Logger.ErrorException(string.Format("Skip2 because an error has occurred. path:{0}", path), ex);
                        }
                        else
                        {
                            Logger.TraceException(string.Format("Skip2 because an error has occurred. path:{0}", path), ex);
                        }

                        cookedValue = -1;
                    }

                    return new { Path = path, CookedValue = cookedValue };
                })
                .ToDictionary(x => x.Path, x => (object)x.CookedValue);

                if (payload.Count > 0)
                {
                    base.Publish(payload);
                }
            }
            catch (Exception ex)
            {
                Logger.WarnException("Skip3 because an error has occurred in PerformanceCounterEventListener.", ex);
                throw ex;
            }
        }

        public override void OnError(Exception error)
        {
            Logger.FatalException("PerformanceCounterListener", error);
        }

        public override void OnCompleted()
        {
            Logger.Fatal("PerformanceCounterListener#OnCompleted");

            foreach (var performanceCounter in _performanceCounters)
            {
                performanceCounter.Dispose();
            }
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
                return string.Format("InternalValueObject: {{ CategoryName:{0}, CounterName:{1}, InstanceName:{2} }}", CategoryName, CounterName, InstanceName);
            }
        }
    }
}

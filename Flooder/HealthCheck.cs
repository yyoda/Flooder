using Flooder.CircuitBreaker;
using System;
using System.IO;
using System.Reactive.Linq;

namespace Flooder
{
    public class HealthCheck
    {
        public static void FileMonitoring(string filePath, IFlooderService service)
        {
            var circuitBraker        = new IncrementalRetryableCircuitBreaker();
            var currentlastWriteTime = File.GetLastWriteTime(filePath);

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(
                i =>
                {
                    circuitBraker.ExecuteAction(() =>
                    {
                        if (File.Exists(filePath))
                        {
                            var lastWriteTime = File.GetLastWriteTime(filePath);

                            if (currentlastWriteTime < lastWriteTime)
                            {
                                currentlastWriteTime = lastWriteTime;

                                if (service.IsRunning)
                                {
                                    service.Stop();
                                }

                                service = service.Create();
                                service.Start();
                                return;
                            }
                        }
                        else
                        {
                            if (service.IsRunning)
                            {
                                service.Stop();
                            }

                            throw new InvalidOperationException("detach...");
                        }

                        if (!service.IsRunning)
                        {
                            service = service.Create();
                            service.Start();
                        }
                    });
                }
            );
        }
    }
}

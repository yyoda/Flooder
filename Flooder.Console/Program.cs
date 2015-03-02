using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Flooder.Core.Configuration;
using Flooder.Core.Transfer;
using Flooder.EventLog;
using Flooder.FileSystem;
using Flooder.PerformanceCounter;
using NLog;

namespace Flooder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var instances = new List<IDisposable>();

            var config = Section.GetSectionFromAppConfig();
            var hosts = config.Out.Wokers.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();
            var tcp = new TcpConnectionStateStore(hosts);

            try
            {
                if (config.Out.Wokers.Type == "fluentd")
                {
                    if (tcp.Connect())
                    {
                        var client = new FluentEmitter(tcp);
                        instances.AddRange(SendFileSystemToServer.Start(config.In.FileSystems, client));
                        instances.AddRange(SendEventLogToServer.Start(config.In.EventLogs, client));
                        instances.Add(SendPerformanceCounterToServer.Start(config.In.PerformanceCounters, client));
                        instances.Add(tcp.HealthCheck());
                    }
                }

                if (!instances.Any())
                {
                    throw new InvalidOperationException("Instances is empty.");
                }

                //listening...
                System.Console.ReadLine();
            }
            catch (Exception e)
            {
                logger.ErrorException("Problem occurs.", e);
            }
            finally
            {
                instances.ForEach(x => x.Dispose());
                tcp.Close();
            }
        }
    }
}

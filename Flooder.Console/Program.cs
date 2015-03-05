using Flooder.Core.Configuration;
using Flooder.Core.Transfer;
using Flooder.EventLog;
using Flooder.FileSystem;
using Flooder.PerformanceCounter;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Core.Settings;
using Flooder.IIS;

namespace Flooder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var instances = new List<IDisposable>();

            logger.Info("Flooder start.");

            var settings = SettingsFactory.Create<Section>();
            var hosts = settings.Out.Worker.Details.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();
            var tcp = new TcpConnectionManager(hosts);

            try
            {
                if (settings.Out.Worker.Type == "fluentd" && tcp.Connect())
                {
                    var fluent = new FluentEmitter(tcp);

                    instances = new[]
                    {
                        SendIISLogToServer.Start(settings, fluent),
                        SendFileSystemToServer.Start(settings, fluent),
                        SendEventLogToServer.Start(settings, fluent),
                        SendPerformanceCounterToServer.Start(settings, fluent),
                        tcp.HealthCheck(),
                    }
                    .SelectMany(x => x)
                    .ToList();
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

            logger.Info("Flooder stoped.");
        }
    }
}

using Flooder.Core;
using Flooder.Core.Settings;
using Flooder.EventLog;
using Flooder.FileSystem;
using Flooder.IIS;
using Flooder.PerformanceCounter;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var instances = new List<IDisposable>();

            logger.Info("Flooder start.");

            var settings = SettingsFactory.Create<Flooder.Core.Configuration.Section>();
            var tcp = settings.Out.Worker.Connection;

            try
            {
                if (settings.Out.Worker.Type == "fluentd" && tcp.Connect())
                {
                    instances = new IFlooderEvent[]
                    {
                        new SendFileSystemToServer(settings),
                        new SendIISLogToServer(settings),
                        new SendEventLogToServer(settings),
                        new SendPerformanceCounterToServer(settings),
                    }
                    .SelectMany(x =>
                    {
                        return x.Subscribe();
                    })
                    .ToList();

                    instances.Add(tcp.HealthCheck());
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

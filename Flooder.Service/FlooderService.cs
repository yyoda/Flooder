using Flooder.Core.Configuration;
using Flooder.Core.Transfer;
using Flooder.EventLog;
using Flooder.FileSystem;
using Flooder.PerformanceCounter;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;

namespace Flooder.Service
{
    public partial class FlooderService : ServiceBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        List<IDisposable> instances;

        public FlooderService()
        {
            InitializeComponent();
            instances = new List<IDisposable>();
        }

        protected override void OnStart(string[] args)
        {
            logger.Debug("flooder start.");

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

        protected override void OnStop()
        {
            instances.ForEach(x => x.Dispose());
            logger.Debug("flooder stoped.");
        }
    }
}

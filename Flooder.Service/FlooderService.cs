using Flooder.Core.Configuration;
using Flooder.Core.Transfer;
using Flooder.EventLog;
using Flooder.FileSystem;
using Flooder.PerformanceCounter;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;

namespace Flooder.Service
{
    public partial class FlooderService : ServiceBase
    {
        private readonly Logger _logger;
        private readonly List<IDisposable> _instances;
        private TcpConnectionStateStore _tcp;

        public FlooderService()
        {
            InitializeComponent();
            _logger = LogManager.GetCurrentClassLogger();
            _instances = new List<IDisposable>();
        }

        protected override void OnStart(string[] args)
        {
            var config = Section.GetSectionFromAppConfig();
            var hosts = config.Out.Wokers.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();

            try
            {
                _tcp = new TcpConnectionStateStore(hosts);

                if (config.Out.Wokers.Type == "fluentd" && _tcp.Connect())
                {
                    var client = new FluentEmitter(_tcp);
                    _instances.AddRange(SendFileSystemToServer.Start(config.In.FileSystems, client));
                    _instances.AddRange(SendEventLogToServer.Start(config.In.EventLogs, client));
                    _instances.Add(SendPerformanceCounterToServer.Start(config.In.PerformanceCounters, client));
                    _instances.Add(_tcp.HealthCheck());
                }

                if (!_instances.Any())
                {
                    throw new InvalidOperationException("Instances is empty.");
                }
            }
            catch (Exception e)
            {
                _logger.ErrorException("Problem occurs.", e);
            }
        }

        protected override void OnStop()
        {
            _instances.ForEach(x => x.Dispose());
            _tcp.Close();
            _logger.Debug("flooder stoped.");
        }
    }
}

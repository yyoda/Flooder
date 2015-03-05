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
using Flooder.Core.Settings;

namespace Flooder.Service
{
    public partial class FlooderService : ServiceBase
    {
        private readonly Logger _logger;
        private List<IDisposable> _instances;
        private TcpConnectionManager _tcp;

        public FlooderService()
        {
            InitializeComponent();
            _logger = LogManager.GetCurrentClassLogger();
            _instances = new List<IDisposable>();
        }

        protected override void OnStart(string[] args)
        {
            var settings = SettingsFactory.Create<Section>();
            var hosts = settings.Out.Worker.Details.Select(x => Tuple.Create(x.Host, x.Port)).ToArray();

            try
            {
                _tcp = new TcpConnectionManager(hosts);

                if (settings.Out.Worker.Type == "fluentd" && _tcp.Connect())
                {
                    var fluent = new FluentEmitter(_tcp);

                    _instances = new[]
                    {
                        SendFileSystemToServer.Start(settings, fluent),
                        SendEventLogToServer.Start(settings, fluent),
                        SendPerformanceCounterToServer.Start(settings, fluent),
                        _tcp.HealthCheck(),
                    }
                    .SelectMany(x => x)
                    .ToList();
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

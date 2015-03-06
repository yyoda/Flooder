using Flooder.Core.Transfer;
using Flooder.EventLog;
using Flooder.FileSystem;
using Flooder.PerformanceCounter;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Flooder.Core;
using Flooder.Core.Settings;
using Flooder.IIS;

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
            var settings = SettingsFactory.Create<Flooder.Core.Configuration.Section>();
            _tcp = settings.Out.Worker.Connection;

            try
            {
                if (settings.Out.Worker.Type == "fluentd" && _tcp.Connect())
                {
                    _instances = new IFlooderEvent[]
                    {
                        new SendIISLogToServer(settings),
                        new SendFileSystemToServer(settings),
                        new SendEventLogToServer(settings),
                        new SendPerformanceCounterToServer(settings),
                    }
                    .SelectMany(x => x.Subscribe())
                    .ToList();

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

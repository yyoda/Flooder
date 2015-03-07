using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Event;
using Flooder.Event.EventLog;
using Flooder.Event.FileSystem;
using Flooder.Event.IIS;
using Flooder.Event.PerformanceCounter;
using Flooder.Model;
using NLog;

namespace Flooder
{
    public class FlooderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private List<IDisposable> _events = new List<IDisposable>();

        public FlooderService(FlooderModel model)
        {
            Model = model;
        }

        public FlooderModel Model { get; private set; }

        public void Start()
        {
            Logger.Info("Flooder start.");

            if (this.Model.Output.Workers.Type == "fluentd" && this.Model.Output.Workers.Connection.Connect())
            {
                _events = new IFlooderEvent[]
                {
                    new SendFileSystemToServer(this.Model),
                    new SendIISLogToServer(this.Model),
                    new SendEventLogToServer(this.Model),
                    new SendPerformanceCounterToServer(this.Model),
                }
                .SelectMany(x =>
                {
                    return x.Subscribe();
                })
                .ToList();

                _events.Add(this.Model.Output.Workers.Connection.HealthCheck());
            }

            if (!_events.Any())
            {
                throw new InvalidOperationException("Instances is empty.");
            }
        }

        public void Stop()
        {
            _events.ForEach(x => x.Dispose());
            this.Model.Output.Workers.Connection.Close();
            Logger.Info("Flooder stop.");
        }
    }
}

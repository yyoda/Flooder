using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Flooder.Core.Configuration;
using Flooder.Event;
using Flooder.Event.EventLog;
using Flooder.Event.FileSystem;
using Flooder.Event.IIS;
using Flooder.Event.PerformanceCounter;
using Flooder.Model;
using Flooder.Model.Flooder;
using Flooder.Model.Flooder.Input;
using Flooder.Model.Flooder.Output;
using NLog;

namespace Flooder
{
    public class DefaultService : IFlooderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private List<IDisposable> _events = new List<IDisposable>();

        public DefaultService()
        {
        }

        public DefaultService(FlooderModel model)
        {
            Model = model;
        }

        public FlooderModel Model { get; private set; }

        public IFlooderService Create()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;

            //in
            var fs = new FileSystemLogs(section.In.FileSystems
                .Select(x => new FileSystemLogs.FileSystemLog(x.Tag, x.Path, x.File, x.Payload)));

            var iis = new IISLogs(section.In.IIS
                .Select(x => new IISLogs.IISLog(x.Tag, x.Path, x.Interval)));

            var ev = new EventLogs(section.In.EventLogs.Tag, section.In.EventLogs.Scopes, section.In.EventLogs
                .Select(x => new EventLogs.EventLog(x.Type, x.Mode, x.Source, x.Id)));

            var pc = new PerformanceCounterLogs(section.In.PerformanceCounters.Tag, section.In.PerformanceCounters.Interval, section.In.PerformanceCounters
                .Select(x => new PerformanceCounterLogs.PerformanceCounterLog(x.CategoryName, x.CounterName, x.InstanceName)));

            //out
            var wk = new Workers(section.Out.Workers.Type, section.Out.Workers
                .Select(x => new Workers.Worker(x.Host, x.Port)));

            //model
            var model = new FlooderModel(new InputModel(fs, iis, ev, pc), new OutputModel(wk));

            return new DefaultService(model);
        }

        public void Start()
        {
            if (Model == null) throw new NullReferenceException("Model");

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
            if (Model == null) throw new NullReferenceException("Model");

            _events.ForEach(x => x.Dispose());
            this.Model.Output.Workers.Connection.Close();
            Logger.Info("Flooder stop.");
        }
    }
}

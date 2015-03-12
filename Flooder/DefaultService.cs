using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Flooder.Configuration;
using Flooder.Event;
using Flooder.Event.EventLog;
using Flooder.Event.FileSystem;
using Flooder.Event.IIS;
using Flooder.Event.PerformanceCounter;
using Flooder.Transfer;
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

        public DefaultService(FlooderObject obj)
        {
            FlooderObject = obj;
        }

        public FlooderObject FlooderObject { get; private set; }

        public IFlooderService Create()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;

            //in
            var fs = new FileSystemEventSource(section.In.FileSystems
                .Select(x => new FileSystemEventSourceDetail(x.Tag, x.Path, x.File, x.Parser)));

            var iis = new IISLogEventSource(section.In.IIS
                .Select(x => new IISLogEventSourceDetail(x.Tag, x.Path, x.Interval)));

            var ev = new EventLogEventSource(section.In.EventLogs.Tag, section.In.EventLogs.Scopes, section.In.EventLogs
                .Select(x => new EventLogEventSourceDetail(x.Type, x.Mode, x.Source, x.Id)));

            var pc = new PerformanceCounterEventSource(section.In.PerformanceCounters.Tag, section.In.PerformanceCounters.Interval, section.In.PerformanceCounters
                .Select(x => new PerformanceCounterEventSourceDetail(x.CategoryName, x.CounterName, x.InstanceName)));

            //out
            var wk = new Worker(section.Out.Workers.Type, section.Out.Workers
                .Select(x => new WorkerDetail(x.Host, x.Port)));

            var obj = new FlooderObject(new IEventSource[] { fs, iis, ev, pc }, wk);

            return new DefaultService(obj);
        }

        public void Start()
        {
            if (FlooderObject == null) throw new NullReferenceException("FlooderObject");

            Logger.Info("Flooder start.");

            if (this.FlooderObject.Worker.Type == "fluentd" && this.FlooderObject.Worker.Connection.Connect())
            {
                _events = new SendEventSourceToServerBase[]
                {
                    new SendFileSystemToServer(this.FlooderObject),
                    new SendIISLogToServer(this.FlooderObject),
                    new SendEventLogToServer(this.FlooderObject),
                    new SendPerformanceCounterToServer(this.FlooderObject),
                }
                .SelectMany(x =>
                {
                    return x.Subscribe();
                })
                .ToList();

                _events.Add(this.FlooderObject.Worker.Connection.HealthCheck());
            }

            if (!_events.Any())
            {
                throw new InvalidOperationException("Instances is empty.");
            }
        }

        public void Stop()
        {
            if (FlooderObject == null) throw new NullReferenceException("FlooderObject");

            _events.ForEach(x => x.Dispose());
            this.FlooderObject.Worker.Connection.Close();
            Logger.Info("Flooder stop.");
        }
    }
}

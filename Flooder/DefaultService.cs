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
            var events = new List<IEventSource>();

            //event
            if (section.Event.FileSystems.Any())
            {
                var fs = new FileSystemEventSource(
                    section.Event.FileSystems.Select(x => new FileSystemEventSourceDetail(x.Tag, x.Path, x.File, x.Listener, x.Parser)));

                events.Add(fs);
            }

            if (section.Event.IIS.Any())
            {
                var iis = new IISLogEventSource(
                    section.Event.IIS.Select(x => new IISLogEventSourceDetail(x.Tag, x.Path, x.Interval)));

                events.Add(iis);
            }

            if (section.Event.EventLogs.Scopes.Any())
            {
                var ev = new EventLogEventSource(
                    section.Event.EventLogs.Tag,
                    section.Event.EventLogs.Scopes,
                    section.Event.EventLogs.Select(x => new EventLogEventSourceDetail(x.Type, x.Mode, x.Source, x.Id)));

                events.Add(ev);
            }

            if (section.Event.PerformanceCounters.Any())
            {
                var pc = new PerformanceCounterEventSource(
                    section.Event.PerformanceCounters.Tag,
                    section.Event.PerformanceCounters.Interval,
                    section.Event.PerformanceCounters.Select(x => new PerformanceCounterEventSourceDetail(x.CategoryName, x.CounterName, x.InstanceName)));

                events.Add(pc);
            }

            //worker
            var wk = new Worker(
                section.Worker.Type,
                section.Worker.Select(x => new WorkerDetail(x.Host, x.Port)));

            var obj = new FlooderObject(events, wk);

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

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
        private readonly List<IDisposable> instances = new List<IDisposable>();

        private readonly IEnumerable<SendDataSourceToServerBase> _events;
        private readonly Worker _worker;

        public DefaultService()
        {
        }

        public DefaultService(IEnumerable<SendDataSourceToServerBase> events, Worker worker)
        {
            _events = events;
            _worker = worker;
        }

        public IFlooderService Create()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;

            //worker
            var worker = new Worker(
                section.Worker.Type,
                section.Worker.Select(x => new WorkerDetail(x.Host, x.Port)));

            //event
            var events = new List<SendDataSourceToServerBase>();
            if (section.Event.FileSystems.Any())
            {
                var fs = new FileSystemDataSource(
                    section.Event.FileSystems.Select(x => new FileSystemDataSourceDetail(x.Tag, x.Path, x.File, x.Listener, x.Parser)));

                events.Add(new SendFileSystemToServer(fs, worker.MessageBroker));
            }

            if (section.Event.IIS.Any())
            {
                var iis = new IISLogDataSource(
                    section.Event.IIS.Select(x => new IISLogDataSourceDetail(x.Tag, x.Path, x.Interval)));

                events.Add(new SendIISLogToServer(iis, worker.MessageBroker));
            }

            if (section.Event.EventLogs.Scopes.Any())
            {
                var ev = new EventLogDataSource(
                    section.Event.EventLogs.Tag,
                    section.Event.EventLogs.Scopes,
                    section.Event.EventLogs.Select(x => new EventLogDataSourceDetail(x.Type, x.Mode, x.Source, x.Id)));

                events.Add(new SendEventLogToServer(ev, worker.MessageBroker));
            }

            if (section.Event.PerformanceCounters.Any())
            {
                var pc = new PerformanceCounterDataSource(
                    section.Event.PerformanceCounters.Tag,
                    section.Event.PerformanceCounters.Interval,
                    section.Event.PerformanceCounters.Select(x => new PerformanceCounterDataSourceDetail(x.CategoryName, x.CounterName, x.InstanceName)));

                events.Add(new SendPerformanceCounterToServer(pc, worker.MessageBroker));
            }

            return new DefaultService(events, worker);
        }

        public void Start()
        {
            Logger.Info("Flooder start.");

            if (_worker == null) throw new NullReferenceException("Worker");
            if (_events == null) throw new NullReferenceException("Events");

            var worker = _worker.Subscribe();
            var events = _events.SelectMany(x => x.Subscribe());

            instances.AddRange(worker);
            instances.AddRange(events);
        }

        public void Stop()
        {
            if (_worker == null) throw new NullReferenceException("Worker");
            if (_events == null) throw new NullReferenceException("Events");

            instances.ForEach(x => x.Dispose());
            Logger.Info("Flooder stop.");
        }
    }
}

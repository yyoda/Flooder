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
using Flooder.Worker;
using NLog;

namespace Flooder
{
    public class DefaultService : IFlooderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<IDisposable> instances = new List<IDisposable>();

        private readonly IEnumerable<SendDataSourceToServerBase> _events;
        private readonly IMessageBroker _worker;

        public DefaultService()
        {
        }

        public DefaultService(IEnumerable<SendDataSourceToServerBase> events, IMessageBroker worker)
        {
            _events = events;
            _worker = worker;
        }

        public IFlooderService Create()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;

            //worker
            var worker = WorkerFactory.Create(
                section.Worker.Type,
                new WorkerOption(section.Worker.Select(x => Tuple.Create(x.Host, x.Port))));

            //event
            var events = new List<SendDataSourceToServerBase>();
            if (section.Event.FileSystems.Any())
            {
                var fs = new FileSystemDataSource(
                    section.Event.FileSystems.Select(x => new FileSystemDataSourceOption(x.Tag, x.Path, x.File, x.Listener, x.Parser)));

                events.Add(new SendFileSystemToServer(fs, worker));
            }

            if (section.Event.IIS.Any())
            {
                var iis = new IISLogDataSource(
                    section.Event.IIS.Select(x => new IISLogDataSourceOption(x.Tag, x.Path, x.Interval)));

                events.Add(new SendIISLogToServer(iis, worker));
            }

            if (section.Event.EventLogs.Any() && section.Event.EventLogs.Scopes.Any())
            {
                var ev = new EventLogDataSource(
                    section.Event.EventLogs.Tag,
                    section.Event.EventLogs.Scopes,
                    section.Event.EventLogs.Select(x => new EventLogDataSourceOption(x.Type, x.Mode, x.Source, x.Id)));

                events.Add(new SendEventLogToServer(ev, worker));
            }

            if (section.Event.PerformanceCounters.Any())
            {
                var pc = new PerformanceCounterDataSource(
                    section.Event.PerformanceCounters.Tag,
                    section.Event.PerformanceCounters.Interval,
                    section.Event.PerformanceCounters.Select(x => new PerformanceCounterDataSourceOption(x.CategoryName, x.CounterName, x.InstanceName)));

                events.Add(new SendPerformanceCounterToServer(pc, worker));
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

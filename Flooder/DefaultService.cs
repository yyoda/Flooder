using Flooder.Configuration;
using Flooder.Worker;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Flooder.Events;
using Flooder.Events.EventLog;
using Flooder.Events.FileLoad;
using Flooder.Events.FileSystem;
using Flooder.Events.IIS;
using Flooder.Events.PerformanceCounter;

namespace Flooder
{
    public class DefaultService : IFlooderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<IDisposable> _instances = new List<IDisposable>();
        private IEnumerable<EventBase> _events;
        private IMessageBroker _worker;

        public bool IsRunning { get; private set; }

        public DefaultService()
        {
        }

        public DefaultService(IEnumerable<EventBase> events, IMessageBroker worker)
        {
            _events = events;
            _worker = worker;
        }

        public IFlooderService Create()
        {
            ConfigurationManager.RefreshSection("flooder");

            var section = ConfigurationManager.GetSection("flooder") as Section;
            if (section == null) throw new NullReferenceException("Section");

            var worker = WorkerFactory.Create(
                section.Worker.Type,
                new MessageBrokerOption(section.Worker.Select(x => Tuple.Create(x.Host, x.Port))));

            var events = new List<EventBase>();
            if (section.Event.FileSystems.Any())
            {
                var fs = new FileSystemDataSource(
                    section.Event.FileSystems.Select(x => new FileSystemDataSourceOption(x.Tag, x.Path, x.File, x.Listener, x.Parser)));

                events.Add(new FileSystemToServerEvent(fs, worker));
            }

            if (section.Event.FileLoad.Any())
            {
                var fs = new FileLoadDataSource(
                    section.Event.FileLoad.Select(x => new FileLoadDataSourceOption(x.Tag, x.Path, x.File, x.Listener, x.Parser, x.Interval)));

                events.Add(new FileLoadToServerEvent(fs, worker));
            }

            if (section.Event.IIS.Any())
            {
                var iis = new IISLogDataSource(
                    section.Event.IIS.Select(x => new IISLogDataSourceOption(x.Tag, x.Path, x.Interval)));

                events.Add(new IISLogToServerEvent(iis, worker));
            }

            if (section.Event.EventLogs.Scopes.Any())
            {
                var ev = new EventLogDataSource(
                    section.Event.EventLogs.Tag,
                    section.Event.EventLogs.Scopes,
                    section.Event.EventLogs.Select(x => new EventLogDataSourceOption(x.Type, x.Mode, x.Source, x.Id)));

                events.Add(new EventLogToServerEvent(ev, worker));
            }

            if (section.Event.PerformanceCounters.Any())
            {
                var pc = new PerformanceCounterDataSource(
                    section.Event.PerformanceCounters.Tag,
                    section.Event.PerformanceCounters.Interval,
                    section.Event.PerformanceCounters.Select(x => new PerformanceCounterDataSourceOption(x.CategoryName, x.CounterName, x.InstanceName)));

                events.Add(new PerformanceCounterToServerEvent(pc, worker));
            }

            return new DefaultService(events, worker);
        }

        public void Start()
        {
            Logger.Info("Flooder start.");

            if (_worker == null) throw new NullReferenceException("worker");
            if (_events == null) throw new NullReferenceException("events");

            var worker = _worker.Subscribe();
            var events = _events.SelectMany(x =>
            {
                return x.Subscribe();
            });

            _instances.AddRange(worker);
            _instances.AddRange(events);

            IsRunning = true;
        }

        public void Stop()
        {
            _instances.ForEach(x => x.Dispose());
            _events = null;
            _worker = null;

            IsRunning = false;

            Logger.Info("Flooder stop.");
        }
    }
}

using Flooder.Configuration;
using Flooder.Event;
using Flooder.Event.EventLog;
using Flooder.Event.FileLoad;
using Flooder.Event.FileSystem;
using Flooder.Event.IIS;
using Flooder.Event.PerformanceCounter;
using Flooder.Worker;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Flooder
{
    public class DefaultService : IFlooderService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<IDisposable> _instances = new List<IDisposable>();
        private IEnumerable<SendDataSourceToServerBase> _events;
        private IMessageBroker _worker;

        public bool IsStart { get; private set; }

        public DefaultService()
        {
            IsStart = false;
        }

        public DefaultService(IEnumerable<SendDataSourceToServerBase> events, IMessageBroker worker)
        {
            IsStart = false;

            _events = events;
            _worker = worker;
        }

        public IFlooderService Create()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;
            if (section == null) throw new NullReferenceException("Section");

            var worker = WorkerFactory.Create(
                section.Worker.Type,
                new MessageBrokerOption(section.Worker.Select(x => Tuple.Create(x.Host, x.Port))));

            var events = new List<SendDataSourceToServerBase>();
            if (section.Event.FileSystems.Any())
            {
                var fs = new FileSystemDataSource(
                    section.Event.FileSystems.Select(x => new FileSystemDataSourceOption(x.Tag, x.Path, x.File, x.Listener, x.Parser)));

                events.Add(new SendFileSystemToServer(fs, worker));
            }

            if (section.Event.FileLoad.Any())
            {
                var fs = new FileLoadDataSource(
                    section.Event.FileLoad.Select(x => new FileLoadDataSourceOption(x.Tag, x.Path, x.File, x.Parser, x.Interval)));

                events.Add(new SendFileLoadToServer(fs, worker));
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

            if (_worker == null) throw new NullReferenceException("worker");
            if (_events == null) throw new NullReferenceException("events");

            var worker = _worker.Subscribe();
            var events = _events.SelectMany(x =>
            {
                return x.Subscribe();
            });

            _instances.AddRange(worker);
            _instances.AddRange(events);

            IsStart = true;
        }

        public void Stop()
        {
            _instances.ForEach(x => x.Dispose());

            _events = null;
            _worker = null;

            IsStart = false;

            Logger.Info("Flooder stop.");
        }

        public void Restart()
        {
            this.Stop();
            this.Create().Start();
        }

        //TODO:
        //public IDisposable HealthCheck()
        //{
        //    var locationPath  = Assembly.GetExecutingAssembly().Location;
        //    var locationDir   = Path.GetDirectoryName(locationPath);
        //    var circuitBraker = new DefaultCircuitBreaker();

        //    return Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(
        //        i =>
        //        {
        //            circuitBraker.ExecuteAction(() =>
        //            {
        //                var fullPath = locationDir + @"\detach.txt";
        //                if (File.Exists(fullPath))
        //                {
        //                    this.Stop();
        //                    Logger.Info("detach. path:{0}", fullPath);
        //                    throw new InvalidOperationException("detach start.");
        //                }

        //                if (!IsStart)
        //                {
        //                    this.Restart();
        //                }

        //                Logger.Info("attach. path:{0}", fullPath);
        //            });
        //        }
        //    );
        //}
    }
}

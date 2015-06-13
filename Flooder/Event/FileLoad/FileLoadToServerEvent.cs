using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Flooder.Event.FileLoad
{
    public class FileLoadToServerEvent : EventBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FileLoadToServerEvent(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as FileLoadDataSource ?? new FileLoadDataSource();
            return source.Options.Select(x =>
            {
                var parser = (IParsePlugin) Activator.CreateInstance(
                    x.Parser, BindingFlags.CreateInstance, null, new object[] { }, null);

                var listener = (EventListenerBase<long>) Activator.CreateInstance(
                    x.Listener, BindingFlags.CreateInstance, null, new object[] { x.Tag, x.Path, x.File, base.MessageBroker, parser }, null);

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(x.Interval))
                    .Subscribe(listener);

                Logger.Info("FileLoadListener start. tag:{0}, path:{1}, interval:{2}", x.Tag, x.Path, x.Interval);

                return subscribe;
            })
            .ToArray();
        }
    }
}

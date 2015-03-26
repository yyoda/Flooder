using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace Flooder.Event.FileLoad
{
    public class SendFileLoadToServer : EventBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendFileLoadToServer(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as FileLoadDataSource ?? new FileLoadDataSource();
            return source.Options.Select(x =>
            {
                var parser = (IPayloadParser) Activator.CreateInstance(
                    x.Parser, BindingFlags.CreateInstance, null, new object[] { }, null);

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(x.Interval))
                    .Subscribe(new FileLoadListener(x.Tag, x.Path, x.File, base.MessageBroker, parser));

                Logger.Info("FileLoadListener start. tag:{0}, path:{1}, interval:{2}", x.Tag, x.Path, x.Interval);

                return subscribe;
            })
            .ToArray();
        }
    }
}

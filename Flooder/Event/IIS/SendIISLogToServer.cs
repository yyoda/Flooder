using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using NLog;

namespace Flooder.Event.IIS
{
    public class SendIISLogToServer : SendDataSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendIISLogToServer(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as IISLogDataSource ?? new IISLogDataSource();

            var enable = source.Options.Where(x =>
            {
                if (Directory.Exists(x.Path)) return true;
                Logger.Debug("[{0}] will be skipped because it does not exist.", x.ToString());
                return false;
            })
            .Any();

            if (!enable) return new IDisposable[0];

            return source.Options.Select(x =>
            {
                var observer = new IISLogListener(x.Tag, x.Path, base.MessageBroker);
                observer.OnInitAction();

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(x.Interval))
                    .Subscribe(observer);

                Logger.Info("IISLogListener start. tag:{0}, path:{1}, interval:{2}", x.Tag, x.Path, x.Interval);

                return subscribe;
            })
            .ToArray();
        }
    }
}

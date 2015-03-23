using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using NLog;

namespace Flooder.Event.FileSystem
{
    public class SendFileSystemToServer : SendEventSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public SendFileSystemToServer(IEventSource eventSource, IMessageBroker messageBroker)
            : base(eventSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.EventSource as FileSystemEventSource ?? new FileSystemEventSource();
            if (source.Details.Any())
            {
                return source.Details.Select(x =>
                {
                    var parser = (IPayloadParser) Activator.CreateInstance(
                        x.Parser, BindingFlags.CreateInstance, null, new object[] { }, null);

                    var listener = (FileSystemEventListenerBase) Activator.CreateInstance(
                        x.Listner, BindingFlags.CreateInstance, null, new object[] { x.Tag, x.Path, base.MessageBroker, parser }, null);

                    var subscribe = CreateSubject(x).Subscribe(listener.Create());

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}, parser:{2}", x.Tag, x.Path, x.Parser.FullName);

                    return subscribe;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }

        private static IObservable<FileSystemEventArgs> CreateSubject(FileSystemEventSourceDetail source)
        {
            if (!Directory.Exists(source.Path))
            {
                Logger.Warn("[{0}] will be skipped because it does not exist.", source.Path);
                return Observable.Never<FileSystemEventArgs>();
            }

            var fsw = new FileSystemWatcher(source.Path, source.File)
            {
                EnableRaisingEvents = true
            };

            var sources = new[]
            {
                fsw.CreatedAsObservable(),
                fsw.DeletedAsObservable(),
                fsw.ChangedAsObservable(),
                fsw.RenamedAsObservable()
            };

            return Observable.Merge(sources);
        }
    }
}

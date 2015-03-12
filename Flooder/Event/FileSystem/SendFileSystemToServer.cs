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
        private readonly FileSystemEventSource _eventSource;
        
        public SendFileSystemToServer(FlooderObject obj) : base(obj)
        {
            _eventSource = obj.Events.OfType<FileSystemEventSource>().First();
        }

        public override IDisposable[] Subscribe()
        {
            if (_eventSource.Details.Any())
            {
                return _eventSource.Details.Select(e =>
                {
                    var parser = (IPayloadParser) Activator.CreateInstance(
                        e.Parser, BindingFlags.CreateInstance, null, new object[] { }, null);

                    IObserver<FileSystemEventArgs> observer = new DefaultEventListener(e.Tag, e.Path, base.FlooderObject, parser).Create();

                    var subscribe = CreateSubject(e).Subscribe(observer);

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}, parser:{2}", e.Tag, e.Path, e.Parser.FullName);

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

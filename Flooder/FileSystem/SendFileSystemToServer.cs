using Flooder.Core.Configuration.In;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Flooder.FileSystem
{
    public class SendFileSystemToServer : IObservable<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _filePath;
        
        public SendFileSystemToServer(string filePath)
        {
            _filePath = filePath;
        }

        public IDisposable Subscribe(IObserver<FileSystemEventArgs> observer)
        {
            if (!Directory.Exists(_filePath))
            {
                Logger.Warn("[{0}] will be skipped because it does not exist.", _filePath);
                return Observable.Never<FileSystemEventArgs>().Subscribe(observer);
            }

            ((FileSystemEventListener) observer).InitFileSeekStore(_filePath);

            var fsw = new FileSystemWatcher(_filePath, "*.log") { EnableRaisingEvents = true };

            var sources = new[]
            {
                fsw.CreatedAsObservable(),
                fsw.DeletedAsObservable(),
                fsw.ChangedAsObservable(),
                fsw.RenamedAsObservable()
            };

            return Observable.Merge(sources).Subscribe(observer);
        }

        public static IDisposable[] Start(FileSystemElementCollection config, IEmitter emitter)
        {
            if (config.Any())
            {
                return config.Select(x =>
                {
                    var subject = new SendFileSystemToServer(x.Path);
                    var subscribe = subject.Subscribe(new FileSystemEventListener(x.Tag, emitter));

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}", x.Tag, x.Path);
                    return subscribe;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }
    }
}

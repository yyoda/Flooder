using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core;
using Flooder.Core.Configuration;
using Flooder.Core.Configuration.In;
using Flooder.Core.Logging;
using Flooder.Core.Transfer;
using NLog;

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
                    return subject.Subscribe(new FileSystemEventListener(x.Tag, emitter));
                })
                .ToArray();
            }

            return new IDisposable[0];
        }
    }
}

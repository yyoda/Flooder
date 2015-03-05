using Flooder.Core.Transfer;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core.Settings;

namespace Flooder.FileSystem
{
    public class SendFileSystemToServer : IObservable<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _filePath;
        private readonly string _fileName;
        
        public SendFileSystemToServer(string filePath, string fileName)
        {
            _filePath = filePath;
            _fileName = fileName;
        }

        public IDisposable Subscribe(IObserver<FileSystemEventArgs> observer)
        {
            if (!Directory.Exists(_filePath))
            {
                Logger.Warn("[{0}] will be skipped because it does not exist.", _filePath);
                return Observable.Never<FileSystemEventArgs>().Subscribe(observer);
            }

            ((FileSystemEventListener) observer).OnInitAction(_filePath);

            var fsw = new FileSystemWatcher(_filePath, _fileName) { EnableRaisingEvents = true };

            var sources = new[]
            {
                fsw.CreatedAsObservable(),
                fsw.DeletedAsObservable(),
                fsw.ChangedAsObservable(),
                fsw.RenamedAsObservable()
            };

            return Observable.Merge(sources).Subscribe(observer);
        }

        public static IDisposable[] Start(Settings settings, IEmitter emitter)
        {
            var fileSystem = settings.In.FileSystems.Details.ToArray();

            if (fileSystem.Any())
            {
                return fileSystem.Select(x =>
                {
                    var subject = new SendFileSystemToServer(x.Path, x.File);
                    IDisposable subscribe;

                    switch (x.Format)
                    {
                        //additional.
                        default:
                            subscribe = subject.Subscribe(new TxtEventListener(x.Tag, emitter));
                            break;
                    }

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}", x.Tag, x.Path);
                    return subscribe;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }
    }
}

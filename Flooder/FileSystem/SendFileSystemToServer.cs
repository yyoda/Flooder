using Flooder.Core;
using Flooder.Core.Settings;
using Flooder.Core.Settings.In;
using Flooder.Core.Transfer;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Flooder.FileSystem
{
    public class SendFileSystemToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly FileSystemSettings _settings;
        private readonly IEmitter _emitter;

        
        public SendFileSystemToServer(Settings settings)
        {
            _settings = settings.In.FileSystems;
            _emitter  = settings.Out.Worker.Emitter;
        }

        public IDisposable[] Subscribe()
        {
            if (_settings.Details.Any())
            {
                return _settings.Details.Select(s =>
                {
                    IDisposable subscribe;
                    var subject = CreateSubject(s);

                    switch (s.Format)
                    {
                        //additional.
                        default:
                            subscribe = subject.Subscribe(TxtEventListener.Create(s.Tag, s.Path, _emitter));
                            break;
                    }

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}", s.Tag, s.Path);
                    return subscribe;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }

        private static IObservable<FileSystemEventArgs> CreateSubject(FileSystemSettings.FileSystemSettingsDetail settingsDetail)
        {
            if (!Directory.Exists(settingsDetail.Path))
            {
                Logger.Warn("[{0}] will be skipped because it does not exist.", settingsDetail.Path);
                return Observable.Never<FileSystemEventArgs>();
            }

            var fsw = new FileSystemWatcher(settingsDetail.Path, settingsDetail.File)
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

using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Flooder.Core.Transfer;
using Flooder.Model;
using Flooder.Model.Flooder.Input;
using NLog;

namespace Flooder.Event.FileSystem
{
    public class SendFileSystemToServer : IFlooderEvent
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly FileSystemLogs _model;
        private readonly IEmitter _emitter;

        
        public SendFileSystemToServer(FlooderModel flooderModel)
        {
            _model    = flooderModel.Input.FileSystem;
            _emitter  = flooderModel.Output.Workers.Emitter;
        }

        public IDisposable[] Subscribe()
        {
            if (_model.Details.Any())
            {
                return _model.Details.Select(s =>
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

        private static IObservable<FileSystemEventArgs> CreateSubject(FileSystemLogs.FileSystemLog settingsDetail)
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

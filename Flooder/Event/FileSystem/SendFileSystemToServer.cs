using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Flooder.Core.Transfer;
using Flooder.Event.FileSystem.Payloads;
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
                return _model.Details.Select(model =>
                {
                    var payload = (IPayload) Activator.CreateInstance(
                        model.Payload, BindingFlags.CreateInstance, null, new object[] { }, null);

                    IObserver<FileSystemEventArgs> observer = new DefaultEventListener(model.Tag, model.Path, _emitter, payload).Create();

                    var subscribe = CreateSubject(model).Subscribe(observer);

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}, payload:{2}", model.Tag, model.Path, model.Payload.FullName);

                    return subscribe;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }

        private static IObservable<FileSystemEventArgs> CreateSubject(FileSystemLogs.FileSystemLog model)
        {
            if (!Directory.Exists(model.Path))
            {
                Logger.Warn("[{0}] will be skipped because it does not exist.", model.Path);
                return Observable.Never<FileSystemEventArgs>();
            }

            var fsw = new FileSystemWatcher(model.Path, model.File)
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

using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Flooder.Plugins;
using NLog;

namespace Flooder.Events.FileSystem
{
    public class FileSystemToServerEvent : EventBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public FileSystemToServerEvent(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as FileSystemDataSource ?? new FileSystemDataSource();
            if (source.Options.Any())
            {
                return source.Options.Select(opt =>
                {
                    var parser = (IMultipleDictionaryParser) Activator.CreateInstance(
                        opt.Parser, BindingFlags.CreateInstance, null, new object[] { }, null);

                    var listener = (FileSystemEventListenerBase) Activator.CreateInstance(
                        opt.Listner, BindingFlags.CreateInstance, null, new object[] { opt.Tag, opt.Path, opt.File, base.MessageBroker, parser }, null);

                    var r = CreateFileSystemEvent(opt).Subscribe(listener.Create());

                    Logger.Info("FileSystemEventListener start. tag:{0}, path:{1}, listner:{2}, parser:{3}", opt.Tag, opt.Path, opt.Listner.FullName, opt.Parser.FullName);

                    return r;
                })
                .ToArray();
            }

            return new IDisposable[0];
        }

        private static IObservable<FileSystemEventArgs> CreateFileSystemEvent(FileSystemDataSourceOption option)
        {
            if (!Directory.Exists(option.Path))
            {
                Logger.Warn("[{0}] will be skipped because it does not exist.", option.Path);
                return Observable.Never<FileSystemEventArgs>();
            }

            var watcher = new FileSystemWatcher(option.Path, option.File)
            {
                EnableRaisingEvents = true
            };

            return Observable.Merge(new[]
            {
                watcher.CreatedAsObservable(),
                watcher.DeletedAsObservable(),
                watcher.ChangedAsObservable(),
                watcher.RenamedAsObservable()
            });
        }
    }
}

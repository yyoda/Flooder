﻿using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using NLog;

namespace Flooder.Event.FileSystem
{
    public class SendFileSystemToServer : SendDataSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public SendFileSystemToServer(IDataSource dataSource, IMessageBroker messageBroker)
            : base(dataSource, messageBroker)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.DataSource as FileSystemDataSource ?? new FileSystemDataSource();
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

        private static IObservable<FileSystemEventArgs> CreateSubject(FileSystemDataSourceDetail source)
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

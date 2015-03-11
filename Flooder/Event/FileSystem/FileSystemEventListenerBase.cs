using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using Flooder.Core.Transfer;
using Flooder.Event.FileSystem.Payloads;
using NLog;

namespace Flooder.Event.FileSystem
{
    public abstract class FileSystemEventListenerBase : IObserver<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected string Tag { get; private set; }
        protected string Path { get; private set; }
        protected IEmitter Emitter { get; private set; }
        protected ConcurrentDictionary<string, long> FileSeekPositionStateStore { get; private set; }
        public string HostName { get; private set; }
        protected IPayload Payload { get; private set; }

        protected FileSystemEventListenerBase(string tag, string path, IEmitter emitter, IPayload payload)
        {
            Tag                        = tag;
            Path                       = path;
            Emitter                    = emitter;
            FileSeekPositionStateStore = new ConcurrentDictionary<string, long>();
            HostName                   = Dns.GetHostName();
            Payload                    = payload;
        }

        public abstract FileSystemEventListenerBase Create();
        protected abstract void OnInitAction();
        protected abstract void OnCreateAction(FileSystemEventArgs e);
        protected abstract void OnChangeAction(FileSystemEventArgs e);
        protected abstract void OnRenameAction(FileSystemEventArgs e);
        protected abstract void OnDeleteAction(FileSystemEventArgs e);

        public void OnNext(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    OnCreateAction(e);
                    return;
                case WatcherChangeTypes.Changed:
                    OnChangeAction(e);
                    return;
                case WatcherChangeTypes.Renamed:
                    OnRenameAction(e);
                    return;
                case WatcherChangeTypes.Deleted:
                    OnDeleteAction(e);
                    return;
                default:
                    throw new InvalidOperationException(string.Format("Unknown WatcherChangeTypes:{0}", e.ChangeType));
            }
        }

        public void OnError(Exception error)
        {
            Logger.FatalException("Handle error in FileSystemEventListener.", error);
        }

        public void OnCompleted()
        {
            Logger.Info("FileSystemEventListener is completed.");
        }
    }
}

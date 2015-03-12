using System;
using System.Collections.Concurrent;
using System.IO;
using NLog;

namespace Flooder.Event.FileSystem
{
    public abstract class FileSystemEventListenerBase : EventListenerBase, IObserver<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected string Path { get; private set; }
        protected ConcurrentDictionary<string, long> FileSeekPositionStateStore { get; private set; }
        protected IPayloadParser Payload { get; private set; }

        protected FileSystemEventListenerBase(string tag, string path, FlooderObject obj, IPayloadParser payload)
            : base(tag, obj)
        {
            Path                       = path;
            FileSeekPositionStateStore = new ConcurrentDictionary<string, long>();
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

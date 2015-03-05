using System;
using System.Collections.Concurrent;
using System.IO;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.FileSystem
{
    public abstract class FileSystemEventListener : IObserver<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected string Tag { get; private set; }
        protected IEmitter Emitter { get; private set; }
        protected ConcurrentDictionary<string, long> FileSeekPositionStateStore { get; private set; }
        
        protected FileSystemEventListener(string tag, IEmitter emitter)
        {
            Tag                        = tag;
            Emitter                    = emitter;
            FileSeekPositionStateStore = new ConcurrentDictionary<string, long>();
        }

        public abstract void OnInitAction(string filePath);
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

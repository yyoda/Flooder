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
        protected IPayloadParser Parser { get; private set; }

        protected FileSystemEventListenerBase(string tag, string path, FlooderObject obj, IPayloadParser parser)
            : base(tag, obj)
        {
            Path                       = path;
            FileSeekPositionStateStore = new ConcurrentDictionary<string, long>();
            Parser                     = parser;
        }

        public abstract FileSystemEventListenerBase Create();
        protected virtual void OnInitAction() { }
        protected virtual void OnCreateAction(FileSystemEventArgs e) { }
        protected virtual void OnChangeAction(FileSystemEventArgs e) { }
        protected virtual void OnRenameAction(FileSystemEventArgs e) { }
        protected virtual void OnDeleteAction(FileSystemEventArgs e) { }

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

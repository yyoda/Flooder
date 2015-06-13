using System;
using System.Collections.Concurrent;
using System.IO;
using Flooder.Plugins;
using NLog;

namespace Flooder.Events.FileSystem
{
    public abstract class FileSystemEventListenerBase : EventListenerBase<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected string Path { get; private set; }
        protected string FileName { get; private set; }
        protected ConcurrentDictionary<string, long> FileSeekPositionStateStore { get; private set; }
        protected IMultipleDictionaryParser Parser { get; private set; }

        protected FileSystemEventListenerBase(string tag, string path, string fileName, IMessageBroker messageBroker, IMultipleDictionaryParser parser)
            : base(tag, messageBroker)
        {
            Path                       = path;
            FileName                   = fileName;
            FileSeekPositionStateStore = new ConcurrentDictionary<string, long>();
            Parser                     = parser;
        }

        public FileSystemEventListenerBase Create()
        {
            this.OnInit();
            return this;
        }

        protected virtual void OnInit() { }
        protected virtual void OnCreate(FileSystemEventArgs e) { }
        protected virtual void OnChange(FileSystemEventArgs e) { }
        protected virtual void OnRename(FileSystemEventArgs e) { }
        protected virtual void OnDelete(FileSystemEventArgs e) { }

        public override void OnNext(FileSystemEventArgs e)
        {
            try
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        OnCreate(e);
                        return;
                    case WatcherChangeTypes.Changed:
                        OnChange(e);
                        return;
                    case WatcherChangeTypes.Renamed:
                        OnRename(e);
                        return;
                    case WatcherChangeTypes.Deleted:
                        OnDelete(e);
                        return;
                    default:
                        throw new InvalidOperationException(string.Format("Unknown WatcherChangeTypes:{0}", e.ChangeType));
                }
            }
            catch (Exception ex)
            {
                Logger.DebugException("Skip because an error has occurred in FileSystemEventListener.", ex);
            }
        }

        public override void OnError(Exception error)
        {
            Logger.FatalException("Handle error in FileSystemEventListener.", error);
        }

        public override void OnCompleted()
        {
            Logger.Info("FileSystemEventListener is completed.");
        }
    }
}

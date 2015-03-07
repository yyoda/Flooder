using System;
using System.IO;
using System.Reactive.Linq;
using NLog;

namespace Flooder.Event.FileSystem
{
    internal static class FileSystemWatcherExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IObservable<FileSystemEventArgs> CreatedAsObservable(this FileSystemWatcher watcher)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (sender, e) => h(e),
                h =>
                {
                    watcher.Created += h;
                    Logger.Debug("watch file created event for {0}", watcher.Path);
                },
                h =>
                {
                    watcher.Created -= h;
                    Logger.Debug("wnwatch file created event for {0}", watcher.Path);
                });
        }

        public static IObservable<FileSystemEventArgs> DeletedAsObservable(this FileSystemWatcher watcher)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (sender, e) => h(e),
                h =>
                {
                    watcher.Deleted += h;
                    Logger.Debug("watch file deleted event for {0}", watcher.Path);
                },
                h =>
                {
                    watcher.Deleted -= h;
                    Logger.Debug("unwatch file deleted event for {0}", watcher.Path);
                });
        }

        public static IObservable<RenamedEventArgs> RenamedAsObservable(this FileSystemWatcher watcher)
        {
            return Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>(
                h => (sender, e) => h(e),
                h =>
                {
                    watcher.Renamed += h;
                    Logger.Debug("watch file renamed event for {0}", watcher.Path);
                },
                h =>
                {
                    watcher.Renamed -= h;
                    Logger.Debug("unwatch file renamed event for {0}", watcher.Path);
                });
        }

        public static IObservable<FileSystemEventArgs> ChangedAsObservable(this FileSystemWatcher watcher)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                h => (sender, e) => h(e),
                h =>
                {
                    watcher.Changed += h;
                    Logger.Debug("watch file changed event for {0}", watcher.Path);
                },
                h =>
                {
                    watcher.Changed -= h;
                    Logger.Debug("unwatch file changed event for {0}", watcher.Path);
                });
        }
    }
}
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Flooder.Core.Logging;
using NLog;

namespace Flooder.EventLog
{
    internal static class EventLogWatcherExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IObservable<EntryWrittenEventArgs> EntryWrittenAsObservable(this System.Diagnostics.EventLog watcher)
        {
            return Observable.FromEvent<EntryWrittenEventHandler, EntryWrittenEventArgs>(
                h => (sender, e) => h(e),
                h =>
                {
                    watcher.EntryWritten += h;
                    Logger.Debug("watch written eventlog for {0}", watcher.LogDisplayName);
                },
                h =>
                {
                    watcher.EntryWritten -= h;
                    Logger.Debug("unwatch written eventlog for {0}", watcher.LogDisplayName);
                });
        }
    }
}
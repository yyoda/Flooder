using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Flooder.Plugins;
using NLog;

namespace Flooder.Events.FileSystem
{
    public class DefaultEventListener : FileSystemEventListenerBase
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>inject option.</summary>
        public static Func<string, string, string> TagGen = (tag, fileName) => tag;

        public DefaultEventListener(string tag, string path, string fileName, IMessageBroker messageBroker, IMultipleDictionaryParser parser)
            : base(tag, path, fileName, messageBroker, parser)
        {
        }

        protected override void OnInit()
        {
            foreach (var path in Directory.GetFiles(base.Path, base.FileName))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    FileSeekPositionStateStore.TryAdd(path, fs.Length);
                }
            }
        }

        protected override void OnCreate(FileSystemEventArgs e)
        {
            base.FileSeekPositionStateStore.TryAdd(e.FullPath, 0);
            this.OnChange(e);
        }

        protected override void OnChange(FileSystemEventArgs e)
        {
            using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding))
            {
                long lastTimePosition;
                fs.Position = base.FileSeekPositionStateStore
                    .TryGetValue(e.FullPath, out lastTimePosition) ? lastTimePosition : 0;

                var buffer = sr.ReadToEnd();
                if (buffer.Length <= 0) return;

                var tag = TagGen(base.Tag, e.Name);

                var payload = base.Parser.Parse(buffer);

                base.FileSeekPositionStateStore
                    .AddOrUpdate(e.FullPath, key => fs.Position, (key, value) => fs.Position);
 
                base.Publish(tag, payload);
            }
        }

        protected override void OnRename(FileSystemEventArgs e)
        {
            //プロセスロックでIOExceptionが発生する可能性を考慮して３回リトライさせる
            Observable.Create<Unit>(observer =>
            {
                long _; //unused.

                try
                {
                    using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        base.FileSeekPositionStateStore.TryAdd(e.FullPath, fs.Length);

                        foreach (var state in base.FileSeekPositionStateStore.Where(x => !File.Exists(x.Key)))
                        {
                            base.FileSeekPositionStateStore.TryRemove(state.Key, out _);
                        }
                    }

                    observer.OnCompleted();
                }
                catch (FileNotFoundException)
                {
                    base.FileSeekPositionStateStore.TryRemove(e.FullPath, out _);
                    observer.OnCompleted();
                }
                catch (IOException ioe)
                {
                    Thread.Sleep(300);
                    observer.OnError(ioe);
                }

                return Disposable.Empty;
            })
            .Retry(3)
            .Subscribe(x => { },
                ex => Logger.DebugException("File access error.", ex),
                () => Logger.Trace("Renamed at {0}", e.FullPath)
            );
        }

        protected override void OnDelete(FileSystemEventArgs e)
        {
            long _; //unused.
            base.FileSeekPositionStateStore.TryRemove(e.FullPath, out _);
        }
    }
}

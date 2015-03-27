using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using NLog;

namespace Flooder.Event.FileSystem
{
    public class DefaultEventListener : FileSystemEventListenerBase
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>inject option.</summary>
        public static Func<string, string, string> TagGen = (tag, fileName) => tag;

        public DefaultEventListener(string tag, string path, IMessageBroker messageBroker, IPayloadParser parser)
            : base(tag, path, messageBroker, parser)
        {
        }

        protected override void OnInitAction()
        {
            foreach (var path in Directory.GetFiles(base.Path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    FileSeekPositionStateStore.AddOrUpdate(path, key => fs.Length, (key, value) => fs.Length);
                }
            }
        }

        protected override void OnCreateAction(FileSystemEventArgs e)
        {
            this.OnChangeAction(e);
        }

        protected override void OnChangeAction(FileSystemEventArgs e)
        {
            using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding))
            {
                fs.Position = base.FileSeekPositionStateStore.GetOrAdd(e.FullPath, key => 0);

                var buffer = sr.ReadToEnd();
                if (buffer.Length <= 0) return;

                var tag = TagGen(base.Tag, e.Name);

                var payload = base.Parser.Parse(buffer);

                base.FileSeekPositionStateStore.AddOrUpdate(e.FullPath, key => fs.Position, (key, value) => fs.Position);
                base.Publish(tag, payload);
            }
        }

        protected override void OnRenameAction(FileSystemEventArgs e)
        {
            //プロセスロック対策
            Observable.Create<Unit>(observer =>
            {
                try
                {
                    using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        base.FileSeekPositionStateStore.AddOrUpdate(e.FullPath, key => fs.Length, (key, value) => fs.Length);

                        var removeFiles = base.FileSeekPositionStateStore.Where(x => !File.Exists(x.Key)).Select(x => x.Key).ToArray();

                        for (var i = 0; i < removeFiles.Length; i++)
                        {
                            long _; //unused.
                            base.FileSeekPositionStateStore.TryRemove(removeFiles[i], out _);
                        }
                    }

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
            .Subscribe(x => { }, ex => Logger.WarnException("File access error.", ex));
        }

        protected override void OnDeleteAction(FileSystemEventArgs e)
        {
            long _; //unused.
            base.FileSeekPositionStateStore.TryRemove(e.FullPath, out _);
        }
    }
}

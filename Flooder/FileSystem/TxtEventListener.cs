using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flooder.Core.Transfer;

namespace Flooder.FileSystem
{
    public class TxtEventListener : FileSystemEventListener
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        /// <summary>inject option.</summary>
        public static Func<string, string, string> TagGen =
            (tag, fileName) => string.Format("{0}-{1}", tag, fileName.Split('.').FirstOrDefault() ?? "unknown");

        public TxtEventListener(string tag, IEmitter emitter) : base(tag, emitter)
        {
        }

        public override void OnInitAction(string filePath)
        {
            foreach (var path in Directory.GetFiles(filePath))
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

                var payload = new Dictionary<string, object>
                {
                    {"messages", buffer}
                };

                base.FileSeekPositionStateStore.AddOrUpdate(e.FullPath, key => fs.Position, (key, value) => fs.Position);
                Task.Factory.StartNew(() => base.Emitter.Emit(tag, payload));
            }
        }

        protected override void OnRenameAction(FileSystemEventArgs e)
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
        }

        protected override void OnDeleteAction(FileSystemEventArgs e)
        {
            long _; //unused.
            base.FileSeekPositionStateStore.TryRemove(e.FullPath, out _);
        }

        public static FileSystemEventListener Create(string tag, string filePath, IEmitter emitter)
        {
            var listener = new TxtEventListener(tag, emitter);
            listener.OnInitAction(filePath);
            return listener;
        }

    }
}

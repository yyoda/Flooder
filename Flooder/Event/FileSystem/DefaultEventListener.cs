using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flooder.Core.Transfer;

namespace Flooder.Event.FileSystem
{
    public class DefaultEventListener : FileSystemEventListenerBase
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        /// <summary>inject option.</summary>
        public static Func<string, string, string> TagGen =
            (tag, fileName) => string.Format("{0}.{1}", tag, fileName.Split('.').FirstOrDefault() ?? "unknown");

        public DefaultEventListener(string tag, IEmitter emitter) : base(tag, emitter)
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

                var payload = CreatePayload(buffer);

                base.FileSeekPositionStateStore.AddOrUpdate(e.FullPath, key => fs.Position, (key, value) => fs.Position);
                Task.Factory.StartNew(() => base.Emitter.Emit(tag, payload));
            }
        }

        protected virtual IDictionary<string, object> CreatePayload(string source)
        {
            return new Dictionary<string, object>
            {
                {"hostname", base.HostName},
                {"messages", source},
            };
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

        public override FileSystemEventListenerBase Create(string filePath)
        {
            var listener = new DefaultEventListener(base.Tag, base.Emitter);
            listener.OnInitAction(filePath);
            return listener;
        }

    }
}

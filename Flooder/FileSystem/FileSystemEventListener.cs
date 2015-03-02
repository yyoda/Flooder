using Flooder.Core.Transfer;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Flooder.FileSystem
{
    public class FileSystemEventListener : IObserver<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<string, string> _tag;
        private readonly IEmitter _emitter;

        public ConcurrentDictionary<string, long> FileSeekStore { get; set; }

        public FileSystemEventListener(Func<string, string> tag, IEmitter emitter)
        {
            FileSeekStore = new ConcurrentDictionary<string, long>();
            _tag          = tag;
            _emitter      = emitter;
        }

        public void InitFileSeekStore(string fullPath)
        {
            foreach (var filePath in Directory.GetFiles(fullPath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    var _ = sr.ReadToEnd();
                    FileSeekStore.AddOrUpdate(filePath, key => fs.Position, (key, value) => fs.Position);
                }
            }
        }

        public void OnNext(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        fs.Position = FileSeekStore.GetOrAdd(e.FullPath, key => 0);

                        var buffer = sr.ReadToEnd();
                        if (buffer.Length <= 0) return;

                        var tag = _tag(e.Name);

                        #region old code.
                        //const int newLineSize = 2;
                        //var indexes = (new[] { 0 })
                        //    .Union(
                        //        Regex.Matches(buffer,
                        //            "\r\n[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2},[0-9]{3}")
                        //            .OfType<Match>()
                        //            .Select(x => x.Index + newLineSize)
                        //    ).ToArray();


                        //var payload = indexes.Select((index, count) =>
                        //{
                        //    var posSet = (count + 1) < indexes.Length
                        //        ? new KeyValuePair<int, int>(indexes[count], indexes[count + 1] - indexes[count] - newLineSize)
                        //        : new KeyValuePair<int, int>(indexes[count], buffer.Length - indexes[count]);

                        //    return new { Key = count, Value = buffer.Substring(posSet.Key, posSet.Value) };
                        //})
                        //.ToDictionary(x => x.Key.ToString(), x => (object)x.Value);
                        #endregion

                        var payload = new Dictionary<string, object>
                        {
                            { "message", buffer }
                        };

                        Task.Factory.StartNew(() => _emitter.Emit(tag, payload));
                        FileSeekStore.AddOrUpdate(e.FullPath, key => fs.Position, (key, value) => fs.Position);
                        return;
                    }
                case WatcherChangeTypes.Renamed:
                    using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        FileSeekStore.AddOrUpdate(e.FullPath, key => fs.Length, (key, value) => fs.Length);
                        return;
                    }
                case WatcherChangeTypes.Deleted:
                    long _; //unused
                    FileSeekStore.TryRemove(e.FullPath, out _);
                    return;
                default:
                    throw new InvalidOperationException(string.Format("unknown WatcherChangeTypes:{0}", e.ChangeType));
            }
        }

        public void OnError(Exception error)
        {
            Logger.ErrorException("FileSystemEventListener", error);
        }

        public void OnCompleted()
        {
            Logger.Debug("FileSystemEventListener#OnCompleted");
        }
    }
}

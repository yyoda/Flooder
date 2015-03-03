using Flooder.Core.Transfer;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flooder.FileSystem
{
    public class FileSystemEventListener : IObserver<FileSystemEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<string, long> _fileSeekStore;
        private readonly string _tag;
        private readonly IEmitter _emitter;

        /// <summary>inject option.</summary>
        public static Func<string, string, string> TagGen =
            (tag, fileName) => string.Format("{0}.{1}", tag, fileName.Split('.').FirstOrDefault() ?? "unknown");

        public FileSystemEventListener(string tag, IEmitter emitter)
        {
            _fileSeekStore = new ConcurrentDictionary<string, long>();
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
                    _fileSeekStore.AddOrUpdate(filePath, key => fs.Position, (key, value) => fs.Position);
                }
            }
        }

        public void OnNext(FileSystemEventArgs e)
        {
            long _; //unused
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.GetEncoding("Shift_JIS")))
                    {
                        fs.Position = _fileSeekStore.GetOrAdd(e.FullPath, key => 0);

                        var buffer = sr.ReadToEnd();
                        if (buffer.Length <= 0) return;

                        var tag = TagGen(_tag, e.Name);

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

                        _fileSeekStore.AddOrUpdate(e.FullPath, key => fs.Position, (key, value) => fs.Position);
                        Task.Factory.StartNew(() => _emitter.Emit(tag, payload));
                        return;
                    }
                case WatcherChangeTypes.Renamed:
                    using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        _fileSeekStore.AddOrUpdate(e.FullPath, key => fs.Length, (key, value) => fs.Length);

                        var removeFiles = _fileSeekStore.Where(x => !File.Exists(x.Key)).Select(x => x.Key).ToArray();

                        for (var i = 0; i < removeFiles.Length; i++)
                        {
                            _fileSeekStore.TryRemove(removeFiles[i], out _);
                        }

                        return;
                    }
                case WatcherChangeTypes.Deleted:
                    _fileSeekStore.TryRemove(e.FullPath, out _);
                    return;
                default:
                    throw new InvalidOperationException(string.Format("unknown WatcherChangeTypes:{0}", e.ChangeType));
            }
        }

        public void OnError(Exception error)
        {
            Logger.FatalException("FileSystemEventListener", error);
        }

        public void OnCompleted()
        {
            Logger.Fatal("FileSystemEventListener#OnCompleted");
        }
    }
}

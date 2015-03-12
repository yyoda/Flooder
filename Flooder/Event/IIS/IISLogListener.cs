using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Flooder.Event.IIS
{
    internal class IISLogListener : EventListenerBase, IObserver<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        //inject option.
        public static readonly HashSet<string> IntValues = new HashSet<string>(new[]
        {
            "time-taken", "sc-status",
            "sc-substatus", "sc-win32-status"
        });

        private readonly string _filePath;

        private string[] _fields;
        private Tuple<string, long> _fileSeekPositionStateStore;

        public IISLogListener(string tag, string filePath, FlooderObject obj)
            : base(tag, obj)
        {
            _filePath                   = filePath;
            _fields                     = new string[0];
            _fileSeekPositionStateStore = null;
        }

        public void OnInitAction()
        {
            string fullPath;
            DateTime lastWriteTime;

            if (TryGetLatestFile(_filePath, out fullPath, out lastWriteTime))
            {
                using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!line.StartsWith("#Fields")) continue;
                        _fields = line.Split(' ').Skip(1)
                            .Select(x => x == "date" ? "w-date" : x)
                            .Select(x => x == "time" ? "w-time" : x)
                            .ToArray();
                        break;
                    }
                }
            }
        }

        public void OnNext(long value)
        {
            string fullPath;
            DateTime lastWriteTime;

            if (TryGetLatestFile(_filePath, out fullPath, out lastWriteTime))
            {
                using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding))
                {
                    if (_fileSeekPositionStateStore == null)
                    {
                        //initial access.
                        _fileSeekPositionStateStore = new Tuple<string, long>(fullPath, 0);
                    }
                    else
                    {
                        //arrival new file.
                        if (_fileSeekPositionStateStore.Item1 != fullPath)
                        {
                            _fileSeekPositionStateStore = new Tuple<string, long>(fullPath, 0);
                        }
                    }

                    var stateStore = _fileSeekPositionStateStore;
                    var isFirst = stateStore.Item2 == 0;
                    fs.Position = stateStore.Item2;

                    //Logger.Debug("[IISLog READ START] _filePath:{0}, fullPath:{1}, lastWriteTime:{2}, fs.Position:{3}", _filePath, fullPath, lastWriteTime, fs.Position);

                    string line;
                    Dictionary<string, object> payload = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length <= 0) continue;

                        if (line[0] == '#')
                        {
                            if (line.StartsWith("#Fields"))
                            {
                                _fields = line.Split(' ').Skip(1)
                                    .Select(x => x == "date" ? "w-date" : x)
                                    .Select(x => x == "time" ? "w-time" : x)
                                    .ToArray();
                            }

                            continue;
                        }

                        payload = line.Split(' ').Select((x, idx) =>
                        {
                            string key = _fields[idx];
                            object val = IntValues.Contains(key) ? (object)int.Parse(x) : x;
                            
                            return new
                            {
                                Key   = key,
                                Value = val,
                            };
                        })
                        .ToDictionary(x => x.Key, x => x.Value);

                        if (!isFirst)
                        {
                            string cache = line;

                            var pl = cache.Split(' ').Select((x, idx) =>
                            {
                                string key = _fields[idx];
                                object val = IntValues.Contains(key) ? (object) int.Parse(x) : x;

                                return new
                                {
                                    Key   = key,
                                    Value = val,
                                };
                            })
                            .ToDictionary(x => x.Key, x => x.Value);

                            base.Emit(pl);
                        }
                    }

                    if (isFirst && payload != null)
                    {
                        base.Emit(payload);
                    }

                    _fileSeekPositionStateStore = Tuple.Create(fullPath, fs.Position);
                }
            }
        }

        public void OnError(Exception error)
        {
            Logger.FatalException("IISLogListener", error);
        }

        public void OnCompleted()
        {
            Logger.Fatal("IISLogListener#OnCompleted");
        }

        /// <summary>
        /// 指定フォルダから最新のファイルを一つ取り、ファイルのフルパス、最終書き込み日時を返す
        /// </summary>
        /// <param name="filePath">指定フォルダ</param>
        /// <param name="latestFilePath">フルパス</param>
        /// <param name="lastWriteTime">最終書き込み日時</param>
        /// <returns>ファイルが存在しなかったらfalse、そうでなければtrue</returns>
        private static bool TryGetLatestFile(string filePath, out string latestFilePath, out DateTime lastWriteTime)
        {
            string tmpLatestFilePath = null;
            var tmpLastWriteTime = new DateTime();
            var result = false;

            foreach (var path in Directory.GetFiles(filePath))
            {
                var nowTime = File.GetLastWriteTime(path);
                if (tmpLastWriteTime < nowTime)
                {
                    tmpLastWriteTime = nowTime;
                    tmpLatestFilePath = path;
                    result = true;
                }
            }

            latestFilePath = tmpLatestFilePath;
            lastWriteTime = tmpLastWriteTime;
            return result;
        }
    }
}

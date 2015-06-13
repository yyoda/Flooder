using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace Flooder.Events.IIS
{
    internal class IISLogListener : EventListenerBase<long>
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

        public IISLogListener(string tag, string filePath, IMessageBroker messageBroker)
            : base(tag, messageBroker)
        {
            _filePath                   = filePath;
            _fields                     = new string[0];
            _fileSeekPositionStateStore = null;
        }

        public void OnInit()
        {
            string fullPath;
            DateTime lastWriteTime;

            if (base.TryGetLatestFile(_filePath, out fullPath, out lastWriteTime))
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

        public override void OnNext(long value)
        {
            try
            {
                string fullPath;
                DateTime lastWriteTime;
                if (!base.TryGetLatestFile(_filePath, out fullPath, out lastWriteTime))
                {
                    return;
                }

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

                    fs.Position = _fileSeekPositionStateStore.Item2;

                    //Logger.Debug("[IISLog READ START] _filePath:{0}, fullPath:{1}, lastWriteTime:{2}, fs.Position:{3}", _filePath, fullPath, lastWriteTime, fs.Position);

                    string line;
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

                        if (_fileSeekPositionStateStore.Item2 == 0) continue;

                        var payload = line.Split(' ').Select((x, idx) =>
                        {
                            string key = _fields[idx];
                            object val = IntValues.Contains(key) ? (object)int.Parse(x) : x;

                            return new
                            {
                                Key = key,
                                Value = val,
                            };
                        })
                        .ToDictionary(x => x.Key, x => x.Value);

                        base.Publish(payload);
                    }

                    _fileSeekPositionStateStore = Tuple.Create(fullPath, fs.Position);
                }
            }
            catch (Exception ex)
            {
                Logger.WarnException("Skip because an error has occurred in IISLogListener.", ex);
                throw;
            }
        }

        public override void OnError(Exception error)
        {
            Logger.FatalException("IISLogListener", error);
        }

        public override void OnCompleted()
        {
            Logger.Fatal("IISLogListener#OnCompleted");
        }
    }
}

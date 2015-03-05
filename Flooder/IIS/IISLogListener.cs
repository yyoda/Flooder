﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.IIS
{
    internal class IISLogListener : IObserver<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        private readonly string _tag;
        private readonly string _filePath;
        private readonly IEmitter _emitter;

        private string[] _fields;
        private Tuple<string, long> _fileSeekPositionStateStore;

        public IISLogListener(string tag, string filePath, IEmitter emitter)
        {
            _tag                        = tag;
            _filePath                   = filePath;
            _emitter                    = emitter;
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
                        _fields = line.Split(' ').Skip(1).ToArray();
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
                    var stateStore = _fileSeekPositionStateStore ?? (_fileSeekPositionStateStore = new Tuple<string, long>(fullPath, 0));
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
                                _fields = line.Split(' ').Skip(1).ToArray();
                            }

                            continue;
                        }

                        payload = line.Split(' ').Select((x, idx) => new
                        {
                            Key = _fields[idx],
                            Value = x,
                        })
                        .ToDictionary(x => x.Key, x => (object)x.Value);

                        if (!isFirst)
                        {
                            string cache = line;
                            Task.Factory.StartNew(() =>
                            {
                                _emitter.Emit(_tag, cache.Split(' ').Select((x, idx) => new
                                {
                                    Key   = _fields[idx],
                                    Value = x,
                                })
                                .ToDictionary(x => x.Key, x => (object) x.Value));
                            });
                        }
                    }

                    if (isFirst && payload != null)
                    {
                        Task.Factory.StartNew(() => _emitter.Emit(_tag, payload));
                    }

                    _fileSeekPositionStateStore = Tuple.Create(fullPath, fs.Position);

                    //Logger.Debug("[IISLog READ END] fs.Position:{0}", fs.Position);
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
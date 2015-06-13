using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Flooder.Plugins;
using NLog;

namespace Flooder.Events.FileLoad
{
    internal class CsvFileLoadListener : EventListenerBase<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        private readonly string _filePath, _fileName;
        private readonly IMultipleDictionaryParser _parser;

        public CsvFileLoadListener(string tag, string filePath, string fileName, IMessageBroker messageBroker, IMultipleDictionaryParser parser)
            : base(tag, messageBroker)
        {
            _filePath = filePath;
            _fileName = base.ChangeRegexPattern(fileName);
            _parser   = parser;
        }

        public override void OnNext(long value)
        {
            try
            {
                var now = DateTime.Now;

                var paths = Directory.GetFiles(_filePath)
                    .Select(x => new { Path = x, LastWriteTime = File.GetLastWriteTime(x) })
                    .Where(x => Regex.IsMatch(x.Path, _fileName))
                    .Where(x => x.LastWriteTime < now.AddSeconds(-1))   //書き込み直前のファイルに遭遇しないための対策
                    .OrderBy(x => x.LastWriteTime)
                    .Select(x => x.Path);

                foreach (var path in paths)
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding))
                    {
                        var lines = sr.ReadToEnd();
                        if (lines.Length > 0)
                        {
                            var payloads = _parser.Parse(lines);

                            if (payloads.Any())
                            {
                                base.Publish(payloads);
                                Logger.Debug("FileLoadListener#OnNext publish payloads:{0}.", payloads.Length);
                            }
                        }
                    }

                    Thread.Sleep(1000); //ファイルを完全に開放するまで待たないと例外を吐くことがある

                    File.Delete(path);

                    Logger.Debug("FileLoadListener#OnNext file deleted, at {0}", path);
                }
            }
            catch (Exception ex)
            {
                Logger.WarnException("Skip because an error has occurred in FileLoadListener.", ex);
            }
        }

        public override void OnError(Exception error)
        {
            Logger.FatalException("FileLoadListener#OnError", error);
        }

        public override void OnCompleted()
        {
            Logger.Fatal("FileLoadListener#OnCompleted");
        }
    }
}

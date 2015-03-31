using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Flooder.Event.FileLoad
{
    public class BlgFileLoadListener : EventListenerBase<long>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>path of cmd.exe</summary>
        private static readonly string ComSpec = Environment.GetEnvironmentVariable("ComSpec");
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        private readonly string _filePath;
        private readonly IPayloadParser _parser;
        private DateTime _currentlastWriteTime;

        public BlgFileLoadListener(string tag, string filePath, string fileName, IMessageBroker messageBroker, IPayloadParser parser)
            : base(tag, messageBroker)
        {
            _currentlastWriteTime = new DateTime();
            _filePath             = filePath;
            _parser               = parser;
        }

        public override void OnNext(long value)
        {
            try
            {
                DateTime lastWriteTime;
                string fullPath;

                if (!base.TryGetLatestFile(_filePath, out fullPath, out lastWriteTime))
                {
                    Logger.Warn("File not found. search path:{0}", _filePath);
                    return;
                }
                
                if (_currentlastWriteTime < lastWriteTime)
                {
                    _currentlastWriteTime = lastWriteTime;
                }
                else
                {
                    Logger.Trace("New file not found. search path:{0}", _filePath);
                    return;
                }

                var payloads = this.LoadPayloads(fullPath);
                if (payloads.Length > 0)
                {
                    base.Publish(payloads);
                }
            }
            catch (Exception ex)
            {
                Logger.WarnException("Skip because an error has occurred in FileLoadListener.", ex);
            }
        }

        public override void OnError(Exception error)
        {
            Logger.ErrorException("FileLoadListener#OnError", error);
        }

        public override void OnCompleted()
        {
            Logger.Fatal("FileLoadListener#OnCompleted");
        }

        private Dictionary<string, object>[] LoadPayloads(string fullPath)
        {
            var blgFilePath = fullPath;
            var csvFilePath = blgFilePath.Replace(".blg", ".csv");

            var startInfo = new ProcessStartInfo
            {
                FileName               = ComSpec,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardInput  = false,
                CreateNoWindow         = true,
                Arguments              = string.Format(@"/c relog {0} -f CSV -o {1} -y", blgFilePath, csvFilePath)
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException("process start failure.");
                }

                var stdout = process.StandardOutput.ReadToEnd();

                if (!process.WaitForExit(3000))
                {
                    throw new TimeoutException("relog command timeout.");
                }

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(stdout);
                }

                Logger.Info("generated tmp file." + csvFilePath);

                process.Close();
            }

            var result = new Dictionary<string, object>[0];

            using (var fs = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding))
            {
                var buffer = sr.ReadToEnd();

                Logger.Info("loaded tmp file.");

                if (buffer.Length > 0)
                {
                    result = _parser.MultipleParse(buffer);

                    Logger.Info("parsed payload.");
                }
            }

            IEnumerable<Exception> exceptions;
            if (!base.TryRemoveFile(csvFilePath, out exceptions))
            {
                throw new AggregateException(string.Format("remove file error. path:{0}", csvFilePath), exceptions);
            }

            Logger.Info("removed temp file. at {0}", csvFilePath);

            return result;
        }
    }
}

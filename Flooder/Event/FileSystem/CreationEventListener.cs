using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;

namespace Flooder.Event.FileSystem
{
    public class CreationEventListener : FileSystemEventListenerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        public CreationEventListener(string tag, string path, IMessageBroker messageBroker, IPayloadParser parser)
            : base(tag, path, messageBroker, parser)
        {
        }

        protected override void OnCreateAction(FileSystemEventArgs e)
        {
            try
            {
                Thread.Sleep(1000);

                using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding))
                {
                    var buffer = sr.ReadToEnd();
                    if (buffer.Length > 0)
                    {
                        var payloads = base.Parser.MultipleParse(buffer);

                        if (payloads.Any())
                        {
                            base.Publish(payloads);
                            Logger.Debug("CreationEventListener#OnCreateAction publish payloads[{0}].", payloads.Count());
                        }
                    }
                }

                Thread.Sleep(1000);

                File.Delete(e.FullPath);

                Logger.Debug("CreationEventListener#OnCreateAction file deleted, at {0}", e.FullPath);
            }
            catch (Exception ex)
            {
                Logger.WarnException(string.Format("File access error at {0}, skip of this action.", e.FullPath), ex);
            }
        }
    }
}

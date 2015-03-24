using System;
using System.IO;
using System.Text;
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

        public override FileSystemEventListenerBase Create()
        {
            return this;
        }

        protected override void OnCreateAction(FileSystemEventArgs e)
        {
            using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding))
            {
                var buffer = sr.ReadToEnd();
                if (buffer.Length > 0)
                {
                    var payloads = base.Parser.MultipleParse(buffer);

                    base.Publish(payloads);
                }
            }

            RemoveFile(e.FullPath);
        }

        private static void RemoveFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.WarnException(string.Format("Remove file error. filePath:{0}", filePath), ex);
            }
        }
    }
}

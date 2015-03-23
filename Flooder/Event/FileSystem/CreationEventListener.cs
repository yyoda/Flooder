using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;

namespace Flooder.Event.FileSystem
{
    public class CreationEventListener : FileSystemEventListenerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Encoding Encoding = Encoding.GetEncoding("Shift_JIS");

        /// <summary>inject option.</summary>
        public static Func<string, string, string> TagGen =
            (tag, fileName) => string.Format("{0}.{1}", tag, fileName.Split('.').FirstOrDefault() ?? "unknown");

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
                if (buffer.Length <= 0)
                {
                    RemoveFile(e.FullPath);
                    return;
                }

                var tag = TagGen(base.Tag, e.Name);
                var payload = base.Parser.MultipleParse(buffer);

                base.Publish(tag, payload);
                RemoveFile(e.FullPath);
            }
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

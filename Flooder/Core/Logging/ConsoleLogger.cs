using System;

namespace Flooder.Core.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine("[{0}] [INFO] {1}", DateTimeOffset.Now, message);
        }

        public void Info(string format, params object[] args)
        {
            Info(string.Format(format, args));
        }

        public void Warn(string message)
        {
            Console.WriteLine("[{0}] [WARN] {1}", DateTimeOffset.Now, message);
        }

        public void Debug(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public void Debug(string message)
        {
            Console.WriteLine("[{0}] [DEBUG] {1}", DateTimeOffset.Now, message);
        }

        public void Warn(string format, params object[] args)
        {
            Warn(string.Format(format, args));
        }

        public void Error(string message)
        {
            Console.WriteLine("[{0}] [ERROR] {1}", DateTimeOffset.Now, message);
        }

        public void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public void Fatal(string message)
        {
            Console.WriteLine("[{0}] [FATAL] {1}", DateTimeOffset.Now, message);
        }

        public void Fatal(string format, params object[] args)
        {
            Fatal(string.Format(format, args));
        }
    }
}

using NLog;

namespace Flooder.Core.Logging
{
    public static class NLogLoggerExtensions
    {
        public static NLogLogger Apply(this NLog.Logger @this)
        {
            return new NLogLogger(@this);
        }
    }

    public class NLogLogger : ILogger
    {
        private readonly Logger _logger;

        public NLogLogger(Logger logger)
        {
            _logger = logger;
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Info(string format, params object[] args)
        {
            Info(string.Format(format, args));
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Debug(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Warn(string format, params object[] args)
        {
            Warn(string.Format(format, args));
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(string format, params object[] args)
        {
            Fatal(string.Format(format, args));
        }
    }
}

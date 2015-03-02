namespace Flooder.Core.Logging
{
    public class NullLogger : ILogger
    {
        public void Info(string message)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Warn(string message)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Debug(string message)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Error(string message)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Fatal(string message)
        {
        }

        public void Fatal(string format, params object[] args)
        {
        }
    }
}

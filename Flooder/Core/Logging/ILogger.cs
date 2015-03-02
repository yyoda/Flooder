namespace Flooder.Core.Logging
{
    public interface ILogger
    {
        void Error(string format, params object[] args);
        void Error(string message);
        void Fatal(string format, params object[] args);
        void Fatal(string message);
        void Info(string format, params object[] args);
        void Info(string message);
        void Warn(string format, params object[] args);
        void Warn(string message);
        void Debug(string format, params object[] args);
        void Debug(string message);
    }
}

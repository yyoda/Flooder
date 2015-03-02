using System.Diagnostics;
using NLog;

namespace Flooder.Core.Logging
{
    public class LoggerFactory
    {
        private static readonly string LogType = System.Configuration.ConfigurationManager.AppSettings["log.type"];

        public static ILogger GetLogger()
        {
            switch (LogType)
            {
                case "NLog":
                    var type = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
                    return new NLogLogger(LogManager.GetLogger(type == null ? "Unknown" : type.ToString()));
                case "Console":
                    return new ConsoleLogger();
                default:
                    return new NullLogger();
            }
        }
    }
}

using NLog;
using System;
using System.IO;
using System.Reflection;

namespace Flooder.Console
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string ConfigPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Flooder.Console.exe.config";

        static void Main(string[] args)
        {
            var flooder = ServiceFactory.Create<DefaultService>();

            try
            {
                flooder.Start();

                HealthCheck.FileMonitoring(ConfigPath, flooder);

                System.Console.ReadLine();  //listening...
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Problem occurs.", ex);
            }
            finally
            {
                flooder.Stop();
            }
        }
    }
}

using NLog;
using System;

namespace Flooder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var flooder = ServiceFactory.Create<DefaultService>();

            try
            {
                flooder.Start();

                System.Console.ReadLine();  //listening...
            }
            catch (Exception ex)
            {
                logger.ErrorException("Problem occurs.", ex);
            }
            finally
            {
                flooder.Stop();
            }
        }
    }
}

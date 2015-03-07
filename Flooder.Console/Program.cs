using NLog;
using System;

namespace Flooder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            var flooder = FlooderFactory.Create<Flooder.Core.Configuration.Section>();

            try
            {
                flooder.Start();
                System.Console.ReadLine();  //listening...
            }
            catch (Exception e)
            {
                logger.ErrorException("Problem occurs.", e);
            }
            finally
            {
                flooder.Stop();
            }
        }
    }
}

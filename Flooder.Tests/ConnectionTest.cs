using System;
using Flooder.Core.Transfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace Flooder.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void HealthCheck()
        {
            var logger = LogManager.GetCurrentClassLogger();
            
            var hosts = new[]
            {
                Tuple.Create("localhost", 888),
                Tuple.Create("localhost", 999),
            };

            IDisposable subscribe = null;

            try
            {
                var tcp = new TcpConnectionManager(hosts);
                tcp.Connect();
                subscribe = tcp.HealthCheck();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                logger.ErrorException("error.", ex);
            }
            finally
            {
                if (subscribe != null)
                {
                    subscribe.Dispose();
                }
            }
        }
    }
}

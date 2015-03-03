using System;
using Flooder.Core.Transfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flooder.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public void HealthCheck()
        {
            var hosts = new[]
            {
                Tuple.Create("localhost", 888),
                Tuple.Create("localhost", 999),
            };

            IDisposable subscribe = null;

            try
            {
                var tcp = new TcpConnectionStateStore(hosts);
                tcp.Connect();
                subscribe = tcp.HealthCheck();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
            }

            if (subscribe != null)
            {
                subscribe.Dispose();
            }
        }
    }
}

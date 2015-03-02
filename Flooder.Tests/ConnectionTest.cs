using System;
using System.Runtime.InteropServices;
using System.Threading;
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
                Tuple.Create("192.168.233.162", 5140),
                Tuple.Create("192.168.233.162", 5141),
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

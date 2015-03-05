using System;
using System.Collections.Generic;
using System.Linq;
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

            var subscribe = new List<IDisposable>();

            try
            {
                var tcp = new TcpConnectionManager(hosts);
                tcp.Connect();
                subscribe = tcp.HealthCheck().ToList();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
            }

            subscribe.ForEach(x => x.Dispose());
        }
    }
}

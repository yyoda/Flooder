using Flooder.Transfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Flooder.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();
        TcpManager _tcp;

        [TestInitialize]
        public void ConnectionTestInitialize()
        {
            _tcp = new TcpManager(new[] { Tuple.Create("localhost", 9999) });
            _tcp.Connect();
        }

        [TestMethod]
        public void HealthCheck()
        {
            using (_tcp.HealthCheck())
            {
                Thread.Sleep(10000);
                //Console.ReadLine();
            }
        }

        [TestMethod]
        public void IfOverflowBlockingCollection()
        {
            const int limit = 3;
            var messageBroker = new FluentMessageBroker(_tcp, TimeSpan.FromSeconds(1), 3, limit);

            for (var i = 0; i < limit + 1; i++)
            {
                messageBroker.Publish("test.tag", new Dictionary<string, object> {{"id", i}, {"name", "foo"}});

                if (i < limit)
                    Assert.AreEqual(i, messageBroker.Count);
                else
                    Assert.AreEqual(limit, messageBroker.Count);
            }
        }

        [TestMethod]
        public void MultipleTakeBlockingCollection()
        {
            const int limit = 10;
            var messageBroker = new FluentMessageBroker(_tcp, TimeSpan.FromSeconds(1), 3, limit, 3);

            for (var i = 0; i < limit; i++)
            {
                messageBroker.Publish("test.tag", new Dictionary<string, object> { { "id", i }, { "name", "foo" } });
            }

            messageBroker.Subscribe();
        }

        [TestMethod]
        public void MultipleDataTransfer()
        {
            IDisposable subscribe = null;

            try
            {
                var messageBroker = new FluentMessageBroker(_tcp, TimeSpan.FromSeconds(1), 3, 10);
                subscribe = messageBroker.Subscribe();

                for (var i = 0; i < 10; i++)
                {
                    messageBroker.Publish("test.tag", new[]
                    {
                        new Dictionary<string, object> { {"name", "yyoda"}, {"age", 35} },
                        new Dictionary<string, object> { {"name", "yyoda"}, {"age", 36} },
                    });

                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("error.", ex);
            }

            if (subscribe != null) subscribe.Dispose();
        }

        [TestCleanup]
        public void ConnectionTestCleanup()
        {
            _tcp.Dispose();
        }
    }
}

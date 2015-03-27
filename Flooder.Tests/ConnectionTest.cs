using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Flooder.CircuitBreaker;
using Flooder.Worker;

namespace Flooder.Tests
{
    [TestClass]
    public class ConnectionTest
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();
        TcpManager _tcp;
        readonly Tuple<string, int>[] _hosts = {Tuple.Create("localhost", 9999)};

        [TestInitialize]
        public void ConnectionTestInitialize()
        {
            _tcp = new TcpManager(_hosts, new IncrementalRetryableCircuitBreaker());
            _tcp.Connect();
        }

        [TestMethod]
        public void HealthCheck()
        {
            using (_tcp.ConnectionMonitoring())
            {
                Thread.Sleep(10000);
                //Console.ReadLine();
            }
        }

        [TestMethod]
        public void IfOverflowBlockingCollection()
        {
            const int limit = 3;
            var messageBroker = new FluentMessageBroker(new MessageBrokerOption(_hosts));

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
            var messageBroker = new FluentMessageBroker(new MessageBrokerOption(_hosts)
            {
                Interval      = TimeSpan.FromSeconds(1),
                RetryMaxCount = 3,
                Capacity      = limit,
                ExtractCount    = 3,
            });

            for (var i = 0; i < limit; i++)
            {
                messageBroker.Publish("test.tag", new Dictionary<string, object> { { "id", i }, { "name", "foo" } });
            }

            messageBroker.Subscribe();
        }

        [TestMethod]
        public void MultipleDataTransfer()
        {
            IEnumerable<IDisposable> subscribe = null;

            try
            {
                var messageBroker = new FluentMessageBroker(new MessageBrokerOption(_hosts)
                {
                    Interval      = TimeSpan.FromSeconds(1),
                    RetryMaxCount = 3,
                    Capacity      = 10,
                });

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

            if (subscribe != null) subscribe.ToList().ForEach(x => x.Dispose());
        }

        [TestCleanup]
        public void ConnectionTestCleanup()
        {
            _tcp.Dispose();
        }
    }
}

using Flooder.Events.FileLoad;
using Flooder.Plugins;
using Flooder.Worker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flooder.Tests
{
    [TestClass]
    public class LoadBlgFileTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var messageBroker = new StdOutMessageBroker(new MessageBrokerOption());
            var parser = new CsvParser();
            var listener = new BlgFileLoadListener("tag", @"C:\temp\pc", "dummuy", messageBroker, parser);
            listener.OnNext(1);
        }
    }
}

using Flooder.Event.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flooder.Tests
{
    [TestClass]
    public class ParseTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var source = @"2015-03-26 21:35:25,471 [DEBUG] [stats] XXXXXXXXXXXXXXX";

            var result = new StatsParser().Parse(source);
        }
    }
}

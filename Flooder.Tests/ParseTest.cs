using System.IO;
using System.Text;
using Flooder.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flooder.Tests
{
    [TestClass]
    public class ParseTest
    {
        [TestMethod]
        public void JsonParse()
        {
            var source = "{ \"a\": \"111\", \"b\": \"222\" }\r\n{ \"a\": \"111\", \"b\": \"222\" }\n{ \"a\": \"111\", \"b\": \"222\" }";

            var result = new JsonParser().Parse(source);
        }
    }
}

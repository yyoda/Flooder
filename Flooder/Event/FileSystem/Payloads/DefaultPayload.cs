using System.Collections.Generic;

namespace Flooder.Event.FileSystem.Payloads
{
    public class DefaultPayload : IPayload
    {
        public IDictionary<string, object> Parse(string hostName, string source)
        {
            return new Dictionary<string, object>
            {
                {"hostname", hostName},
                {"messages", source},
            };
        }
    }
}

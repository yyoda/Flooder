using System.Collections.Generic;

namespace Flooder.Event.FileSystem.Payloads
{
    public class DefaultPayload : IPayloadParser
    {
        public IDictionary<string, object> Parse(string source)
        {
            return new Dictionary<string, object>
            {
                {"messages", source},
            };
        }
    }
}

using System;
using System.Collections.Generic;

namespace Flooder.Event.FileSystem.Parser
{
    public class DefaultParser : IPayloadParser
    {
        public IDictionary<string, object> Parse(string source)
        {
            return new Dictionary<string, object>
            {
                {"messages", source},
            };
        }

        public IDictionary<string, object>[] MultipleParse(string source)
        {
            throw new NotImplementedException();
        }
    }
}

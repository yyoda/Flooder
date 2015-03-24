using System;
using System.Collections.Generic;

namespace Flooder.Event.FileSystem.Parser
{
    public class DefaultParser : IPayloadParser
    {
        public Dictionary<string, object> Parse(string source)
        {
            return new Dictionary<string, object>
            {
                {"messages", source},
            };
        }

        public Dictionary<string, object>[] MultipleParse(string source)
        {
            throw new NotImplementedException();
        }
    }
}

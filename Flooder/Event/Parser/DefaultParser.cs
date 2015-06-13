using System;
using System.Collections.Generic;

namespace Flooder.Event.Parser
{
    public class DefaultParser : IParsePlugin
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

using System.Collections.Generic;

namespace Flooder.Event.Parser
{
    public class DefaultParser : IMultipleDictionaryParser
    {
        public Dictionary<string, object>[] Parse(string source)
        {
            return new[]
            {
                new Dictionary<string, object>
                {
                    {"messages", source},
                }
            };
        }
    }
}

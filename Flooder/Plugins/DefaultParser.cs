using System.Collections.Generic;
using Flooder.Events;

namespace Flooder.Plugins
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

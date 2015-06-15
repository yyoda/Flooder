using System.Collections.Generic;

namespace Flooder.Events
{
    public interface IMultipleDictionaryParser
    {
        Dictionary<string, object>[] Parse(string source);
    }
}

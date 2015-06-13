using System.Collections.Generic;

namespace Flooder.Event
{
    public interface IMultipleDictionaryParser
    {
        Dictionary<string, object>[] Parse(string source);
    }
}

using System.Collections.Generic;

namespace Flooder.Plugins
{
    public interface IMultipleDictionaryParser
    {
        Dictionary<string, object>[] Parse(string source);
    }
}

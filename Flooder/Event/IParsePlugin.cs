using System.Collections.Generic;

namespace Flooder.Event
{
    public interface IParsePlugin
    {
        Dictionary<string, object> Parse(string source);
        Dictionary<string, object>[] MultipleParse(string source);
    }
}

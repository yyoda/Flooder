using System.Collections.Generic;

namespace Flooder.Event
{
    public interface IPayloadParser
    {
        Dictionary<string, object> Parse(string source);
        Dictionary<string, object>[] MultipleParse(string source);
    }
}

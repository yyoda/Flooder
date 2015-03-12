using System.Collections.Generic;

namespace Flooder.Event.FileSystem
{
    public interface IPayloadParser
    {
        IDictionary<string, object> Parse(string source);
    }
}

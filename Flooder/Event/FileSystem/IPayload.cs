using System.Collections.Generic;

namespace Flooder.Event.FileSystem
{
    public interface IPayload
    {
        IDictionary<string, object> Parse(string hostName, string source);
    }
}

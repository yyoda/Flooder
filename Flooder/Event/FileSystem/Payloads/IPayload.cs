using System.Collections.Generic;

namespace Flooder.Event.FileSystem.Payloads
{
    public interface IPayload
    {
        IDictionary<string, object> Parse(string hostName, string source);
    }
}

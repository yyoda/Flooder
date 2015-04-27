using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flooder.Event.Parser
{
    public class JsonParser : IPayloadParser
    {
        public Dictionary<string, object> Parse(string source)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(source);
        }

        public Dictionary<string, object>[] MultipleParse(string source)
        {
            throw new NotImplementedException();
        }
    }
}

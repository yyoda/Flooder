using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace Flooder.Event.Parser
{
    public class JsonParser : IPayloadParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object> Parse(string source)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(source);
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("JsonParserのデシリアライズが失敗しました. source:{0}", source), ex);
                throw;
            }
        }

        public Dictionary<string, object>[] MultipleParse(string source)
        {
            throw new NotImplementedException();
        }
    }
}

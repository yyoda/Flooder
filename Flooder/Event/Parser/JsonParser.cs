using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.Event.Parser
{
    public class JsonParser : IPayloadParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object> Parse(string source)
        {
            try
            {
                //TODO:複数行に対応させる
                var first = source.Replace("\r\n", "\n").Split('\n').First();
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(first);
            }
            catch (Exception ex)
            {
                Logger.DebugException(string.Format("JsonParserのデシリアライズが失敗しました. source:{0}", source), ex);
                throw;
            }
        }

        public Dictionary<string, object>[] MultipleParse(string source)
        {
            try
            {
                return source.Replace("\r\n", "\n").Split('\n')
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(JsonConvert.DeserializeObject<Dictionary<string, object>>)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Logger.DebugException(string.Format("JsonParserのデシリアライズが失敗しました. source:{0}", source), ex);
                throw;
            }
        }
    }
}

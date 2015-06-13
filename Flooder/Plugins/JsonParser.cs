using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;

namespace Flooder.Plugins
{
    public class JsonParser : IMultipleDictionaryParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object>[] Parse(string source)
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

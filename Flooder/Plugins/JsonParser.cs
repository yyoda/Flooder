using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.Plugins
{
    public class JsonParser : IMultipleDictionaryParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object>[] Parse(string source)
        {
            var r = source.Replace("\r\n", "\n").Split('\n')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, object>>(x);
                    }
                    catch (Exception ex)
                    {
                        Logger.TraceException(string.Format("[SKIP] Json parse error. source:{0}", source), ex);
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToArray();

            return r;
        }
    }
}

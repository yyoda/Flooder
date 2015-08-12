using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Flooder.Events;

namespace Flooder.Plugins
{
    public class JsonEofParser : IMultipleDictionaryParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object>[] Parse(string source)
        {
            var r = source.Split(new[] { "EOF" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, object>>(x.Replace("\\", "\\\\"));
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

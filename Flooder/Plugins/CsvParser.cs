using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Plugins
{
    public class CsvParser : IMultipleDictionaryParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object>[] Parse(string source)
        {
            try
            {
                var lines = source.Replace("\r\n", "\n").Split('\n');
                var head  = lines.First().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                return lines.Skip(1).Select(row =>
                {
                    return row.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select((col, idx) =>
                        {
                            int intValue;
                            if (int.TryParse(col, out intValue))
                            {
                                return new { Key = head[idx], Value = (object)intValue };
                            }

                            return new { Key = head[idx], Value = (object)col };
                        })
                        .ToDictionary(x => x.Key, x => x.Value);
                })
                .Where(x => x.Keys.Count > 0)
                .ToArray();
            }
            catch (Exception ex)
            {
                Logger.WarnException(string.Format("Failure parse. source:{0}", source), ex);

                return new[]
                {
                    new Dictionary<string, object>
                    {
                        {"messages", "Failure parse."},
                    }
                };
            }
        }
    }
}

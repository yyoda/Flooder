using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.FileSystem.Parser
{
    public class CsvParser : IPayloadParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IDictionary<string, object> Parse(string source)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object>[] MultipleParse(string source)
        {
            try
            {
                var chank = source.Split('\n');
                var head = chank.First().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                return chank.Skip(0).Select(row =>
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
                .ToArray();
            }
            catch (Exception ex)
            {
                Logger.WarnException("Failure parse.", ex);

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

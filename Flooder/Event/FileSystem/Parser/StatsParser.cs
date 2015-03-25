using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.FileSystem.Parser
{
    public class StatsParser : IPayloadParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object> Parse(string source)
        {
            try
            {
                var lines = source.Replace("\r\n", "\n").Split('\n');
                var head  = lines.First().Split(new[] { " [", "] [", "] " }, StringSplitOptions.RemoveEmptyEntries);

                var executePosition = 0;
                var duplicatePosision = 0;

                for (var i = lines.Length - 1; 0 < i; i--)
                {
                    if (lines[i].IndexOf("[重複実行回数]", StringComparison.Ordinal) >= 0)
                    {
                        duplicatePosision = i + 1;
                    }

                    if (lines[i].IndexOf("別実行回数]", StringComparison.Ordinal) >= 0)
                    {
                        executePosition = i + 1;
                        break;
                    }
                }

                var executeCounts = 0;
                for (var i = executePosition; i < lines.Length - 1; i++)
                {
                    if (lines[i].IndexOf("-----", StringComparison.Ordinal) >= 0) break;

                    if (lines[i].IndexOf("回 ", StringComparison.Ordinal) >= 0)
                    {
                        var val = lines[i].Split('回').Select(x => x.Trim()).FirstOrDefault() ?? "0";
                        executeCounts += int.Parse(val);
                    }
                }

                var duplicateCounts = new List<bool>();
                for (var i = duplicatePosision; i < lines.Length - 1; i++)
                {
                    if (lines[i].IndexOf("回 ", StringComparison.Ordinal) >= 0)
                    {
                        duplicateCounts.Add(true);
                    }
                }

                return new Dictionary<string, object>
                {
                    {"datetime", head[0].Replace(",", ".").Replace(" ", "T")},
                    {"status", head[1]},
                    {"category", head[2]},
                    {"path", head[3]},
                    {"execute", executeCounts},
                    {"duplicate", duplicateCounts.Count},
                    {"messages", source}
                };
            }
            catch (Exception ex)
            {
                Logger.WarnException("Failure parse.", ex);

                return new Dictionary<string, object>
                {
                    {"messages", source},
                };
            }
        }

        public Dictionary<string, object>[] MultipleParse(string source)
        {
            throw new NotImplementedException();
        }
    }
}

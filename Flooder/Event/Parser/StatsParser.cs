using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.Parser
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

                if (executePosition > 0)
                {
                    for (var i = executePosition; i < lines.Length - 1; i++)
                    {
                        if (lines[i].IndexOf("-----", StringComparison.Ordinal) >= 0) break;

                        if (lines[i].IndexOf("回 ", StringComparison.Ordinal) >= 0)
                        {
                            var val = lines[i].Split('回').Select(x => x.Trim()).FirstOrDefault() ?? "0";
                            executeCounts += int.Parse(val);
                        }
                    }
                }

                var duplicateCounts = new List<bool>();

                if (duplicatePosision > 0)
                {
                    for (var i = duplicatePosision; i < lines.Length - 1; i++)
                    {
                        if (lines[i].IndexOf("回 ", StringComparison.Ordinal) >= 0)
                        {
                            duplicateCounts.Add(true);
                        }
                    }
                }

                string datetime = "", status = "", category = "", path = "";

                switch (head.Length)
                {
                    case 0:
                        break;
                    case 1:
                        datetime = head[0];
                        break;
                    case 2:
                        datetime = head[0];
                        status   = head[1];
                        break;
                    case 3:
                        datetime = head[0];
                        status   = head[1];
                        category = head[2];
                        break;
                    default:
                        datetime = head[0];
                        status   = head[1];
                        category = head[2];
                        path     = head[3];
                        break;
                }

                return new Dictionary<string, object>
                {
                    {"datetime", datetime.Replace(",", ".").Replace(" ", "T")},
                    {"status", status ?? ""},
                    {"category", category ?? ""},
                    {"path", path ?? ""},
                    {"execute", executeCounts},
                    {"duplicate", duplicateCounts.Count},
                    {"messages", source}
                };
            }
            catch (Exception ex)
            {
                Logger.WarnException(string.Format("Failure parse. source:{0}", source), ex);

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

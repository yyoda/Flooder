using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.Parser
{
    public class ApplogParser : IMultipleDictionaryParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object>[] Parse(string source)
        {
            try
            {
                var lines = source.Replace("\r\n", "\n").Split('\n');
                var head  = lines.First().Split(new[] { " [", "] [", "] " }, StringSplitOptions.RemoveEmptyEntries);

                string datetime = "", uid = "", status = "", method = "", title = "";

                if (head.Length >= 5)
                {
                    datetime = head[0];
                    uid      = head[1];
                    status   = head[2];
                    method   = head[3];
                    title    = head[4];
                }
                else if (head.Length < 5)
                {
                    switch (head.Length)
                    {
                        case 4:
                            datetime = head[0];
                            uid      = head[1];
                            status   = head[2];
                            title    = head[3];
                            break;
                        case 3:
                            datetime = head[0];
                            uid      = head[1];
                            status   = head[2];
                            break;
                        case 2:
                            datetime = head[0];
                            uid      = head[1];
                            break;
                        case 1:
                            datetime = head[0];
                            break;
                    }
                }

                return new[]
                {
                    new Dictionary<string, object>
                    {
                        {"datetime", datetime.Replace(",", ".").Replace(" ", "T")},
                        {"uid", uid ?? ""},
                        {"status", status ?? ""},
                        {"method", method ?? ""},
                        {"title", title ?? ""},
                        {
                            "url",
                            lines.Where(x => x.StartsWith(" Url: "))
                                .Select(x => x.Replace(" Url: ", ""))
                                .FirstOrDefault() ?? ""
                        },
                        {
                            "user",
                            lines.Where(x => x.StartsWith(" User: "))
                                .Select(x => x.Replace(" User: ", ""))
                                .FirstOrDefault() ?? ""
                        },
                        {
                            "parameters",
                            lines.Where(x => x.StartsWith(" Parameters: "))
                                .Select(x => x.Replace(" Parameters: ", ""))
                                .FirstOrDefault() ?? ""
                        },
                        {
                            "useragent",
                            lines.Where(x => x.StartsWith(" UserAgent : "))
                                .Select(x => x.Replace(" UserAgent : ", ""))
                                .FirstOrDefault() ?? ""
                        },
                        {"messages", source},
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.WarnException(string.Format("Failure parse. source:{0}", source), ex);

                return new[]
                {
                    new Dictionary<string, object>
                    {
                        {"messages", source},
                    }
                };
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.Parser
{
    public class ApiParser : IPayloadParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Dictionary<string, object> Parse(string source)
        {
            try
            {
                var lines = source.Replace("\r\n", "\n").Split('\n');
                var head  = lines.First().Split(new[] { " [", "] [", "] " }, StringSplitOptions.RemoveEmptyEntries);

                return new Dictionary<string, object>
                {
                    {"datetime",      head[0].Replace(",", ".").Replace(" ", "T")},
                    {"type1",         head[1] ?? ""},
                    {"type2",         head[2] ?? ""},
                    {"url",           head[3] ?? ""},
                    {"request_model", lines[1].Split('=').LastOrDefault() ?? ""},
                    {"method",        lines[2].Split('=').LastOrDefault() ?? ""},
                    {"content_type",  lines[3].Split('=').LastOrDefault() ?? ""},
                    {"body",          lines[4].Split('=').LastOrDefault() ?? ""},
                    {"status_code",   lines[5].Split('=').LastOrDefault() ?? ""},
                    {"response",      lines[6].Replace("Response=", "")},
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

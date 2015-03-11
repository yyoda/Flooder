using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Flooder.Event.FileSystem.Payloads
{
    public class ApplogPayload : IPayload
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IDictionary<string, object> Parse(string hostName, string source)
        {
            try
            {
                var lines = source.Split(new[] { @"\r\n", @"\n" }, StringSplitOptions.RemoveEmptyEntries);
                var head = lines.First().Split(new[] { " [", "] [", "] " }, StringSplitOptions.RemoveEmptyEntries);

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

                return new Dictionary<string, object>
                {
                    {"datetime",   datetime},
                    {"uid",        uid},
                    {"status",     status},
                    {"method",     method},
                    {"title",      title},
                    {"url",        lines.Where(x => x.StartsWith(" Url: ")).Select(x => x.Replace(" Url: ", "")).FirstOrDefault()},
                    {"user",       lines.Where(x => x.StartsWith(" User: ")).Select(x => x.Replace(" User: ", "")).FirstOrDefault()},
                    {"parameters", lines.Where(x => x.StartsWith(" Parameters: ")).Select(x => x.Replace(" Parameters: ", "")).FirstOrDefault()},
                    {"useragent",  lines.Where(x => x.StartsWith(" UserAgent : ")).Select(x => x.Replace(" UserAgent : ", "")).FirstOrDefault()},
                    {"hostname",   hostName},
                    {"messages",   source},
                };
            }
            catch (Exception ex)
            {
                Logger.WarnException("CustomApplogPayload failure parse.", ex);

                return new Dictionary<string, object>
                {
                    {"hostname", hostName},
                    {"messages", source},
                };
            }
        }
    }
}

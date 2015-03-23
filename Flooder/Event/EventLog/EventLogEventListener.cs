using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace Flooder.Event.EventLog
{
    internal class EventLogEventListener : EventListenerBase, IObserver<EntryWrittenEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public EventLogEventListener(string tag, IMessageBroker messageBroker) : base(tag, messageBroker)
        {
            IncludeInfo = IncludeWarn = IncludeError = ExcludeInfo = ExcludeWarn = ExcludeError = new HashSet<Tuple<string, string>>();
        }

        public HashSet<Tuple<string, string>> IncludeInfo { get; set; }
        public HashSet<Tuple<string, string>> IncludeWarn { get; set; }
        public HashSet<Tuple<string, string>> IncludeError { get; set; }
        public HashSet<Tuple<string, string>> ExcludeInfo { get; set; }
        public HashSet<Tuple<string, string>> ExcludeWarn { get; set; }
        public HashSet<Tuple<string, string>> ExcludeError { get; set; }

        public void OnNext(EntryWrittenEventArgs e)
        {
            var checkValue = Tuple.Create(e.Entry.Source, e.Entry.InstanceId.ToString());

            switch (e.Entry.EntryType)
            {
                case EventLogEntryType.Information:
                    if (IncludeInfo.Any() && !IncludeInfo.Contains(checkValue)) return;
                    if (ExcludeInfo.Contains(checkValue)) return;
                    break;
                case EventLogEntryType.Warning:
                    if (IncludeWarn.Any() && !IncludeWarn.Contains(checkValue)) return;
                    if (ExcludeWarn.Contains(checkValue)) return;
                    return;
                case EventLogEntryType.Error:
                    if (IncludeError.Any() && !IncludeError.Contains(checkValue)) return;
                    if (ExcludeError.Contains(checkValue)) return;
                    break;
                default:
                    break;
            }

            try
            {
                var payload = new Dictionary<string, object>
                {
                    {"Category", e.Entry.Category},
                    {"CategoryNumber", e.Entry.CategoryNumber},
                    {"Data", e.Entry.Data},
                    {"EntryType", e.Entry.EntryType},
                    //{"EventID", e.Entry.EventID},
                    {"EventID", e.Entry.InstanceId},
                    {"Index", e.Entry.Index},
                    {"InstanceId", e.Entry.InstanceId},
                    {"MachineName", e.Entry.MachineName},
                    {"Message", e.Entry.Message},
                    {"ReplacementStrings", e.Entry.ReplacementStrings},
                    //{"Site", e.Entry.Site.Name},
                    {"Source", e.Entry.Source},
                    {"TimeGenerated", e.Entry.TimeGenerated},
                    {"TimeWritten", e.Entry.TimeWritten},
                    {"UserName", e.Entry.UserName},
                };

                base.Publish(payload);
            }
            catch (Exception ex)
            {
                Logger.WarnException("Skip because an error has occurred.", ex);
            }
        }

        public void OnError(Exception error)
        {
            Logger.FatalException("EventLogListener#OnError", error);
        }

        public void OnCompleted()
        {
            Logger.Fatal("EventLogListener#OnCompleted");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.EventLog
{
    internal class EventLogListener : IObserver<EntryWrittenEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _tag;
        private readonly IEmitter _emitter;

        public EventLogListener(string tag, IEmitter emitter)
        {
            _tag            = tag;
            _emitter        = emitter;
            TrapInfomations = TrapWarnings = SkipErrors = new HashSet<Tuple<string, string>>();
        }

        public HashSet<Tuple<string, string>> SkipErrors { get; set; }
        public HashSet<Tuple<string, string>> TrapWarnings { get; set; }
        public HashSet<Tuple<string, string>> TrapInfomations { get; set; }

        public void OnNext(EntryWrittenEventArgs e)
        {
            var checkValue = Tuple.Create(e.Entry.Source, e.Entry.InstanceId.ToString());

            switch (e.Entry.EntryType)
            {
                case EventLogEntryType.Error:
                    if (SkipErrors.Contains(checkValue)) return;
                    break;
                case EventLogEntryType.FailureAudit:
                    return;
                case EventLogEntryType.Information:
                    if (TrapInfomations.Contains(checkValue)) break;
                    return;
                case EventLogEntryType.SuccessAudit:
                    return;
                case EventLogEntryType.Warning:
                    if (TrapWarnings.Contains(checkValue)) break;
                    return;
                default:
                    return;
            }

            var tag = _tag + ".log";

            var payload = new Dictionary<string, object>
            {
                {"Category", e.Entry.Category},
                {"CategoryNumber", e.Entry.CategoryNumber},
                {"Container", e.Entry.Container},
                {"Data", e.Entry.Data},
                {"EntryType", e.Entry.EntryType},
                //{"EventID", e.Entry.EventID},
                {"EventID", e.Entry.InstanceId},
                {"Index", e.Entry.Index},
                {"InstanceId", e.Entry.InstanceId},
                {"MachineName", e.Entry.MachineName},
                {"Message", e.Entry.Message},
                {"ReplacementStrings", e.Entry.ReplacementStrings},
                {"Site", e.Entry.Site},
                {"Source", e.Entry.Source},
                {"TimeGenerated", e.Entry.TimeGenerated},
                {"TimeWritten", e.Entry.TimeWritten},
                {"UserName", e.Entry.UserName},
            };

            Task.Factory.StartNew(() => _emitter.Emit(tag, payload));
        }

        public void OnError(Exception error)
        {
            Logger.ErrorException("EventLogListener", error);
        }

        public void OnCompleted()
        {
            Logger.Debug("EventLogListener#OnCompleted");
        }
    }
}

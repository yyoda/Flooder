using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flooder.Core;
using Flooder.Core.Logging;
using Flooder.Core.Transfer;
using NLog;

namespace Flooder.EventLog
{
    internal class EventLogListener : IObserver<EntryWrittenEventArgs>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _appName;
        private readonly IEmitter _emitter;

        public EventLogListener(string appName, IEmitter emitter)
        {
            _appName = appName;
            _emitter = emitter;
        }

        public void OnNext(EntryWrittenEventArgs e)
        {
            var checkValue = e.Entry.Source + " " + e.Entry.InstanceId;

            switch (e.Entry.EntryType)
            {
                case EventLogEntryType.Error:
                    if (_skipErrors.Contains(checkValue)) return;
                    break;
                case EventLogEntryType.FailureAudit:
                    return;
                case EventLogEntryType.Information:
                    if (_trapInfomations.Contains(checkValue)) break;
                    return;
                case EventLogEntryType.SuccessAudit:
                    return;
                case EventLogEntryType.Warning:
                    if (_trapWarnings.Contains(checkValue)) break;
                    return;
                default:
                    return;
            }

            var tag = _appName + ".event.log";

            var payload = new Dictionary<string, object>
            {
                {"Category", e.Entry.Category},
                {"CategoryNumber", e.Entry.CategoryNumber},
                {"Container", e.Entry.Container},
                {"Data", e.Entry.Data},
                {"EntryType", e.Entry.EntryType},
                {"EventID", e.Entry.EventID},
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

        private readonly string[] _skipErrors =
        {
            "OpsMgr Connector 21006",
            "VDS Basic Provider 1",
            "VSS 8193",
            "VSS 7001",
            "volsnap 27",
            "Iphlpsvc 4202",
            "Schannel 36882",
            "Schannel 36888",
            "Schannel 36887",
            "Schannel 36874",
            "Defrag 257",
            "Microsoft-Windows-Defrag 257",
            "Perflib 1008",
            "UmrdpService 1111",
            "TermDD 56",
            "TermDD 50",
            "TermService 1061",
            "DCOM 10016",
            "ESENT 489",
            "MSSQLSERVER 14420",
            "MSSQLSERVER 14421",
            "MSSQLSERVER 17806",
            "MSSQLSERVER 18204",
            "SNMP 1500",
            "DCOM 10006",
            "DCOM 10009",
            "DCOM 10010",
            "Microsoft-Windows-CAPI2 4107"
        };

        private readonly string[] _trapWarnings =
        {
            "ixgbn 27",
            "e1rexpress 27"
        };

        private readonly string[] _trapInfomations =
        {
            "Server Administrator 2095"
        };
    }
}

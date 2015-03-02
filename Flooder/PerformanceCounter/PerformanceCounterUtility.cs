using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Flooder.Core.Logging;
using NLog;

namespace Flooder.PerformanceCounter
{
    //internal class PerformanceCounterUtility
    //{
    //    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    //    public static Setting[] Settings =
    //    {
    //        new Setting("ASP.NET", "Request Error Events Raised"),
    //        new Setting("ASP.NET", "Request Execution Time"),
    //        new Setting("ASP.NET", "Request Wait Time"),
    //        new Setting("ASP.NET", "Requests Disconnected"),
    //        new Setting("ASP.NET", "Requests Queued"),
    //        new Setting("ASP.NET", "Requests Rejected"),
    //        new Setting("HTTP Service Request Queues", "CacheHitRate"),
    //        new Setting("IPv4", "Datagrams Received/sec"),
    //        new Setting("IPv4", "Datagrams Sent/sec"),
    //        new Setting("Memory", "% Committed Bytes In Use"),
    //        new Setting("Memory", "Available MBytes"),
    //        new Setting("Memory", "Committed Bytes"),
    //        new Setting("Memory", "Pages/sec"),
    //        new Setting("Memory", "Pool Nonpaged Bytes"),
    //        new Setting("PhysicalDisk", "% Disk Time"),
    //        new Setting("PhysicalDisk", "Avg. Disk sec/Read"),
    //        new Setting("PhysicalDisk", "Avg. Disk sec/Transfer"),
    //        new Setting("PhysicalDisk", "Avg. Disk sec/Write"),
    //        new Setting("PhysicalDisk", "Current Disk Queue Length"),
    //        new Setting("Process", "% Privileged Time"),
    //        new Setting("Process", "% Processor Time"),
    //        new Setting("Process", "% User Time"),
    //        new Setting("Processor", "% Processor Time", "_Total"),
    //        new Setting(".NET CLR Exceptions", "# of Exceps Thrown"),
    //        new Setting(".NET CLR Exceptions", "# of Exceps Thrown / sec"),
    //        new Setting(".NET CLR Exceptions", "# of Filters / sec"),
    //        new Setting(".NET CLR Exceptions", "# of Finallys / sec"),
    //        new Setting(".NET CLR Exceptions", "Throw To Catch Depth / sec"),
    //        new Setting(".NET CLR LocksAndThreads", "# of current physical Threads"),
    //        new Setting(".NET CLR LocksAndThreads", "Contention Rate / sec"),
    //        new Setting(".NET CLR LocksAndThreads", "Current Queue Length"),
    //        new Setting(".NET CLR LocksAndThreads", "Queue Length / sec"),
    //        new Setting(".NET CLR LocksAndThreads", "Queue Length Peak"),
    //        new Setting(".NET CLR LocksAndThreads", "Total # of Contentions"),
    //        new Setting(".NET CLR Memory", "# Bytes in all Heaps"),
    //        new Setting(".NET CLR Memory", "# GC Handles"),
    //        new Setting(".NET CLR Memory", "# Gen 0 Collections"),
    //        new Setting(".NET CLR Memory", "# Gen 1 Collections"),
    //        new Setting(".NET CLR Memory", "# Gen 2 Collections"),
    //        new Setting(".NET CLR Memory", "# Induced GC"),
    //        new Setting(".NET CLR Memory", "# of Pinned Objects"),
    //        new Setting(".NET CLR Memory", "# of Sink Blocks in use"),
    //        new Setting(".NET CLR Memory", "# Total committed Bytes"),
    //        new Setting(".NET CLR Memory", "# Total reserved Bytes"),
    //        new Setting(".NET CLR Memory", "% Time in GC"),
    //        new Setting(".NET CLR Memory", "Allocated Bytes/sec"),
    //        new Setting(".NET CLR Memory", "Finalization Survivors"),
    //        new Setting(".NET CLR Memory", "Gen 0 heap size"),
    //        new Setting(".NET CLR Memory", "Gen 0 Promoted Bytes/Sec"),
    //        new Setting(".NET CLR Memory", "Gen 1 heap size"),
    //        new Setting(".NET CLR Memory", "Gen 1 Promoted Bytes/Sec"),
    //        new Setting(".NET CLR Memory", "Gen 2 heap size"),
    //        new Setting(".NET CLR Memory", "Large Object Heap size"),
    //        new Setting(".NET CLR Memory", "Process ID"),
    //        new Setting(".NET CLR Memory", "Promoted Finalization-Memory from Gen 0"),
    //        new Setting(".NET CLR Memory", "Promoted Memory from Gen 0"),
    //        new Setting(".NET CLR Memory", "Promoted Memory from Gen 1"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfActiveConnectionPools"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfActiveConnections"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfReclaimedConnections"),
    //        new Setting("ASP.NET Applications", "Cache API Hit Ratio"),
    //        new Setting("ASP.NET Applications", "Cache Total Hit Ratio"),
    //        new Setting("ASP.NET Applications", "Errors Total/Sec"),
    //        new Setting("ASP.NET Applications", "Output Cache Hit Ratio"),
    //        new Setting("ASP.NET Applications", "Pipeline Instance Count"),
    //        new Setting("ASP.NET Applications", "Requests Executing"),
    //        new Setting("ASP.NET Applications", "Requests In Application Queue"),
    //        new Setting("ASP.NET Applications", "Requests Timed Out"),
    //        new Setting("ASP.NET Applications", "Requests/Sec"),
    //        new Setting("Memory", "Cache Faults/sec"),
    //        new Setting("Process", "Handle Count"),
    //        new Setting("Process", "IO Read Bytes/sec"),
    //        new Setting("Process", "IO Write Bytes/sec"),
    //        new Setting("Process", "Private Bytes"),
    //        new Setting("Process", "Thread Count"),
    //        new Setting("Process", "Virtual Bytes"),
    //        new Setting("Process", "Working Set"),
    //        new Setting("Process", "Page Faults/sec", "_Total"),
    //        new Setting("Processor", "% Interrupt Time", "_Total"),
    //        new Setting("Processor", "% Privileged Time", "_Total"),
    //        new Setting("ASP.NET", "Requests Current"),
    //        new Setting("Network Interface", "Bytes Received/sec"),
    //        new Setting("Network Interface", "Bytes Sent/sec"),
    //        new Setting("Network Interface", "Packets Outbound Discarded"),
    //        new Setting("Network Interface", "Packets Received Discarded"),
    //        new Setting("System", "Context Switches/sec"),
    //        new Setting("System", "Processor Queue Length"),
    //        new Setting("Server", "Pool Nonpaged Failures"),
    //        new Setting("W3SVC_W3WP", "Active Requests"),
    //        new Setting("W3SVC_W3WP", "Active Threads Count"),
    //        new Setting("W3SVC_W3WP", "Requests / Sec"),
    //        new Setting(".NET Data Provider for SqlServer", "HardConnectsPerSecond"),
    //        new Setting(".NET Data Provider for SqlServer", "HardDisconnectsPerSecond"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfActiveConnectionPoolGroups"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfFreeConnections"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfInactiveConnectionPoolGroups"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfInactiveConnectionPools"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfNonPooledConnections"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfPooledConnections"),
    //        new Setting(".NET Data Provider for SqlServer", "NumberOfStasisConnections"),
    //        new Setting(".NET Data Provider for SqlServer", "SoftConnectsPerSecond"),
    //        new Setting(".NET Data Provider for SqlServer", "SoftDisconnectsPerSecond"),
    //    };

    //    public static Dictionary<string, object> GetPayload()
    //    {
    //        return Settings.SelectMany(setting =>
    //        {
    //            return new PerformanceCounterCategory(setting.CategoryName)
    //                .GetInstanceNames()
    //                .Where(instanceName =>
    //                {
    //                    if (string.IsNullOrEmpty(setting.InstanceName))
    //                    {
    //                        return true;
    //                    }

    //                    return setting.InstanceName == instanceName;
    //                })
    //                .Select(instanceName =>
    //                {
    //                    var perf = new System.Diagnostics.PerformanceCounter(setting.CategoryName, setting.CounterName, instanceName);

    //                    var path = string.IsNullOrEmpty(perf.InstanceName)
    //                        ? string.Format("{0}\\{1}", perf.CategoryName, perf.CounterName)
    //                        : string.Format("{0}({1})\\{2}", perf.CategoryName, perf.InstanceName, perf.CounterName);

    //                    try
    //                    {
    //                        return new { Path = path, CookedValue = perf.NextValue() };
    //                    }
    //                    catch (Exception e)
    //                    {
    //                        Logger.DebugException(string.Format("skip because an error has occurred. path:{0}", path), e);
    //                        return null;
    //                    }
    //                })
    //                .Where(x => x != null);
    //        })
    //        .ToDictionary(x => x.Path, x => (object)x.CookedValue);
    //    }

    //    public class Setting
    //    {
    //        public string CategoryName { get; private set; }
    //        public string CounterName { get; private set; }
    //        public string InstanceName { get; private set; }

    //        public Setting(string categoryName, string counterName, string instanceName = null)
    //        {
    //            CategoryName = categoryName;
    //            CounterName = counterName;
    //            InstanceName = instanceName;
    //        }
    //    }
    //}
}

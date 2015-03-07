using System.Configuration;
using System.Linq;
using Flooder.Model;
using Flooder.Model.Input;
using Flooder.Model.Output;

namespace Flooder.Core.Configuration
{
    public class Section : ConfigurationSection, IFlooderServiceCreator
    {
        [ConfigurationProperty("in", IsRequired = true)]
        public InputElement In { get { return (InputElement)base["in"]; } }

        [ConfigurationProperty("out", IsRequired = true)]
        public OutputElement Out { get { return (OutputElement)base["out"]; } }

        public FlooderService CreateService()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;

            //in
            var fs = new FileSystemLogs(section.In.FileSystems
                .Select(x => new FileSystemLogs.FileSystemLog(x.Tag, x.Path, x.File, x.Format)));

            var iis = new IISLogs(section.In.IIS
                .Select(x => new IISLogs.IISLog(x.Tag, x.Path, x.Interval)));

            var ev = new EventLogs(section.In.EventLogs.Tag, section.In.EventLogs.Scopes, section.In.EventLogs
                .Select(x => new EventLogs.EventLog(x.Type, x.Mode, x.Source, x.Id)));

            var pc = new PerformanceCounterLogs(section.In.PerformanceCounters.Tag, section.In.PerformanceCounters.Interval, section.In.PerformanceCounters
                .Select(x => new PerformanceCounterLogs.PerformanceCounterLog(x.CategoryName, x.CounterName, x.InstanceName)));

            //out
            var wk = new Workers(section.Out.Workers.Type, section.Out.Workers
                .Select(x => new Workers.Worker(x.Host, x.Port)));

            //model
            var model = new FlooderModel(new InputModel(fs, iis, ev, pc), new OutputModel(wk));

            return new FlooderService(model);
        }
    }
}

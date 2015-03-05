using System.Configuration;
using System.Linq;
using Flooder.Core.Settings;
using Flooder.Core.Settings.In;
using Flooder.Core.Settings.Out;

namespace Flooder.Core.Configuration
{
    public class Section : ConfigurationSection, ISettingsCreator
    {
        [ConfigurationProperty("in", IsRequired = true)]
        public InputElement In { get { return (InputElement)base["in"]; } }

        [ConfigurationProperty("out", IsRequired = true)]
        public OutputElement Out { get { return (OutputElement)base["out"]; } }

        public Settings.Settings Create()
        {
            var section = ConfigurationManager.GetSection("flooder") as Section;

            //in
            var fs = new FileSystemSettings(section.In.FileSystems
                .Select(x => new FileSystemSettings.FileSystemSettingsDetail(x.Tag, x.Path, x.File, x.Format)));

            var iis = new IISSettings(section.In.IIS
                .Select(x => new IISSettings.IISSettingsDetail(x.Tag, x.Path, x.Interval)));

            var ev = new EventLogSettings(section.In.EventLogs.Tag, section.In.EventLogs.Scopes, section.In.EventLogs
                .Select(x => new EventLogSettings.EventLogSettingsDetail(x.Type, x.Mode, x.Source, x.Id)));

            var pc = new PerformanceCounterSettings(section.In.PerformanceCounters.Tag, section.In.PerformanceCounters.Interval, section.In.PerformanceCounters
                .Select(x => new PerformanceCounterSettings.PerformanceCounterSettingsDetail(x.CategoryName, x.CounterName, x.InstanceName)));

            //out
            var wk = new WorkerSettings(section.Out.Workers.Type, section.Out.Workers
                .Select(x => new WorkerSettings.WorkerSettingsDetail(x.Host, x.Port)));

            return new Settings.Settings(new InputSettings(fs, iis, ev, pc), new OutputSettings(wk));
        }
    }
}

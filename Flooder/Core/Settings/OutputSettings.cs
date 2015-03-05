using Flooder.Core.Settings.Out;

namespace Flooder.Core.Settings
{
    public class OutputSettings
    {
        public OutputSettings(WorkerSettings worker)
        {
            Worker = worker;
        }

        public WorkerSettings Worker { get; private set; }
    }
}

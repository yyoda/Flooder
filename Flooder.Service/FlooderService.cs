using System.ServiceProcess;

namespace Flooder.Service
{
    public partial class FlooderService : ServiceBase
    {
        private readonly Flooder.FlooderService _service;

        public FlooderService()
        {
            InitializeComponent();
            _service = FlooderFactory.Create<Flooder.Core.Configuration.Section>();
        }

        protected override void OnStart(string[] args)
        {
            _service.Start();
        }

        protected override void OnStop()
        {
            _service.Stop();
        }
    }
}

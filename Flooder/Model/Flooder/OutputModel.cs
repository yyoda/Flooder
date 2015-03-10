using Flooder.Model.Flooder.Output;

namespace Flooder.Model.Flooder
{
    public class OutputModel
    {
        public OutputModel(Workers workers)
        {
            Workers = workers;
        }

        public Workers Workers { get; private set; }
    }
}

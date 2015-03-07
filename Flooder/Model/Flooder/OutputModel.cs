using Flooder.Model.Output;

namespace Flooder.Model
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

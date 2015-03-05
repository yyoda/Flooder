
namespace Flooder.Core.Settings
{
    public class Settings
    {
        public Settings(InputSettings @in, OutputSettings @out)
        {
            In  = @in;
            Out = @out;
        }

        public InputSettings In { get; private set; }
        public OutputSettings Out { get; private set; }
    }
}

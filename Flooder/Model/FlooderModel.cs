
namespace Flooder.Model
{
    public class FlooderModel
    {
        public FlooderModel(InputModel input, OutputModel output)
        {
            Input  = input;
            Output = output;
        }

        public InputModel Input { get; private set; }
        public OutputModel Output { get; private set; }
    }
}

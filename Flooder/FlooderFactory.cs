
namespace Flooder
{
    public class FlooderFactory
    {
        public static IFlooderService Create<T>() where T : IFlooderService, new()
        {
            return new T().Create();
        }
    }
}

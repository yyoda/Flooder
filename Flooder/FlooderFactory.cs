
namespace Flooder
{
    public interface IFlooderServiceCreator
    {
        FlooderService CreateService();
    }

    public class FlooderFactory
    {
        public static FlooderService Create<T>() where T : IFlooderServiceCreator, new()
        {
            return new T().CreateService();
        }
    }
}


namespace Flooder
{
    public interface IFlooderService
    {
        IFlooderService Create();
        void Start();
        void Stop();
        bool IsRunning { get; }
    }
}

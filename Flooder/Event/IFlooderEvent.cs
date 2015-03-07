using System;

namespace Flooder.Event
{
    public interface IFlooderEvent
    {
        IDisposable[] Subscribe();
    }
}

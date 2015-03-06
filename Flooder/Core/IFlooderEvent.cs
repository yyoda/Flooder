using System;

namespace Flooder.Core
{
    public interface IFlooderEvent
    {
        IDisposable[] Subscribe();
    }
}

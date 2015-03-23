using System;

namespace Flooder.RetryPolicy
{
    public interface IRetryPolicy
    {
        TimeSpan CurrentInterval { get; }
        bool TryGetNext(out TimeSpan retryInterval);
        void Reset();
        void Reset(out TimeSpan retryInterval);
    }
}

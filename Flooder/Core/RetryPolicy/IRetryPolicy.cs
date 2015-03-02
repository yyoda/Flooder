using System;

namespace Flooder.Core.RetryPolicy
{
    public interface IRetryPolicy
    {
        bool TryGetNext(out TimeSpan retryInterval);
        void Reset();
    }
}

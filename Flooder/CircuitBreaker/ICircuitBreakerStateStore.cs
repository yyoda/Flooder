using System;

namespace Flooder.CircuitBreaker
{
    public interface ICircuitBreakerStateStore
    {
        CircuitBreakerState State { get; }

        Exception LastException { get; }

        DateTimeOffset LastStateChangedDate { get; }

        void Trip(Exception ex);

        void Reset();

        void HalfOpen();

        bool IsClosed { get; }
    }
}

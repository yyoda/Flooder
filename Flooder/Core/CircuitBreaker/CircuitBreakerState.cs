﻿
namespace Flooder.Core.CircuitBreaker
{
    public enum CircuitBreakerState
    {
        Open = 1,
        HalfOpen = 2,
        Close = 3,
    }
}

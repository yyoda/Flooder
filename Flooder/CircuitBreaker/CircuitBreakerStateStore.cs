using System;
using System.Collections.Generic;

namespace Flooder.CircuitBreaker
{
    public class CircuitBreakerStateStore : ICircuitBreakerStateStore
    {
        private IList<Exception> _exceptions;
        private Exception _exception;
        private DateTimeOffset _now;

        public CircuitBreakerStateStore()
        {
            _exceptions = new List<Exception>();
            _exception  = null;
            _now        = DateTimeOffset.Now;
            State       = CircuitBreakerState.Close;
        }

        public CircuitBreakerState State { get; private set; }

        public Exception LastException { get { return _exception; } }

        public DateTimeOffset LastStateChangedDate { get { return _now; } }

        public void Trip(Exception ex)
        {
            _exceptions.Add(ex);
            _exception = ex;
            _now       = DateTimeOffset.Now;
            State      = CircuitBreakerState.Open;
        }

        public void Reset()
        {
            _now  = DateTimeOffset.Now;
            State = CircuitBreakerState.Close;
        }

        public void HalfOpen()
        {
            _now  = DateTimeOffset.Now;
            State = CircuitBreakerState.HalfOpen;
        }

        public bool IsClosed { get { return State == CircuitBreakerState.Close; } }
    }
}

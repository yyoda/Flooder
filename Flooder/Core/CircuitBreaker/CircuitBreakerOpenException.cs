using System;

namespace Flooder.Model.CircuitBreaker
{
    public class CircuitBreakerOpenException : Exception
    {
        const string DefaultMessage = "CircuitBreaker failed open.";

        public CircuitBreakerOpenException()
            : base(DefaultMessage)
        {
        }

        public CircuitBreakerOpenException(string message)
            : base(DefaultMessage + " (" + message + ")")
        {
        }

        public CircuitBreakerOpenException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        public CircuitBreakerOpenException(string message, Exception innerException)
            : base(DefaultMessage + " (" + message + ")", innerException)
        {
        }
    }
}

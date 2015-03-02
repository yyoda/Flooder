using System.Linq;

namespace Flooder.Core.CircuitBreaker
{
    internal class CircuitBreakerStateStoreFactory
    {
        private static readonly ICircuitBreakerStateStore[] Instance = { new CircuitBreakerStateStore() };

        public static ICircuitBreakerStateStore GetInstance<T>() where T : ICircuitBreakerStateStore
        {
            return Instance.OfType<T>().FirstOrDefault();
        }
    }
}

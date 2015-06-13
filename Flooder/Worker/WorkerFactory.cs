using System;
using System.Collections.Generic;
using System.Reflection;
using Flooder.CircuitBreaker;
using Flooder.Events;

namespace Flooder.Worker
{
    public class WorkerFactory
    {
        public static IMessageBroker Create(Type type, MessageBrokerOption option)
        {
            return (IMessageBroker)Activator
                .CreateInstance(type, BindingFlags.CreateInstance, null, new object[] { option }, null);
        }

        public static IMessageBroker Create<T>(MessageBrokerOption option)
        {
            return (IMessageBroker) Activator
                .CreateInstance(typeof(T), BindingFlags.CreateInstance, null, new object[] {option}, null);
        }

        public static IMessageBroker Create(string type, MessageBrokerOption option)
        {
            switch (type)
            {
                case "fluentd":
                    return new FluentMessageBroker(option);
                case "stdout":
                    return new StdOutMessageBroker(option);
                default:
                    throw new NullReferenceException(string.Format("Type[{0}] is not found.", type));
            }
        }
    }

    public class MessageBrokerOption
    {
        public MessageBrokerOption()
        {
        }

        public MessageBrokerOption(IEnumerable<Tuple<string, int>> hosts)
        {
            Hosts          = hosts;
            Interval       = TimeSpan.FromSeconds(1);
            RetryMaxCount  = 3;
            Capacity       = 10000;
            ExtractCount   = 10;
            CircuitBreaker = new IncrementalRetryableCircuitBreaker();
        }

        public IEnumerable<Tuple<string, int>> Hosts { get; private set; }
        public TimeSpan Interval { get; set; }
        public int RetryMaxCount { get; set; }
        public int Capacity { get; set; }
        public int ExtractCount { get; set; }
        public IncrementalRetryableCircuitBreaker CircuitBreaker { get; private set; }
    }
}

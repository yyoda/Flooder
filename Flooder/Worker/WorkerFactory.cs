using System;
using System.Collections.Generic;
using System.Reflection;
using Flooder.Event;

namespace Flooder.Worker
{
    public class WorkerFactory
    {
        public static IMessageBroker Create(Type type, WorkerOption option)
        {
            return (IMessageBroker)Activator
                .CreateInstance(type, BindingFlags.CreateInstance, null, new object[] { option }, null);
        }

        public static IMessageBroker Create<T>(WorkerOption option)
        {
            return (IMessageBroker) Activator
                .CreateInstance(typeof(T), BindingFlags.CreateInstance, null, new object[] {option}, null);
        }

        [Obsolete("Want to delete.")]  //TODO:
        public static IMessageBroker Create(string type, WorkerOption option)
        {
            switch (type)
            {
                case "fluentd":
                    return new FluentMessageBroker(option);
                default:
                    throw new NullReferenceException(string.Format("Type[{0}] is not found.", type));
            }
        }
    }

    public class WorkerOption
    {
        public WorkerOption(IEnumerable<Tuple<string, int>> hosts)
        {
            Hosts         = hosts;
            Interval      = TimeSpan.FromSeconds(1);
            RetryMaxCount = 3;
            Capacity      = 10000;
            Extraction    = 10;
        }

        public IEnumerable<Tuple<string, int>> Hosts { get; private set; }
        public TimeSpan Interval { get; set; }
        public int RetryMaxCount { get; set; }
        public int Capacity { get; set; }
        public int Extraction { get; set; }
    }
}

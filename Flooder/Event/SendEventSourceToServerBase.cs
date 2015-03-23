using System;
using System.Collections.Generic;
using System.Linq;

namespace Flooder.Event
{
    public abstract class SendEventSourceToServerBase
    {
        protected IEventSource EventSource { get; private set; }
        protected IMessageBroker MessageBroker { get; private set; }

        protected SendEventSourceToServerBase(IEventSource eventSource, IMessageBroker messageBroker)
        {
            EventSource   = eventSource;
            MessageBroker = messageBroker;
        }

        public abstract IDisposable[] Subscribe();
    }

    public static class SendEventSourceToServerBaseExtensions
    {
        public static IEnumerable<IDisposable> Subscribe(this IEnumerable<SendEventSourceToServerBase> @this)
        {
            return @this.SelectMany(x => x.Subscribe());
        }
    }
}

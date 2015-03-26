using System;

namespace Flooder.Event
{
    public abstract class EventBase
    {
        protected IDataSource DataSource { get; private set; }
        protected IMessageBroker MessageBroker { get; private set; }

        protected EventBase(IDataSource dataSource, IMessageBroker messageBroker)
        {
            DataSource    = dataSource;
            MessageBroker = messageBroker;
        }

        public abstract IDisposable[] Subscribe();
    }
}

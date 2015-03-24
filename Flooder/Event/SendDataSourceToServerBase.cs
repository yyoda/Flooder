using System;

namespace Flooder.Event
{
    public abstract class SendDataSourceToServerBase
    {
        protected IDataSource DataSource { get; private set; }
        protected IMessageBroker MessageBroker { get; private set; }

        protected SendDataSourceToServerBase(IDataSource dataSource, IMessageBroker messageBroker)
        {
            DataSource    = dataSource;
            MessageBroker = messageBroker;
        }

        public abstract IDisposable[] Subscribe();
    }
}

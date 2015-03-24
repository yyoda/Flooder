using System;
using System.Collections.Generic;
using System.Linq;

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

    public static class SendEventSourceToServerBaseExtensions
    {
        public static IEnumerable<IDisposable> Subscribe(this IEnumerable<SendDataSourceToServerBase> @this)
        {
            return @this.SelectMany(x => x.Subscribe());
        }
    }
}

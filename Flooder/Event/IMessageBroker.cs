using System;
using System.Collections.Generic;

namespace Flooder.Event
{
    public interface IMessageBroker
    {
        void Publish(string tag, IDictionary<string, object> payload);
        void Publish(string tag, IDictionary<string, object>[] payload);
        IDisposable Subscribe();
    }
}

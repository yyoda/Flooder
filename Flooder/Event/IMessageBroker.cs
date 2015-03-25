using System;
using System.Collections.Generic;

namespace Flooder.Event
{
    public interface IMessageBroker
    {
        void Publish(string tag, Dictionary<string, object> payload);
        void Publish(string tag, Dictionary<string, object>[] payload);
        IEnumerable<IDisposable> Subscribe();
    }
}

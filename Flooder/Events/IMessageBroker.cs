using System;
using System.Collections.Generic;

namespace Flooder.Events
{
    public interface IMessageBroker
    {
        void Publish(string tag, Dictionary<string, object> payload);
        void Publish(string tag, Dictionary<string, object>[] payload);
        IEnumerable<IDisposable> Subscribe();
    }
}

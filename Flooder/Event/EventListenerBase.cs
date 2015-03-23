using System.Collections.Generic;
using System.Net;

namespace Flooder.Event
{
    public abstract class EventListenerBase
    {
        private readonly string _tag;
        private readonly string _hostName;
        private readonly IMessageBroker _messageBroker;

        protected EventListenerBase(string tag, IMessageBroker messageBroker)
        {
            _tag           = tag;
            _messageBroker = messageBroker;
            _hostName      = Dns.GetHostName();
        }

        protected string Tag { get { return _tag; } }

        protected void Publish(IDictionary<string, object> payload)
        {
            payload["hostname"] = _hostName;
            _messageBroker.Publish(Tag, payload);
        }

        protected void Publish(string tag, IDictionary<string, object> payload)
        {
            payload["hostname"] = _hostName;
            _messageBroker.Publish(tag, payload);
        }

        protected void Publish(string tag, IDictionary<string, object>[] payloads)
        {
            foreach (var payload in payloads)
            {
                payload["hostname"] = _hostName;
            }

            _messageBroker.Publish(tag, payloads);
        }
    }
}

using Flooder.Transfer;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Flooder.Event
{
    public abstract class EventListenerBase
    {
        private readonly FlooderObject _obj;
        private readonly string _tag;

        protected EventListenerBase(string tag, FlooderObject obj)
        {
            _tag     = tag;
            _obj     = obj;
            HostName = Dns.GetHostName();
        }

        protected string Tag { get { return _tag; } }
        protected IEmitter Emitter { get { return _obj.Worker.Emitter; } }
        protected string HostName { get; private set; }

        protected void Emit(IDictionary<string, object> payload)
        {
            payload["hostname"] = HostName;

            Task.Factory.StartNew(() => Emitter.Emit(Tag, payload));
        }

        protected void Emit(string tag, IDictionary<string, object> payload)
        {
            payload["hostname"] = HostName;

            Task.Factory.StartNew(() => Emitter.Emit(tag, payload));
        }
    }
}

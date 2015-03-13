using System;
using System.Linq;

namespace Flooder.Event
{
    public abstract class SendEventSourceToServerBase
    {
        private readonly FlooderObject _obj;

        protected FlooderObject FlooderObject { get { return _obj; } }

        protected SendEventSourceToServerBase(FlooderObject obj)
        {
            _obj = obj;
        }

        public abstract IDisposable[] Subscribe();

        protected T GetEventSource<T>() where T : IEventSource, new()
        {
            var t = _obj.Events.OfType<T>().FirstOrDefault();

            if (t == null)
            {
                return new T();
            }

            return t;
        }


    }
}

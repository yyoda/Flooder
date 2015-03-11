using System;

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
    }
}

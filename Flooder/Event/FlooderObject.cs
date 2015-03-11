using System.Collections.Generic;
using Flooder.Transfer;

namespace Flooder.Event
{
    public class FlooderObject
    {
        public FlooderObject(IEnumerable<IEventSource> events, Worker worker)
        {
            Events = events;
            Worker = worker;
        }

        public IEnumerable<IEventSource> Events { get; private set; }
        public Worker Worker { get; private set; }
    }
}

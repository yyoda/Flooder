using System.Collections.Generic;

namespace Flooder.Transfer
{
    public interface IEmitter
    {
        void Emit(string tag, IDictionary<string, object> payload);
    }
}

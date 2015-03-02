using System;
using Flooder.Core.Logging;

namespace Flooder.Core.RetryPolicy
{
    public class FixedInterval : IRetryPolicy
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();
        private int _currentRetryCount;
        private int? _maxRetryCount;
        private readonly TimeSpan _interval;

        public FixedInterval(TimeSpan interval, int? retryCount = null)
        {
            _maxRetryCount = retryCount;
            _interval = interval;
        }

        public bool TryGetNext(out TimeSpan retryInterval)
        {
            if (_maxRetryCount.HasValue)
            {
                if (_currentRetryCount >= _maxRetryCount.Value)
                {
                    retryInterval = TimeSpan.Zero;
                    return false;
                }
            }

            if (_currentRetryCount < int.MaxValue)
            {
                _currentRetryCount++;
            }

            retryInterval = _interval;
            return true;
        }

        public void Reset()
        {
            _currentRetryCount = 0;
        }
    }
}

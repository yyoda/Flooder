using System;
using Flooder.Core.Logging;

namespace Flooder.Core.RetryPolicy
{
    public class FixedInterval : IRetryPolicy
    {
        private int _currentRetryCount;
        private int? _maxRetryCount;
        private readonly TimeSpan _fixedInterval;

        public FixedInterval(TimeSpan interval, int? retryCount = null)
        {
            _maxRetryCount = retryCount;
            _fixedInterval = interval;
        }

        public TimeSpan CurrentInterval { get { return _fixedInterval; } }

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

            retryInterval = _fixedInterval;
            return true;
        }

        public void Reset()
        {
            _currentRetryCount = 0;
        }

        public void Reset(out TimeSpan retryInterval)
        {
            _currentRetryCount = 0;
            retryInterval      = _fixedInterval;
        }
    }
}

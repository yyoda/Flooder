using System;
using Flooder.Core.Logging;

namespace Flooder.Core.RetryPolicy
{
    public class Incremental : IRetryPolicy
    {
        private int _currentRetryCount;
        private int? _maxRetryCount;
        private readonly TimeSpan _initial, _incremental;
        private TimeSpan _currentInterval;

        public Incremental(TimeSpan initial, TimeSpan incremental, int? retryCount = null)
        {
            _initial         = initial;
            _incremental     = incremental;
            _maxRetryCount   = retryCount;
            _currentInterval = _initial;
        }

        public TimeSpan CurrentInterval { get { return _currentInterval; } }

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

            retryInterval = _currentInterval = _currentInterval.Add(_incremental);
            return true;
        }

        public void Reset()
        {
            _currentRetryCount = 0;
            _currentInterval = _initial;
        }

        public void Reset(out TimeSpan retryInterval)
        {
            _currentRetryCount = 0;
            retryInterval = _currentInterval = _initial;
        }
    }
}

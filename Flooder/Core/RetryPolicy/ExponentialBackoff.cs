using System;
using Flooder.Core.Logging;

namespace Flooder.Core.RetryPolicy
{
    public class ExponentialBackoff : IRetryPolicy
    {
        private int _currentRetryCount;
        private readonly int _maxRetryCount;
        private TimeSpan _minBackoff, _maxBackoff, _deltaBackoff, _currentInterval;

        public ExponentialBackoff(int retryCount)
            : this(TimeSpan.FromSeconds(1), TimeSpan.FromHours(1), TimeSpan.FromSeconds(1), retryCount)
        {
        }

        public ExponentialBackoff(TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, int maxRetryCount)
        {
            _minBackoff      = minBackoff;
            _maxBackoff      = maxBackoff;
            _deltaBackoff    = deltaBackoff;
            _maxRetryCount   = maxRetryCount;
            _currentInterval = minBackoff;
        }

        public TimeSpan CurrentInterval { get { return _currentInterval; } }

        public bool TryGetNext(out TimeSpan retryInterval)
        {
            if (_currentRetryCount >= _maxRetryCount)
            {
                _currentInterval = retryInterval = TimeSpan.Zero;
                return false;
            }

            var rnd = new Random();
            var delta = (int)((Math.Pow(2.0, _currentRetryCount) - 1.0) * rnd.Next((int)(_deltaBackoff.TotalMilliseconds * 0.8), (int)(_deltaBackoff.TotalMilliseconds * 1.2)));
            var interval = (int)Math.Min(checked(_minBackoff.TotalMilliseconds + delta), _maxBackoff.TotalMilliseconds);

            retryInterval = _currentInterval = TimeSpan.FromMilliseconds(interval);
            _currentRetryCount++;

            return true;
        }

        public void Reset()
        {
            _currentRetryCount = 0;
            _currentInterval   = _minBackoff;
        }

        public void Reset(out TimeSpan retryInterval)
        {
            _currentRetryCount = 0;
            retryInterval      = _currentInterval = _minBackoff;
        }
    }
}

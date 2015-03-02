using System;
using Flooder.Core.Logging;

namespace Flooder.Core.RetryPolicy
{
    public class ExponentialBackoff : IRetryPolicy
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();
        private int _currentRetryCount, _maxRetryCount;
        private readonly TimeSpan _minBackoff, _maxBackoff, _deltaBackoff;

        public ExponentialBackoff(int retryCount)
            : this(TimeSpan.FromSeconds(1), TimeSpan.FromHours(1), TimeSpan.FromSeconds(1), retryCount)
        {
        }

        public ExponentialBackoff(TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, int maxRetryCount)
        {
            _minBackoff = minBackoff;
            _maxBackoff = maxBackoff;
            _deltaBackoff = deltaBackoff;
            _maxRetryCount = maxRetryCount;
        }

        public bool TryGetNext(out TimeSpan retryInterval)
        {
            if (_currentRetryCount >= _maxRetryCount)
            {
                retryInterval = TimeSpan.Zero;
                return false;
            }

            var rnd = new Random();
            var delta = (int)((Math.Pow(2.0, _currentRetryCount) - 1.0) * rnd.Next((int)(_deltaBackoff.TotalMilliseconds * 0.8), (int)(_deltaBackoff.TotalMilliseconds * 1.2)));
            var interval = (int)Math.Min(checked(_minBackoff.TotalMilliseconds + delta), _maxBackoff.TotalMilliseconds);

            retryInterval = TimeSpan.FromMilliseconds(interval);
            _currentRetryCount++;

            return true;
        }

        public void Reset()
        {
            _currentRetryCount = 0;
        }
    }
}

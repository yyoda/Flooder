using System;
using System.Threading;
using Flooder.Core.RetryPolicy;

namespace Flooder.Core.CircuitBreaker
{
    public class InfiniteCircuitBreaker
    {
        private readonly object _halfOpenSyncObject = new object ();
        private readonly IRetryPolicy _retryPolicy = new FixedInterval(TimeSpan.FromSeconds(1));
        private readonly ICircuitBreakerStateStore _stateStore;

        public bool IsClosed { get { return _stateStore.IsClosed; } }
        public bool IsOpen { get { return !IsClosed; } }

        public InfiniteCircuitBreaker(ICircuitBreakerStateStore stateStore)
        {
            _stateStore = stateStore;
        }

        public void ExecuteAction(Action action)
        {
            if (IsOpen)
            {
                TimeSpan interval;
                if (_retryPolicy.TryGetNext(out interval))
                {
                    //オープンな状態がどれだけの期間発生しているかを見て、規定期間が経過したときにハーフオープン判定を行う
                    if (_stateStore.LastStateChangedDate.Add(interval) < DateTimeOffset.Now)
                    {
                        bool lockTaken = false;

                        try
                        {
                            Monitor.TryEnter(_halfOpenSyncObject, ref lockTaken);

                            if (lockTaken)
                            {
                                _stateStore.HalfOpen();

                                action();

                                //成功したらClosedな状態に変更する
                                _stateStore.Reset();

                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            //失敗したらOpenに戻す
                            _stateStore.Trip(ex);
                            throw;
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(_halfOpenSyncObject);
                            }
                        }
                    }
                }

                throw new CircuitBreakerOpenException(_stateStore.LastException);
            }

            try
            {
                action();
                _retryPolicy.Reset();
            }
            catch (Exception ex)
            {
                _stateStore.Trip(ex);
                throw;
            }
        }
    }
}

﻿using System;
using System.Threading;
using Flooder.RetryPolicy;
using NLog;

namespace Flooder.CircuitBreaker
{
    public class IncrementalRetryableCircuitBreaker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _halfOpenSyncObject = new object ();
        private readonly IRetryPolicy _retryPolicy = new Incremental(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
        private readonly ICircuitBreakerStateStore _stateStore;
        private TimeSpan _interval;

        public bool IsClosed { get { return _stateStore.IsClosed; } }
        public bool IsOpen { get { return !IsClosed; } }

        public IncrementalRetryableCircuitBreaker() : this(new CircuitBreakerStateStore())
        {
        }

        public IncrementalRetryableCircuitBreaker(ICircuitBreakerStateStore stateStore)
        {
            _stateStore = stateStore;
            _interval   = _retryPolicy.CurrentInterval;
        }

        public void ExecuteAction(Action action)
        {
            if (IsOpen)
            {
                if (_retryPolicy.TryGetNext(out _interval))
                {
                    bool lockTaken = false;

                    try
                    {
                        Monitor.TryEnter(_halfOpenSyncObject, ref lockTaken);

                        if (lockTaken)
                        {
                            _stateStore.HalfOpen();

                            action();

                            _stateStore.Reset();
                            _retryPolicy.Reset(out _interval);
                            Logger.Warn("[HalfOpen => Success] CircuitBreaker is close.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _stateStore.Trip(ex);
                        Logger.Warn("[HalfOpen => Failure] CircuitBreaker is open.");
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(_halfOpenSyncObject);
                        }
                    }

                    Logger.Warn("CircuitBreaker try the recovery after {0} second.", _interval);
                    Thread.Sleep(_interval);
                    return;
                }

                throw new CircuitBreakerOpenException("CircuitBreaker is abort.", _stateStore.LastException);
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                _stateStore.Trip(ex);
                Logger.Error("CircuitBreaker is open.", ex);
            }

            Thread.Sleep(_interval);
        }
    }
}

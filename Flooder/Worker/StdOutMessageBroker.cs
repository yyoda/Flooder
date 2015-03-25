using Flooder.Event;
using Flooder.Utility;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Web.Script.Serialization;

namespace Flooder.Worker
{
    public class StdOutMessageBroker : IMessageBroker
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        private readonly BlockingCollection<string> _queue;
        private readonly WorkerOption _option;

        public int Count { get { return _queue.Count; } }

        public StdOutMessageBroker(WorkerOption option)
        {
            _queue  = new BlockingCollection<string>(option.Capacity);
            _option = option;
        }

        public void Publish(string tag, Dictionary<string, object> payload)
        {
            this.Publish(tag, new []{ payload });
        }

        public void Publish(string tag, Dictionary<string, object>[] payloads)
        {
            Publish(JsonSerializer.Serialize(new object[]
            {
                tag,
                DateTime.Now.ToUnixTime(),
                payloads
            }));
        }

        public void Publish(string message)
        {
            var retryCount = 0;

            Observable.Create<TimeSpan>(x =>
            {
                if (_queue.TryAdd(message))
                {
                    x.OnCompleted();
                    return Disposable.Empty;
                }

                x.OnError(new InvalidOperationException(string.Format(
                    "Queue overflow. retryCount:{0}, sleepTime:{1}", ++retryCount, _option.Interval)));

                x.OnNext(_option.Interval);
                return Disposable.Empty;
            })
            .Retry(_option.RetryMaxCount)
            .Subscribe(
                Thread.Sleep,
                ex => Logger.ErrorException("StdOutMessageBroker#Publish.", ex),
                () => Logger.Trace("StdOutMessageBroker#Publish success.")
            );
        }

        public IEnumerable<IDisposable> Subscribe()
        {
            var messageBroker = Observable.Start(() =>
            {
                while (true)
                {
                    var takeCount = _option.Extraction; //initialize.

                    if (_queue.Count > 0)
                    {
                        if (_queue.Count < takeCount)
                        {
                            takeCount = _queue.Count;
                        }

                        var messages = _queue.GetConsumingEnumerable()
                            .Take(takeCount)
                            .ToArray();

                        foreach (var message in messages)
                        {
                            Console.WriteLine(message);
                        }

                        continue;
                    }

                    Thread.Sleep(_option.Interval);
                }
            })
            .Subscribe(
                x => { },
                ex => Logger.ErrorException("StdOutMessageBroker#Subscribe", ex),
                () => Logger.Fatal("StdOutMessageBroker#Subscribe stoped.")
            );

            return new[] { messageBroker };
        }
    }
}

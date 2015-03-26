using Flooder.Event;
using Flooder.Utility;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly MessageBrokerOption _option;

        public int Count { get { return _queue.Count; } }

        public StdOutMessageBroker(MessageBrokerOption option)
        {
            _queue  = new BlockingCollection<string>();
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
            if (_queue.TryAdd(message))
            {
                Logger.Error("StdOutMessageBroker#Publish failue. message:{0}", message);
            }
        }

        public IEnumerable<IDisposable> Subscribe()
        {
            var messageBroker = Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(
                x =>
                {
                    string message;
                    if (_queue.TryTake(out message))
                    {
                        Console.WriteLine(message);
                        return;
                    }

                    Thread.Sleep(_option.Interval);
                },
                ex => Logger.ErrorException("StdOutMessageBroker#Subscribe", ex),
                () => Logger.Fatal("StdOutMessageBroker#Subscribe stoped.")
            );

            return new[] { messageBroker };
        }
    }
}

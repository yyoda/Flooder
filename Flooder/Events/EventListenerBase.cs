using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Flooder.Utility;

namespace Flooder.Events
{
    public abstract class EventListenerBase<T> : IObserver<T>
    {
        private readonly string _tag;
        private readonly string _hostName;
        private readonly IMessageBroker _messageBroker;

        protected EventListenerBase(string tag, IMessageBroker messageBroker)
        {
            _tag           = tag;
            _messageBroker = messageBroker;
            _hostName      = Dns.GetHostName();
        }

        public abstract void OnNext(T value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        protected string Tag { get { return _tag; } }

        protected void Publish(Dictionary<string, object> payload)
        {
            Publish(this.Tag, payload);
        }

        protected void Publish(string tag, Dictionary<string, object> payload)
        {
            payload["hostname"] = _hostName;
            _messageBroker.Publish(tag, payload);
        }

        protected void Publish(Dictionary<string, object>[] payloads)
        {
            Publish(this.Tag, payloads);
        }

        protected void Publish(string tag, Dictionary<string, object>[] payloads)
        {
            foreach (var payload in payloads)
            {
                payload["hostname"] = _hostName;
            }

            _messageBroker.Publish(tag, payloads);
        }

        /// <summary>Like search helper.</summary>
        protected string ChangeRegexPattern(string fileName)
        {
            return RegexUtility.ChangeRegexPattern(fileName);
        }

        protected bool IsLike(string source, string keyword)
        {
            return RegexUtility.IsLike(source, keyword);
        }

        protected bool TryGetLatestFile(string filePath, out string fullPath, out DateTime lastWriteTime)
        {
            return FileUtility.TryGetLatestFile(filePath, out fullPath, out lastWriteTime);
        }

        protected bool TryRemoveFile(string filePath, out IEnumerable<Exception> exceptions)
        {
            exceptions = FileUtility.RemoveFile(filePath);
            return !exceptions.Any();
        }
    }
}

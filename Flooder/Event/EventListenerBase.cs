﻿using System.Collections.Generic;
using System.Net;
using Flooder.Utility;

namespace Flooder.Event
{
    public abstract class EventListenerBase
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
    }
}

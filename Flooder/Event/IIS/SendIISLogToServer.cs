﻿using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using NLog;

namespace Flooder.Event.IIS
{
    public class SendIISLogToServer : SendEventSourceToServerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendIISLogToServer(FlooderObject obj) : base(obj)
        {
        }

        public override IDisposable[] Subscribe()
        {
            var source = base.GetEventSource<IISLogEventSource>();

            var enable = source.Details.Where(x =>
            {
                if (Directory.Exists(x.Path)) return true;
                Logger.Debug("[{0}] will be skipped because it does not exist.", x.ToString());
                return false;
            })
            .Any();

            if (!enable) return new IDisposable[0];

            return source.Details.Select(x =>
            {
                var observer = new IISLogListener(x.Tag, x.Path, base.FlooderObject);
                observer.OnInitAction();

                var subscribe = Observable
                    .Interval(TimeSpan.FromSeconds(x.Interval))
                    .Subscribe(observer);

                Logger.Info("IISLogListener start. tag:{0}, path:{1}, interval:{2}", x.Tag, x.Path, x.Interval);

                return subscribe;
            })
            .ToArray();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using Flooder.CircuitBreaker;
using Flooder.Utility;
using NLog;

namespace Flooder.Worker
{
    public class TcpManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly object _syncObject;
        private readonly IEnumerable<Tuple<string, int, int>> _hosts;
        /// <summary>TupleSpec is Item1:Ip, Item2:Port, Item3:Weight</summary>
        private IDictionary<Tuple<string, int, int>, TcpClient> _connectionStateStore;
        private readonly IncrementalRetryableCircuitBreaker _circuitBreaker;

        public TcpManager(IEnumerable<Tuple<string, int, int>> hosts, IncrementalRetryableCircuitBreaker circuitBreaker)
        {
            _syncObject           = new object();
            _hosts                = hosts;
            _connectionStateStore = new Dictionary<Tuple<string, int, int>, TcpClient>();
            _circuitBreaker       = circuitBreaker;
        }

        public bool Connect()
        {
            lock (_syncObject)
            {
                _connectionStateStore = _hosts.Select(host =>
                {
                    var tcp = new TcpClient();

                    try
                    {
                        tcp.Connect(host.Item1, host.Item2);
                    }
                    catch (SocketException)
                    {
                        return new KeyValuePair<Tuple<string, int, int>, TcpClient>(host, null);
                    }

                    return new KeyValuePair<Tuple<string, int, int>, TcpClient>(host, tcp);
                })
                .Where(x => x.Value != null && x.Value.Connected)
                .ToDictionary(x => x.Key, x => x.Value);

                return _connectionStateStore.Count > 0;
            }
        }

        public void Transfer(byte[] bytes)
        {
            lock (_syncObject)
            {
                if (_connectionStateStore.Count <= 0)
                    throw new NullReferenceException("Zero connection.");

                var client = new WeightedSampleSeed<KeyValuePair<Tuple<string, int, int>, TcpClient>>(
                    _connectionStateStore.ToArray(),
                    x => x.Key.Item3    //Item3 is Weight
                )
                .Sample().Value;

                var stream = client.GetStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }

        public void Close()
        {
            lock (_syncObject)
            {
                var tcpClients = _connectionStateStore.ToArray();

                for (var i = 0; i < tcpClients.Length; i++)
                {
                    var tcp = tcpClients[i];

                    if (tcp.Value.Connected)
                    {
                        tcp.Value.GetStream().Close();
                        tcp.Value.Close();
                    }

                    _connectionStateStore.Remove(tcp.Key);
                }
            }
        }

        public IDisposable ConnectionMonitoring()
        {
            Action monitorAction = () =>
            {
                foreach (var host in _hosts)
                {
                    try
                    {
                        var trial = new TcpClient();
                        trial.Connect(host.Item1, host.Item2);

                        lock (_syncObject)
                        {
                            TcpClient pooledClient;
                            if (_connectionStateStore.TryGetValue(host, out pooledClient) && pooledClient.Connected)
                            {
                                trial.Close();
                            }
                            else
                            {
                                _connectionStateStore[host] = trial;
                            }
                        }
                    }
                    catch (SocketException)
                    {
                        lock (_syncObject)
                        {
                            TcpClient pooledClient;
                            if (!_connectionStateStore.TryGetValue(host, out pooledClient))
                            {
                                continue;
                            }

                            pooledClient.GetStream().Close();
                            pooledClient.Close();
                            _connectionStateStore.Remove(host);
                        }
                    }
                }

                if (!_connectionStateStore.Any())
                {
                    throw new Exception("Problem has occurred. to help CircuitBreaker");
                }

                //Logger.Trace("Healthy now...");
            };

            return Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(
                    e => _circuitBreaker.ExecuteAction(monitorAction),
                    ex => Logger.FatalException("Inability to continue.", ex),
                    () => Logger.Fatal("ConnectionMonitoring stoped.")
                );
        }

        public void Dispose()
        {
            Close();
        }
    }
}

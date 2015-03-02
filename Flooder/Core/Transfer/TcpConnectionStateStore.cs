using Flooder.Core.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using Flooder.Core.CircuitBreaker;

namespace Flooder.Core.Transfer
{
    public class TcpConnectionStateStore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _syncObject;
        private readonly Tuple<string, int>[] _hosts;
        private IDictionary<Tuple<string, int>, TcpClient> _connectionPool;
        
        public TcpConnectionStateStore(Tuple<string, int>[] hosts)
        {
            _syncObject     = new object();
            _connectionPool = new Dictionary<Tuple<string, int>, TcpClient>();
            _hosts          = hosts;
        }

        public bool HasConnection { get { return _connectionPool.Count > 0; } }

        public bool Connect()
        {
            lock (_syncObject)
            {
                _connectionPool = _hosts.Select(host =>
                {
                    var tcp = new TcpClient();

                    try
                    {
                        tcp.Connect(host.Item1, host.Item2);
                    }
                    catch (SocketException)
                    {
                        return new KeyValuePair<Tuple<string, int>, TcpClient>(host, null);
                    }

                    return new KeyValuePair<Tuple<string, int>, TcpClient>(host, tcp);
                })
                .Where(x => x.Value != null && x.Value.Connected)
                .ToDictionary(x => x.Key, x => x.Value);

                return _connectionPool.Count > 0;
            }
        }

        public void Transfer(byte[] bytes)
        {
            lock (_syncObject)
            {
                var source = _connectionPool.ShuffleSlim().ToArray();
                if (source.Any())
                {
                    var client = source.First().Value;
                    using (var stream = client.GetStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                        return;
                    }
                }

                throw new NullReferenceException("Zero connection.");
            }
        }

        public void Close()
        {
            lock (_syncObject)
            {
                foreach (var tcp in _connectionPool)
                {
                    if (tcp.Value.Connected)
                    {
                        try
                        {
                            tcp.Value.GetStream().Close();
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorException("Possibly...case A", ex);
                        }

                        tcp.Value.Close();
                    }

                    _connectionPool.Remove(tcp.Key);
                }
            }
        }

        public IDisposable HealthCheck()
        {
            var breaker = new DefaultCircuitBreaker(new CircuitBreakerStateStore());

            return Observable.Start(() =>
            {
                while (true)
                {
                    breaker.ExecuteAction(() =>
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
                                    if (_connectionPool.TryGetValue(host, out pooledClient) && pooledClient.Connected)
                                    {
                                        trial.Close();
                                    }
                                    else
                                    {
                                        _connectionPool[host] = trial;
                                    }
                                }
                            }
                            catch (SocketException)
                            {
                                lock (_syncObject)
                                {
                                    TcpClient pooledClient;
                                    if (!_connectionPool.TryGetValue(host, out pooledClient))
                                    {
                                        continue;
                                    }

                                    try
                                    {
                                        pooledClient.GetStream().Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.ErrorException("Possibly...case B", ex);
                                    }

                                    pooledClient.Close();
                                    _connectionPool.Remove(host);
                                }
                            }
                        }

                        if (!_connectionPool.Any())
                        {
                            throw new Exception("Problem has occurred. to help CircuitBreaker");
                        }

                        Logger.Debug("Healthy now...");
                    });
                }
            })
            .Subscribe(
                e => { /* nothing. */ },
                ex => Logger.ErrorException("Inability to continue.", ex),
                () => Logger.Fatal("HealthCheck stoped.")
            );
        }
    }
}

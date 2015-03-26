using System;
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
        private readonly IEnumerable<Tuple<string, int>> _hosts;
        private IDictionary<Tuple<string, int>, TcpClient> _connectionStateStore;
        private readonly DefaultCircuitBreaker _circuitBreaker;
        
        public TcpManager(IEnumerable<Tuple<string, int>> hosts, DefaultCircuitBreaker circuitBreaker)
        {
            _syncObject           = new object();
            _hosts                = hosts;
            _connectionStateStore = new Dictionary<Tuple<string, int>, TcpClient>();
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
                        return new KeyValuePair<Tuple<string, int>, TcpClient>(host, null);
                    }

                    return new KeyValuePair<Tuple<string, int>, TcpClient>(host, tcp);
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
                var source = _connectionStateStore.ShuffleSlim().ToArray();
                if (source.Any())
                {
                    var client = source.First().Value;
                    var stream = client.GetStream();

                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    return;
                }

                throw new NullReferenceException("Zero connection.");
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
            return Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(
                e =>
                {
                    _circuitBreaker.ExecuteAction(() =>
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
                    });
                },
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

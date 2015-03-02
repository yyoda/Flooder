using Flooder.Core.RetryPolicy;
using Flooder.Core.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading;

namespace Flooder.Core.Transfer
{
    public class TcpCore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _syncObject;
        private readonly Tuple<string, int>[] _hosts;

        private IDictionary<Tuple<string, int>, TcpClient> _connectionPool; //必ず _syncObject で保護してアクセスすること
        
        public bool HasConnection { get { return _connectionPool.Count > 0; } }

        public TcpCore(Tuple<string, int>[] hosts)
        {
            _syncObject = new object();
            _connectionPool = new Dictionary<Tuple<string, int>, TcpClient>();
            _hosts = hosts;
        }

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
                        tcp = null;
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
                            Logger.ErrorException("エラーになるケースが存在する気がしたのでトラップ. ここで中断させたくないので処理継続", ex);
                        }

                        tcp.Value.Close();
                    }

                    _connectionPool.Remove(tcp.Key);
                }
            }
        }

        public IDisposable HealthCheck()
        {
            return Observable.Start(() =>
            {
                var interval = TimeSpan.FromSeconds(1);
                IRetryPolicy retryPolicy = new ExponentialBackoff(
                    TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), 3);

                while (true)
                {
                    foreach (var host in _hosts)
                    {
                        try
                        {
                            var trialClient = new TcpClient();
                            trialClient.Connect(host.Item1, host.Item2);

                            lock (_syncObject)
                            {
                                TcpClient pooledClient;
                                if (_connectionPool.TryGetValue(host, out pooledClient) && pooledClient.Connected)
                                {
                                    trialClient.Close();
                                }
                                else
                                {
                                    _connectionPool[host] = trialClient;
                                }
                            }
                        }
                        catch (SocketException)
                        {
                            lock (_syncObject)
                            {
                                TcpClient pooledClient;
                                if (_connectionPool.TryGetValue(host, out pooledClient))
                                {
                                    try
                                    {
                                        pooledClient.GetStream().Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.ErrorException("ひょっとしたらエラーになるかもしれないのでトラップ", ex);
                                    }

                                    pooledClient.Close();
                                    _connectionPool.Remove(host);
                                }
                            }
                        }
                    }

                    if (_connectionPool.Any())
                    {
                        //Logger.Debug("enable {0} connection", ConnectionPool.Count());
                        retryPolicy.Reset();
                    }
                    else
                    {
                        if (retryPolicy.TryGetNext(out interval))
                        {
                            //Update next interval time.
                            Logger.Debug("next retry interval:{0}sec", interval.TotalSeconds);
                        }
                        else
                        {
                            break;
                        }
                    }

                    Thread.Sleep(interval);
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

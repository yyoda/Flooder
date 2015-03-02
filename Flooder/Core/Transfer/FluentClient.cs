using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Flooder.Core.RetryPolicy;
using Flooder.Core.Utility;
using MsgPack.Serialization;
using NLog;

namespace Flooder.Core.Transfer
{
    //public class FluentClient : IEmitter
    //{
    //    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    //    private static readonly SerializationContext DefaultContext = new SerializationContext();

    //    private TcpClient _client;
    //    private readonly int _port;
    //    private readonly string[] _hosts;

    //    public IRetryPolicy RetryPolicy { get; set; }

    //    public FluentClient(string hosts, int port)
    //        : this(new[] { hosts }, port)
    //    {
    //    }

    //    public FluentClient(TcpClient tcp)
    //    {
    //        _client = tcp;
    //    }

    //    public FluentClient(string[] hosts, int port)
    //    {
    //        if (!hosts.Any()) throw new ArgumentNullException("hosts");
    //        if (port <= 0) throw new ArgumentNullException("port");

    //        _hosts = hosts;
    //        _port = port;
    //        RetryPolicy = new ExponentialBackoff(retryCount: 3);
    //    }

    //    public void Connect()
    //    {
    //        while (true)
    //        {
    //            if (TryConnect(_hosts, _port, out _client))
    //            {
    //                break;
    //            }

    //            TimeSpan interval;
    //            if (RetryPolicy.TryGetNext(out interval))
    //            {
    //                Logger.Debug("connection fails. waiting for {0}sec.", interval.TotalSeconds);
    //                Thread.Sleep(interval);
    //                continue;
    //            }

    //            Logger.Debug("give up the retry connection.");
    //            break;
    //        }

    //        RetryPolicy.Reset();
    //    }

    //    // TODO 他のホストに切り替えてretryする。retryは、NG Host管理を検討する
    //    public static bool TryConnect(string[] hosts, int port, out TcpClient client)
    //    {
    //        // TODO Connection Pool、Java版はConnection Poolしていない、要検討。
    //        var tcp = new TcpClient();

    //        try
    //        {
    //            var host = hosts.First();
    //            tcp.Connect(host, port);  // TODO Hostの選択はランダムにして再接続時に別ホストにする？
    //            client = tcp;
    //            Logger.Debug("connected. host:{0}, port:{1}", host, port);
    //            return true;
    //        }
    //        catch (SocketException ex)
    //        {
    //            Logger.DebugException("tcp connect error.", ex);
    //            client = null;
    //            return false;
    //        }
    //    }

    //    public void Emit(string tag, IDictionary<string, object> payload)
    //    {
    //        var timestamp = DateTime.Now.ToUnixTime();

    //        using (var ms = new MemoryStream())
    //        {
    //            var packer = MsgPack.Packer.Create(ms);

    //            packer.PackArrayHeader(3); // ["tag", timestamp, obj]
    //            packer.PackString(tag);
    //            packer.Pack(timestamp);
    //            packer.PackMapHeader(payload);

    //            foreach (var row in payload)
    //            {
    //                packer.PackString(row.Key);

    //                new SerializationContext()
    //                    .GetSerializer(row.Value.GetType(), DefaultContext)
    //                    .PackTo(packer, row.Value);
    //            }

    //            var arraySegment = new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
    //            var bytes = arraySegment.ToArray();

    //            Emit(bytes);
    //        }
    //    }

    //    public void Emit(byte[] bytes)
    //    {
    //        while (true)
    //        {
    //            try
    //            {
    //                _client.GetStream().Write(bytes, 0, bytes.Length);
    //                _client.GetStream().Flush();
    //                break;
    //            }
    //            catch (SocketException)
    //            {
    //                TimeSpan interval;
    //                if (RetryPolicy.TryGetNext(out interval))
    //                {
    //                    Logger.Debug("emit fails. waiting for {0}sec.", interval.TotalSeconds);
    //                    Thread.Sleep(interval);
    //                }
    //                else
    //                {
    //                    Logger.Debug("give up the emit.");
    //                    break;
    //                }

    //                //recconect
    //                if (!_client.Connected)
    //                {
    //                    if (TryConnect(_hosts, _port, out _client))
    //                    {
    //                        RetryPolicy.Reset();
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public void Close()
    //    {
    //        if (_client == null) return;

    //        _client.Close();
    //        _client = null;

    //        Logger.Debug("connection closed.");
    //    }

    //    public void Dispose()
    //    {
    //        Close();
    //    }
    //}
}

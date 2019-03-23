using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Simuro5v5
{
    public static class V5RPC
    {
        public static async Task<Peer> Accept(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            var c = new UdpClient(port);
            while (true)
            {
                var result = await c.ReceiveAsync();
                var io = new MemoryStream(result.Buffer);
                try
                {
                    var pkt = io.ReadV5Packet();
                    if (pkt.SYN)
                    {
                        var peer = new Peer
                        {
                            myClient = c,
                            myPort = (ushort)port,
                            remoteEndpoint = new IPEndPoint(result.RemoteEndPoint.Address, pkt.port),
                            seqTx = 0,
                            seqRx = pkt.seq
                        };
                        SendACK(peer, pkt.seq);
                        return peer;
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public static async Task<byte[]> Receive(Peer peer)
        {
            while (true)
            {
                var result = await peer.myClient.ReceiveAsync();
                var io = new MemoryStream(result.Buffer);
                try
                {
                    var pkt = io.ReadV5Packet();
                    if (pkt.SYN)
                    {
                        peer.seqRx = pkt.seq;
                        SendACK(peer, pkt.seq);
                    }
                    else
                    {
                        if (!pkt.ACK && peer.seqRx <= pkt.seq)
                        {
                            SendACK(peer, pkt.seq);
                            if (peer.seqRx < pkt.seq)
                            {
                                peer.seqRx = pkt.seq;
                                var reader = new BinaryReader(io);
                                var payload = reader.ReadBytes((int)(io.Length - io.Position));
                                return payload;
                            }
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public static async Task Send(Peer peer, byte[] payload)
        {
            await Send(peer, payload, false);
        }

        public static async Task Sync(Peer peer)
        {
            await Send(peer, null, true);
        }

        private static async Task Send(Peer peer, byte[] payload, bool sync)
        {
            const int RETRY_INTERVAL = 10;
            const int MAX_RETRY = 300;
            var payloadLen = payload == null ? 0 : payload.Length;
            if (sync && payloadLen != 0)
            {
                throw new ArgumentException("A sync packet cannot have a payload");
            }
            peer.seqTx += 1;
            byte[] data = null;
            uint mySeq = peer.seqTx;
            using (var io = new MemoryStream())
            {
                var pkt = new V5Packet
                {
                    magic = V5Packet.MAGIC,
                    seq = mySeq,
                    flags = sync ? V5Packet.SYN_MASK : (ushort)0,
                    port = peer.myPort,
                    len = (ushort)payloadLen
                };
                io.Write(pkt);
                if (payload != null)
                {
                    io.Write(payload, 0, payloadLen);
                }
                data = io.ToArray();
            }
            void SendMe()
            {
                peer.myClient.Send(data, data.Length, peer.remoteEndpoint);
            }
            var timeout = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();
            var receiver = Task.Run(async () =>
              {
                  while (true)
                  {
                      cts.Token.ThrowIfCancellationRequested();
                      var result = await peer.myClient.ReceiveAsync();
                      var io = new MemoryStream(result.Buffer);
                      try
                      {
                          var pkt = io.ReadV5Packet();
                          if (pkt.SYN)
                          {
                              peer.seqRx = pkt.seq;
                              SendACK(peer, pkt.seq);
                          }
                          else if (pkt.ACK)
                          {
                              if (pkt.seq == mySeq)
                              {
                                  return;
                              }
                          }
                          else
                          {
                              if (pkt.seq <= peer.seqRx)
                              {
                                  SendACK(peer, pkt.seq);
                              }
                          }
                      }
                      catch
                      {
                          throw;
                      }
                  }
              }, cts.Token);
            SendMe();
            int nRetry = 0;
            Timer timer = null;
            timer = new Timer((object state) =>
            {
                Console.WriteLine($"Timer tick {nRetry}");
                ++nRetry;
                if (nRetry > MAX_RETRY)
                {
                    timeout.SetResult(true);
                    cts.Cancel();
                }
                else
                {
                    SendMe();
                }
            }, null, RETRY_INTERVAL, RETRY_INTERVAL);
            try
            {
                await Task.WhenAny(new Task[] { receiver, timeout.Task });
            }
            catch
            {
            }
            timer.Dispose();
            if (timeout.Task.Result)
            {
                throw new TimeoutException();
            }
        }

        static void SendACK(in Peer peer, uint seq)
        {
            var pkt = new V5Packet
            {
                magic = V5Packet.MAGIC,
                seq = seq,
                flags = V5Packet.ACK_MASK,
                port = peer.myPort,
                len = 0
            };
            var io = new MemoryStream();
            io.Write(pkt);
            var arr = io.ToArray();
            peer.myClient.Send(arr, arr.Length, peer.remoteEndpoint);
        }

        static V5Packet ReadV5Packet(this Stream io)
        {
            var pkt = new V5Packet();
            var reader = new BinaryReader(io);
            pkt.magic = reader.ReadUInt32();
            if (pkt.magic != V5Packet.MAGIC)
            {
                throw new InvalidDataException($"Invalid magic {pkt.magic}");
            }
            pkt.seq = reader.ReadUInt32();
            pkt.flags = reader.ReadUInt16();
            pkt.port = reader.ReadUInt16();
            pkt._reserved = reader.ReadUInt16();
            pkt.len = reader.ReadUInt16();
            return pkt;
        }

        static void Write(this Stream io, in V5Packet pkt)
        {
            var writer = new BinaryWriter(io);
            writer.Write(pkt.magic);
            writer.Write(pkt.seq);
            writer.Write(pkt.flags);
            writer.Write(pkt.port);
            writer.Write(pkt._reserved);
            writer.Write(pkt.len);
        }
    }

    public class Peer
    {
        public ushort myPort;
        public UdpClient myClient;
        public IPEndPoint remoteEndpoint;
        public uint seqTx;
        public uint seqRx;
        public override string ToString()
        {
            return $"Peer ({myPort} <-> {remoteEndpoint} TX: {seqTx} RX: {seqRx})";
        }
    }

#pragma warning disable IDE0049
    struct V5Packet
    {
        public static UInt32 MAGIC = 0x2b2b3556;
        public static UInt16 SYN_MASK = 0x1 << 0;
        public static UInt16 ACK_MASK = 0x1 << 1;
        public UInt32 magic;
        public UInt32 seq;
        public UInt16 flags;
        public UInt16 port;
        public UInt16 _reserved;
        public UInt16 len;
        static bool CheckFlag(UInt16 flags, UInt16 mask)
        {
            return (flags & mask) != 0;
        }
        static void AssignFlag(UInt16 flags, UInt16 mask, bool x)
        {
            if (x)
            {
                flags |= mask;
            }
            else
            {
                flags &= (UInt16)~mask;
            }
        }
        public bool SYN
        {
            get { return CheckFlag(flags, SYN_MASK); }
            set { AssignFlag(flags, SYN_MASK, value); }
        }
        public bool ACK
        {
            get { return CheckFlag(flags, ACK_MASK); }
            set { AssignFlag(flags, ACK_MASK, value); }
        }
    }
#pragma warning restore IDE0049

}

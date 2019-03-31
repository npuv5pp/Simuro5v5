using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace V5RPC
{
    public class V5Client : IDisposable
    {
        UdpClient udpClient;
        bool isDisposed = false;
        public IPEndPoint Server { get; set; }

        public V5Client(int port = 0)
        {
            udpClient = new UdpClient(port);
        }

        public byte[] Call(byte[] payload, int timeout = 10000, int retryInterval = 50)
        {
            var server = Server;
            if (server == null)
            {
                throw new InvalidOperationException("Server is not specified");
            }
            byte[] outBuffer;
            Guid outGuid;
            {
                var packet = V5Packet.MakeRequestPacket(payload);
                var io = new MemoryStream();
                io.Write(packet);
                outBuffer = io.ToArray();
                outGuid = packet.requestId;
            }
            Action SendMe = () =>
            {
                udpClient.Send(outBuffer, outBuffer.Length, server);
            };
            var timeoutState = new TaskCompletionSource<bool>();
            var cts = new CancellationTokenSource();
            var receiverTask = Task.Run(() =>
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    try
                    {
                        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        var inBuffer = udpClient.Receive(ref remote);
                        var io = new MemoryStream(inBuffer);
                        var packet = io.ReadV5Packet();
                        if (packet.Reply && packet.requestId == outGuid)
                        {
                            return packet.payload;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }, cts.Token);
            SendMe();
            var startTime = DateTime.Now;
            Timer timer = null;
            timer = new Timer((object state) =>
            {
                var currentTime = DateTime.Now;
                var diffTime = currentTime - startTime;
                if (diffTime.TotalMilliseconds >= timeout)
                {
                    timeoutState.SetResult(true);
                    cts.Cancel();
                    timer.Dispose();
                }
                else
                {
                    SendMe();
                }
            }, null, retryInterval, retryInterval);
            Task.WaitAny(new Task[] { receiverTask, timeoutState.Task });
            timer.Dispose();
            if (timeoutState.Task.IsCompleted)
            {
                throw new TimeoutException();
            }
            return receiverTask.Result;
        }

        void IDisposable.Dispose()
        {
            if (!isDisposed)
            {
                udpClient.Dispose();
                isDisposed = true;
            }
        }
    }

    public class V5Server : IDisposable
    {
        UdpClient udpClient;
        bool isDisposed = false;
        bool breakFlag = false;
        CacheItem lastResponse;

        public V5Server(int port)
        {
            udpClient = new UdpClient(port);
        }

        public delegate byte[] Procedure(byte[] parameter);

        public void Run(Procedure proc)
        {
            breakFlag = false;
            while (!isDisposed && !breakFlag)
            {
                try
                {
                    var endPoint = new IPEndPoint(IPAddress.Any, 0);
                    V5Packet inPacket;
                    {
                        var buffer = udpClient.Receive(ref endPoint);
                        using (var io = new MemoryStream(buffer))
                        {
                            inPacket = io.ReadV5Packet();
                        }
                    }

                    byte[] response;
                    if (lastResponse.requestId == inPacket.requestId)
                    {
                        response = lastResponse.response;
                    }
                    else
                    {
                        response = proc(inPacket.payload);
                        if (response == null)
                        {
                            response = new byte[0];
                        }
                        lastResponse.requestId = inPacket.requestId;
                        lastResponse.response = response;
                    }

                    {
                        V5Packet outPacket = V5Packet.MakeResponsePacket(response, inPacket.requestId);
                        var io = new MemoryStream();
                        io.Write(outPacket);
                        var buffer = io.ToArray();
                        udpClient.Send(buffer, buffer.Length, endPoint);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void Break()
        {
            breakFlag = true;
        }

        void IDisposable.Dispose()
        {
            if (!isDisposed)
            {
                udpClient.Dispose();
                isDisposed = true;
            }
        }

        struct CacheItem
        {
            public Guid requestId;
            public byte[] response;
        }
    }

    static class V5PacketReadWrite
    {
        public static V5Packet ReadV5Packet(this Stream io)
        {
            var packet = new V5Packet();
            var reader = new BinaryReader(io);
            packet.magic = reader.ReadUInt32();
            if (packet.magic != V5Packet.MAGIC)
            {
                throw new InvalidDataException($"Invalid magic {packet.magic}");
            }
            packet.requestId = new Guid(reader.ReadBytes(16));
            packet.flags = reader.ReadByte();
            packet.length = reader.ReadUInt16();
            packet.payload = reader.ReadBytes(packet.length);
            return packet;
        }

        public static void Write(this Stream io, in V5Packet packet)
        {
            var writer = new BinaryWriter(io);
            writer.Write(V5Packet.MAGIC);
            writer.Write(packet.requestId.ToByteArray());
            writer.Write(packet.flags);
            writer.Write(packet.length);
            writer.Write(packet.payload);
        }
    }

    struct V5Packet
    {
        public static uint MAGIC = 0x2b2b3556;
        public static byte REPLY_MASK = 0x1;

        public uint magic;//4 bytes
        public Guid requestId;//16 bytes
        public byte flags;//1 byte
        public ushort length;//2 bytes
        public byte[] payload;

        public static V5Packet MakeRequestPacket(byte[] payload)
        {
            if (payload.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"Payload too large: {payload.Length}");
            }
            V5Packet packet;
            packet.magic = MAGIC;
            packet.requestId = Guid.NewGuid();
            packet.flags = 0;
            packet.length = (ushort)payload.Length;
            packet.payload = payload;
            packet.Reply = false;
            return packet;
        }

        public static V5Packet MakeResponsePacket(byte[] payload, Guid requestId)
        {
            if (payload.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"Payload too large: {payload.Length}");
            }
            V5Packet packet;
            packet.magic = MAGIC;
            packet.requestId = requestId;
            packet.flags = 0;
            packet.length = (ushort)payload.Length;
            packet.payload = payload;
            packet.Reply = true;
            return packet;
        }

        bool CheckFlag(byte mask)
        {
            return (flags & mask) != 0;
        }
        void AssignFlag(byte mask, bool x)
        {
            if (x)
            {
                flags |= mask;
            }
            else
            {
                flags &= (byte)~mask;
            }
        }
        public bool Reply
        {
            get { return CheckFlag(REPLY_MASK); }
            set { AssignFlag(REPLY_MASK, value); }
        }
    }

}

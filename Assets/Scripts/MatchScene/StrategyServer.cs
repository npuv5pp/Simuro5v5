using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simuro5v5.Strategy;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Logger = Simuro5v5.Logger;
using Simuro5v5.Config;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServerMessage;
using Newtonsoft.Json;
using Simuro5v5;
using System.Text;
using System.Runtime.Serialization;

/// <summary>
/// 管理策略服务器以及与其的连接
/// </summary>
public class StrategyServer : MonoBehaviour
{
    static bool Entered { get; set; }

    /// <summary>
    /// 与蓝方策略的连接，负责与其通讯
    /// </summary>
    public static ConnectionHandle BlueHandle { get; set; }
    /// <summary>
    /// 与黄方策略的连接，负责与其通讯
    /// </summary>
    public static ConnectionHandle YellowHandle { get; set; }

    // 平台的锁文件，策略服务器会通过此文件判断平台是否在运行
    static FileStream LockFile { get; set; }

    static Process BlueServer { get; set; }
    static Process YellowServer { get; set; }

    // 服务器正在正常退出
    static bool NormalExiting { get; set; }

    void Start()
    {
        if (!Entered)
        {
            DontDestroyOnLoad(this);
            if (StrategyConfig.RunStrategyServer)
            {
                CreateLockFile();
                StartBothServer(true);
            }
            EstablishConnection();
            Entered = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 尝试与策略建立连接
    /// </summary>
    public static void EstablishConnection()
    {
        Logger.MainLogger.LogInfo("Establishing connection...");

        if (StrategyConfig.UseUdp)
        {
            if (BlueHandle == null || !BlueHandle.Established)
            {
                BlueHandle = new UDPConnectionHandle(IPAddress.Loopback, StrategyConfig.BlueStrategyPort);
                BlueHandle.Connect(8, StrategyConfig.ConnectTimeout);
            }
            if (YellowHandle == null || !YellowHandle.Established)
            {
                YellowHandle = new UDPConnectionHandle(IPAddress.Loopback, StrategyConfig.YellowStrategyPort);
                YellowHandle.Connect(8, StrategyConfig.ConnectTimeout);
            }
        }
        else
        {
            if (BlueHandle == null || !BlueHandle.Established)
            {
                BlueHandle = new TCPConnectionHandle(IPAddress.Loopback, StrategyConfig.BlueStrategyPort);
                BlueHandle.Connect(3, StrategyConfig.ConnectTimeout);
            }
            if (YellowHandle == null || !YellowHandle.Established)
            {
                YellowHandle = new TCPConnectionHandle(IPAddress.Loopback, StrategyConfig.YellowStrategyPort);
                YellowHandle.Connect(3, StrategyConfig.ConnectTimeout);
            }
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public static void DestroyConnection()
    {
        if (BlueHandle != null && BlueHandle.Established)
        {
            BlueHandle.Close();
        }
        if (YellowHandle != null && YellowHandle.Established)
        {
            YellowHandle.Close();
        }
    }

    static void CreateLockFile()
    {
        if (LockFile != null)
        {
            LockFile.Close();
        }
        var tmppath = Path.GetTempPath();
        string lockpath;
        do
        {
            lockpath = Path.Combine(tmppath, Path.GetRandomFileName());
        } while (File.Exists(lockpath));
        LockFile = File.Open(lockpath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
    }

    static Process CreateServer(string desc, string logfilepath, int port, bool autoreboot)
    {
        var p = new Process
        {
            EnableRaisingEvents = autoreboot,
            StartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                FileName = StrategyConfig.StrategyServer,
                Arguments = string.Format("{0} -p {1} --log-file {2} {3} --lock-file {4} {5}",
            StrategyConfig.StrategyServerScript, port, logfilepath, "--log-append", LockFile.Name, StrategyConfig.UseUdp ? "--udp" : "")
            }
        };

        p.Exited += new EventHandler(delegate (object sender, EventArgs e)
        {
            if (NormalExiting)
            {
                Debug.Log("Server " + desc + " normal exited.");
            }
            else
            {
                var info = string.Format("Server " + desc + " exited unexceptedly (exit code {0}). Rebooting...", p.ExitCode);
                Logger.MainLogger.LogError(info);
                Debug.Log(info);
                CreateServer(desc, Utils.FixCmdPath(logfilepath), port, autoreboot).Start();
            }
        });
        return p;
    }

    /// <summary>
    /// 启动两个策略服务器
    /// </summary>
    /// <param name="autoreboot"></param>
    public static void StartBothServer(bool autoreboot)
    {
        new Thread(delegate ()
        {
            try
            {
                //lock (BlueServerLock)
                //{
                Logger.MainLogger.Log("Starting Blue Side Server");
                BlueServer = CreateServer("Blue", StrategyConfig.BlueStrategyLogFile,
                    StrategyConfig.BlueStrategyPort, autoreboot);
                BlueServer.Start();
                //}
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                throw;
            }
        }).Start();

        new Thread(delegate ()
        {
            try
            {
                //lock (YellowServerLock)
                //{
                Logger.MainLogger.Log("Starting Yellow Side Server");
                YellowServer = CreateServer("Yellow", StrategyConfig.YellowStrategyLogFile,
                    StrategyConfig.YellowStrategyPort, autoreboot);
                YellowServer.Start();
                //}
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                throw;
            }
        }).Start();
    }

    /// <summary>
    /// 关闭策略服务器
    /// </summary>
    public static void StopStrategyServer()
    {
        // 断开连接
        DestroyConnection();

        NormalExiting = true;

        LockFile.Close();
        File.Delete(LockFile.Name);
    }

    void OnApplicationQuit()
    {
        DestroyConnection();
        if (StrategyConfig.RunStrategyServer)
        {
            StopStrategyServer();
        }
    }
}

/// <summary>
/// 与策略连接的抽象
/// </summary>
public abstract class ConnectionHandle
{
    public virtual string ServerAddr { get; protected set; }
    public virtual int ServerPort { get; protected set; }
    public virtual bool Established { get; protected set; }

    protected virtual IPEndPoint ServerEndPoint { get; set; }
    protected virtual Socket Conn { get; set; }
    protected byte[] RecvBuf = new byte[10240];

    abstract public bool Connect(int retry, int timeout);

    protected virtual bool Handshake(int timeout)
    {
        Conn.ReceiveTimeout = timeout;
        bool succeed = false;
        try
        {
            succeed = SendThenRecv(new Message(MessageType.MSG_ping, null)).MsgType == MessageType.MSG_pong;
        }
        catch { }
        return succeed;
    }

    abstract public void SendMessage(Message msg);
    abstract public Message RecvMessage();

    public virtual Message SendThenRecv(Message msg)
    {
        SendMessage(msg);
        return RecvMessage();
    }

    public virtual void Close()
    {
        Conn.Close();
    }
}

internal class UDPConnectionHandle : ConnectionHandle
{
    public UDPConnectionHandle(string address, int port)
    {
        ServerAddr = address;
        ServerPort = port;
        ServerEndPoint = new IPEndPoint(IPAddress.Parse(address), port);

        Conn = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public UDPConnectionHandle(IPAddress address, int port)
    {
        ServerAddr = address.ToString();
        ServerPort = port;
        ServerEndPoint = new IPEndPoint(address, port);

        Conn = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public override bool Connect(int retry, int timeout)
    {
        bool succeed = false;

        while (!succeed && retry-- > 0)
        {
            Conn.Connect(ServerEndPoint);
            succeed = Handshake(timeout / 2);

            if (!succeed)
            {
                Thread.Sleep(timeout / 2);
            }
        }

        Established = succeed;
        Conn.ReceiveTimeout = 5000;
        Conn.SendTimeout = 5000;

        return succeed;
    }

    public override void SendMessage(Message msg)
    {
        Conn.Send(msg.ToJsonBytes());
    }

    public override Message RecvMessage()
    {
        int len = Conn.Receive(RecvBuf);
        var msg = Message.FromJson(RecvBuf, 0, len);
        if (msg.MsgType == MessageType.MSG_error)
        {
            Debug.Log("error msg recived: " + ((ErrorMsgContainer)msg.GetData()).Description);
        }
        return msg;
    }
}

internal class TCPConnectionHandle : ConnectionHandle
{
    public override bool Established { get { return Conn.Connected; } }
    const int headerlen = 2;
    const int msgmaxlen = 0xffff;
    protected byte[] SendBuf = new byte[10240];

    public TCPConnectionHandle(IPAddress address, int port)
    {
        ServerAddr = address.ToString();
        ServerPort = port;
        ServerEndPoint = new IPEndPoint(address, port);

        Conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public override bool Connect(int retry, int timeout)
    {
        bool succeed = false;

        while (!succeed && retry-- > 0)
        {
            try
            {
                Conn.Connect(ServerEndPoint);
            }
            catch (SocketException) { }

            succeed = Handshake(timeout / 2);
            if (!succeed)
            {
                Thread.Sleep(timeout / 2);
            }
        }

        Established = succeed;
        Conn.ReceiveTimeout = 5000;
        Conn.SendTimeout = 5000;

        return succeed;
    }

    public override Message RecvMessage()
    {
        int rsize = 0;
        RecvBuf[0] = RecvBuf[1] = 0;
        if ((rsize = Conn.Receive(RecvBuf, 2, SocketFlags.None)) != 2)
        {
            if (rsize == 0)
            {
                throw new TCPConnException("Connection Shutdown Unexceptedly");
            }
            else
            {
                throw new TCPConnException("Error Packet Header");
            }
        }
        // data size
        int size = RecvBuf[0] << 8 | RecvBuf[1];
        rsize = 0;
        if ((rsize = Conn.Receive(RecvBuf, size, SocketFlags.None)) != size)
        {
            if (rsize == 0)
            {
                throw new TCPConnException("Connection Shutdown Unexceptedly");
            }
            else
            {
                throw new TCPConnException("Error Packet Body Received");
            }
        }
        var msg = Message.FromJson(RecvBuf, 0, rsize);
        if (msg.MsgType == MessageType.MSG_error)
        {
            Debug.Log("error msg received: " + ((ErrorMsgContainer)msg.GetData()).Description);
        }
        return msg;
    }

    public override void SendMessage(Message msg)
    {
        // data size
        int size = msg.ToJsonBytes(SendBuf, 2);
        if (size > msgmaxlen)
        {
            throw new Exception("message too long");
        }
        else
        {
            // to big endian
            SendBuf[0] = (byte)(size >> 8);
            SendBuf[1] = (byte)(size & 0xff);
            Conn.Send(SendBuf, size + 2, SocketFlags.None);
        }
    }

    class TCPConnException : Exception
    {
        public TCPConnException()
        {
        }

        public TCPConnException(string message) : base(message)
        {
        }

        public TCPConnException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TCPConnException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

namespace ServerMessage
{
    /// <summary>
    /// 消息类型。
    /// 每个消息类型(TypeName)只能拥有一个实例
    /// </summary>
    public class MessageType
    {
        public static MessageType MSG_true { get; private set; }
        public static MessageType MSG_false { get; private set; }
        public static MessageType MSG_ping { get; private set; }
        public static MessageType MSG_pong { get; private set; }
        public static MessageType MSG_free { get; private set; }
        public static MessageType MSG_load { get; private set; }
        public static MessageType MSG_create { get; private set; }
        public static MessageType MSG_strategy { get; private set; }
        public static MessageType MSG_placement { get; private set; }
        public static MessageType MSG_destroy { get; private set; }
        public static MessageType MSG_placementinfo { get; private set; }
        public static MessageType MSG_wheelinfo { get; private set; }
        public static MessageType MSG_exit { get; private set; }
        public static MessageType MSG_fin { get; private set; }
        public static MessageType MSG_error { get; private set; }
        public static MessageType MSG_getteaminfo { get; private set; }
        public static MessageType MSG_setteaminfo { get; private set; }

        static MessageType()
        {
            MSG_true = new MessageType("true", null);
            MSG_false = new MessageType("false", null);
            MSG_ping = new MessageType("ping", null);
            MSG_pong = new MessageType("pong", null);
            MSG_free = new MessageType("free", null);
            MSG_load = new MessageType("load", typeof(FileMsgContainer));
            MSG_create = new MessageType("create", typeof(SideInfo));
            MSG_strategy = new MessageType("strategy", typeof(SideInfo));
            MSG_placement = new MessageType("placement", typeof(SideInfo));
            MSG_destroy = new MessageType("destroy", typeof(SideInfo));
            MSG_placementinfo = new MessageType("placementinfo", typeof(PlacementInfo));
            MSG_wheelinfo = new MessageType("wheelinfo", typeof(WheelInfo));
            MSG_exit = new MessageType("exit", null);
            MSG_fin = new MessageType("fin", null);
            MSG_error = new MessageType("error", typeof(ErrorMsgContainer));
            MSG_getteaminfo = new MessageType("getteaminfo", null);
            MSG_setteaminfo = new MessageType("setteaminfo", typeof(Teaminfo));
        }

        public string TypeName { get; private set; }
        public Type DataType { get; private set; }

        private static Dictionary<string, MessageType> TypeNameUsed = new Dictionary<string, MessageType>();

        public MessageType(string type, Type datatype)
        {
            if (TypeNameUsed.ContainsKey(type))
            {
                throw new MessageTypeError("type name " + type + " existed");
            }

            TypeName = type;
            DataType = datatype;
            TypeNameUsed[type] = this;
        }

        public static MessageType GetFromName(string name)
        {
            if (!TypeNameUsed.ContainsKey(name))
            {
                throw new MessageTypeError("type name " + name + " not found");
            }

            return TypeNameUsed[name];
        }

        public override string ToString()
        {
            return string.Format("MessageType({0})", TypeName);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Message
    {
        [JsonProperty("type")]
        public string TypeName
        {
            get { return MsgType.TypeName; }
            set { MsgType = MessageType.GetFromName(value); }
        }

        // 可能为null。
        // 序列化时，为null则data字段不存在
        // 反序列化时，如果data字段不存在或为null，则该字段为null
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        private Newtonsoft.Json.Linq.JObject JObjectData { get; set; }

        public MessageType MsgType { get; set; }

        public Message() { }

        public Message(MessageType msgtype, object data)
        {
            var objtype = data?.GetType();
            if (msgtype.DataType != objtype)
            {
                // 检查类型是否匹配
                throw new MessageError("unmatched type and data");
            }

            MsgType = msgtype;
            JObjectData = data == null ? null : Newtonsoft.Json.Linq.JObject.FromObject(data);
        }

        public static Message FromJson(string json)
        {
            Message msg = JsonConvert.DeserializeObject<Message>(json);
            if (msg.MsgType.DataType != null && msg.JObjectData == null)
            {
                // 检查data字段
                throw new MessageError("unexcepted null data");
            }
            return msg;
        }

        public static Message FromJson(byte[] bs, int offset, int len)
        {
            return FromJson(Encoding.UTF8.GetString(bs, offset, len));
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public byte[] ToJsonBytes()
        {
            return Encoding.UTF8.GetBytes(ToJson());
        }

        public int ToJsonBytes(byte[] bytes, int offset)
        {
            var cs = ToJson().ToCharArray();
            return Encoding.UTF8.GetBytes(cs, 0, cs.Length, bytes, offset);
        }

        public object GetData()
        {
            return JObjectData == null ? null : JObjectData.ToObject(MsgType.DataType);
        }
    }

    class MessageTypeError : Exception
    {
        public MessageTypeError()
        {
        }

        public MessageTypeError(string message) : base(message)
        {
        }

        public MessageTypeError(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    class MessageError : Exception
    {
        public MessageError()
        {
        }

        public MessageError(string message) : base(message)
        {
        }

        public MessageError(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    class FileMsgContainer
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }
    }

    class ErrorMsgContainer
    {
        [JsonProperty("errcode")]
        public int ErrCode { get; set; }
        [JsonProperty("errdesc")]
        public string Description { get; set; }
    }
}

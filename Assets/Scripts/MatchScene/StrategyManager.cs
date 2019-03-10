using System;
using System.Net;

namespace Simuro5v5.Strategy
{
    using ServerMessage;
    using Simuro5v5.EventSystem;
    using System.Collections.Generic;

    /// <summary>
    /// 策略管理器
    /// 提供对红蓝方策略的管理，包括加载、卸载、重载、运行以及查错等操作，是该模块对外的唯一接口。
    /// </summary>
    public class StrategyManager : IDisposable
    {
        private IStrategy blue;     // 蓝方策略
        private IStrategy yellow;   // 黄方策略

        public string BlueUrl { get; private set; }
        public string YellowUrl { get; private set; }
        public string BluePath { get; private set; }
        public string YellowPath { get; private set; }

        public StrategyManager() { }

        public bool IsBlueReady { get { return blue != null && blue.IsConnected(); } }
        public bool IsYellowReady { get { return yellow != null && yellow.IsConnected(); } }

        public void CheckBlueReady_ex()
        {
            if (blue == null)
            {
                throw new StrategyNeverConnectException("Blue is not loaded");
            }
            blue.CheckReady_ex();
        }

        public void CheckYellowReady_ex()
        {
            if (yellow == null)
            {
                throw new StrategyNeverConnectException("Yellow is not loaded");
            }
            yellow.CheckReady_ex();
        }

        /// <summary>
        /// 如果策略未成功加载则抛出错误，以得到错误的详细信息
        /// </summary>
        public void CheckReady_ex()
        {
            CheckBlueReady_ex();
            CheckYellowReady_ex();
        }

        /// <summary>
        /// 获得当前蓝方策略的描述信息
        /// </summary>
        /// <returns>描述信息</returns>
        public string GetBlueDescription()
        {
            return blue != null ? blue.Description : "null";
        }

        /// <summary>
        /// 获得当前黄方策略的描述信息
        /// </summary>
        /// <returns>描述信息</returns>
        public string GetYellowDescription()
        {
            return yellow != null ? yellow.Description : "null";
        }

        public void LoadLastSaved()
        {
            var lastsaved = LoadInfo.GetLastInfo();
            LoadBlueDll(lastsaved.BlueDllPath);
            LoadYellowDll(lastsaved.YellowDllPath);
        }

        public void LoadBlueDll(string dllpath)
        {
            if (blue != null)
            {
                blue.Free();
            }

            BluePath = dllpath;
            blue = GetLocalDllStrategyFromPath(dllpath, Side.Blue);
            Event.Send(Event.EventType1.StrategyBlueLoaded, blue);
        }

        public void LoadYellowDll(string dllpath)
        {
            if (yellow != null)
            {
                yellow.Free();
            }

            YellowPath = dllpath;
            yellow = GetLocalDllStrategyFromPath(dllpath, Side.Yellow);
            Event.Send(Event.EventType1.StrategyYellowLoaded, yellow);
        }

        public void LoadBlueDebugStrategy()
        {
            if (blue != null)
            {
                blue.Free();
            }

            BluePath = "Debug";
            blue = new DebugStrategy();
            Event.Send(Event.EventType1.StrategyBlueLoaded, blue);
        }

        public void LoadYellowDebugStrategy()
        {
            if (yellow != null)
            {
                yellow.Free();
            }

            YellowPath = "Debug";
            yellow = new DebugStrategy();
            Event.Send(Event.EventType1.StrategyYellowLoaded, yellow);
        }

        public void LoadBlueDebugStrategy(WheelInfo wi)
        {
            if (blue != null)
            {
                blue.Free();
            }

            BluePath = "Debug";
            blue = new DebugStrategy(wi);
            Event.Send(Event.EventType1.StrategyBlueLoaded, blue);
        }

        public void LoadYellowDebugStrategy(WheelInfo wi)
        {
            if (yellow != null)
            {
                yellow.Free();
            }

            YellowPath = "Debug";
            yellow = new DebugStrategy(wi);
            Event.Send(Event.EventType1.StrategyYellowLoaded, yellow);
        }

        /// <summary>
        /// 重新加载蓝方策略
        /// </summary>
        public void ReloadBlue()
        {
            RemoveBlueDll();
            LoadBlueDll(BlueUrl);
        }

        /// <summary>
        /// 重新加载黄方策略
        /// </summary>
        public void ReloadYellow()
        {
            RemoveYellowDll();
            LoadYellowDll(YellowUrl);
        }

        /// <summary>
        /// 释放蓝方策略，并移除引用
        /// </summary>
        public void RemoveBlueDll()
        {
            if (blue != null)
            {
                blue.Free();
                Event.Send(Event.EventType1.StrategyBlueFreed, blue.IsConnected());

                blue = null;
            }
        }

        /// <summary>
        /// 释放蓝方策略，并移除引用
        /// </summary>
        public void RemoveYellowDll()
        {
            if (yellow != null)
            {
                yellow.Free();
                Event.Send(Event.EventType1.StrategyYellowFreed, yellow.IsConnected());

                yellow = null;
            }
        }

        /// <summary>
        /// 向蓝方策略发送开始信号
        /// </summary>
        /// <param name="minfo">开始时的赛场信息</param>
        public void BeginBlue(MatchInfo minfo)
        {
            blue.SendBegin(minfo.GetBlueSide());
        }

        /// <summary>
        /// 向黄方策略发送开始信号
        /// </summary>
        /// <param name="minfo">开始时的赛场信息</param>
        public void BeginYellow(MatchInfo minfo)
        {
            yellow.SendBegin(minfo.GetYellowSide());
        }

        /// <summary>
        /// 向蓝方策略发送下一拍信号
        /// </summary>
        /// <param name="minfo">当前的赛场信息</param>
        /// <returns>策略返回的轮速</returns>>
        public WheelInfo NextBlue(MatchInfo minfo)
        {
            return blue.SendNext(minfo.GetBlueSide());
        }

        /// <summary>
        /// 向黄方策略发送当前下一拍信号
        /// </summary>
        /// <param name="minfo">当前的赛场信息</param>
        /// <returns>策略返回的轮速</returns>>
        public WheelInfo NextYellow(MatchInfo minfo)
        {
            return yellow.SendNext(minfo.GetYellowSide());
        }

        /// <summary>
        /// 向蓝方策略发送点球信号
        /// </summary>
        /// <param name="minfo">当前的赛场信息</param>
        /// <returns>策略返回的摆位信息</returns>
        public PlacementInfo PlacementBlue(MatchInfo minfo)
        {
            return blue.SendPlacement(minfo.GetBlueSide());
        }

        /// <summary>
        /// 向黄方策略发送点球信号
        /// </summary>
        /// <param name="minfo">当前的赛场信息</param>
        /// <returns>策略返回的摆位信息</returns>
        public PlacementInfo PlacementYellow(MatchInfo minfo)
        {
            return yellow.SendPlacement(minfo.GetYellowSide());
        }

        /// <summary>
        /// 向黄方策略发送结束信号
        /// </summary>
        /// <param name="minfo">当前的赛场信息</param>
        public void OverBlue(MatchInfo minfo)
        {
            blue.SendOver(minfo.GetBlueSide());
        }

        /// <summary>
        /// 向黄方策略发送结束信号
        /// </summary>
        /// <param name="minfo">当前的赛场信息</param>
        public void OverYellow(MatchInfo minfo)
        {
            yellow.SendOver(minfo.GetYellowSide());
        }

        private static IStrategy GetLocalDllStrategyFromPath(string dllpath, Side sidefor)
        {
            return new NetStrategy(
                sidefor == Side.Blue ? StrategyServer.BlueHandle : StrategyServer.YellowHandle,
                dllpath);
        }

        public virtual void Dispose()
        {
            RemoveBlueDll();
            RemoveYellowDll();
        }
    }

    /// <summary>
    /// 一个策略须实现的接口；实现了这些接口的类即可作为一个策略
    /// </summary>
    interface IStrategy
    {
        /// <summary>
        /// 策略的描述信息
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 以返回值判断是否连接
        /// </summary>
        /// <returns>成功连接为true，否则为false</returns>
        bool IsConnected();

        /// <summary>
        /// 以抛出错误的方式检查策略
        /// </summary>
        void CheckReady_ex();

        /// <summary>
        /// 比赛开始
        /// </summary>
        /// <param name="sideInfo">
        /// 当前策略对应的<see cref="SideInfo">SideInfo</see>
        /// </param>
        void SendBegin(SideInfo sideInfo);

        /// <summary>
        /// 策略进行下一拍的计算
        /// </summary>
        /// <param name="sideInfo">
        /// 当前策略对应的<see cref="SideInfo">SideInfo</see>
        /// </param>
        /// <returns>轮速</returns>
        WheelInfo SendNext(SideInfo sideInfo);

        PlacementInfo SendPlacement(SideInfo sideInfo);

        /// <summary>
        /// 比赛结束，策略销毁
        /// </summary>
        /// <param name="sideInfo">
        /// 当前策略对应的<see cref="SideInfo">SideInfo</see>
        /// </param>
        void SendOver(SideInfo sideInfo);

        /// <summary>
        /// 显示调用以释放策略所持有的资源
        /// </summary>
        /// <remarks>
        /// 须允许多次调用，即Free()之后再次调用Free()要保证不会出错。
        /// </remarks>
        void Free();
    }

    class NetStrategy : IStrategy
    {
        public string Description { get { return string.Format("{0} in {1}:{2}", DllPath, ServerAddr, ServerPort); } }

        public string DllPath { get; private set; }
        public string ServerAddr { get { return Conn.ServerAddr; } }
        public int ServerPort { get { return Conn.ServerPort; } }
        public bool LoadSucceed { get; private set; }

        private ConnectionHandle Conn;

        public NetStrategy(ConnectionHandle handle, string dllpath)
        {
            DllPath = dllpath;
            Conn = handle;
            SendLoad();
        }

        public NetStrategy(IPAddress address, int port, string dllpath, bool connectNow)
        {
            DllPath = dllpath;
            Conn = new UDPConnectionHandle(address, port);
            if (connectNow)
            {
                Conn.Connect(5, 400);
                SendLoad();
            }
        }

        protected bool SendLoad()
        {
            var resp = Conn.SendThenRecv(new Message(MessageType.MSG_load, new FileMsgContainer { FileName = DllPath }));
            LoadSucceed = resp.MsgType == MessageType.MSG_fin;
            return LoadSucceed;
        }

        public void CheckReady_ex()
        {
            if (!Conn.Established)
            {
                throw new StrategyDisconnectException("Strategy Not Connected");
            }
            if (!LoadSucceed)
            {
                throw new StrategyConnectFailed("Strategy Load Failed");
            }
        }

        public bool IsConnected()
        {
            return Conn.Established && LoadSucceed;
        }

        public void Free()
        {
            Conn.SendThenRecv(new Message(MessageType.MSG_free, null));
        }

        public void SendBegin(SideInfo sideInfo)
        {
            Conn.SendThenRecv(new Message(MessageType.MSG_create, sideInfo));
        }

        public PlacementInfo SendPlacement(SideInfo sideInfo)
        {
            var resp = Conn.SendThenRecv(new Message(MessageType.MSG_placement, sideInfo));
            return (PlacementInfo)resp.GetData();
        }

        public WheelInfo SendNext(SideInfo sideInfo)
        {
            var resp = Conn.SendThenRecv(new Message(MessageType.MSG_strategy, sideInfo));
            return (WheelInfo)resp.GetData();
        }

        public void SendOver(SideInfo sideInfo)
        {
            Conn.SendThenRecv(new Message(MessageType.MSG_destroy, sideInfo));
        }
        
        public void SendExit()
        {
            Conn.SendThenRecv(new Message(MessageType.MSG_exit, null));
        }
    }

    class DebugStrategy : IStrategy
    {
        WheelInfo output_wheelinfo;

        public DebugStrategy(WheelInfo wi)
        {
            output_wheelinfo = new WheelInfo
            {
                Wheels = new Wheel[5]
            {
                wi.Wheels[0],
                wi.Wheels[1],
                wi.Wheels[2],
                wi.Wheels[3],
                wi.Wheels[4],
            }
            };
        }

        public DebugStrategy()
        {
            var wi = new WheelInfo { Wheels = new Wheel[5] };
            wi.Wheels[0] = new Wheel { left = 40, right = 40 };
            wi.Wheels[1] = new Wheel { left = 40, right = 40 };
            wi.Wheels[2] = new Wheel { left = 40, right = 40 };
            wi.Wheels[3] = new Wheel { left = 40, right = 40 };
            wi.Wheels[4] = new Wheel { left = 40, right = 40 };
            output_wheelinfo = wi;
        }

        public string Description { get { return "Debug Strategy"; } }

        public void CheckReady_ex() { }

        public void Free() { }

        public bool IsConnected()
        {
            return true;
        }

        public void SendBegin(SideInfo sideInfo) { }

        public WheelInfo SendNext(SideInfo sideInfo)
        {
            return output_wheelinfo;
        }

        public void SendOver(SideInfo sideInfo) { }

        public PlacementInfo SendPlacement(SideInfo sideInfo)
        {
            return new PlacementInfo
            {
                Robot = sideInfo.home,
                Ball = sideInfo.currentBall
            };
        }
    }

    /// <summary>
    /// 缓存策略加载的dll信息
    /// </summary>
    static class LoadInfo
    {
        public struct DllInfo
        {
            public string BlueDllPath { get; set; }
            public string YellowDllPath { get; set; }
        }

        public static List<DllInfo> InfoList = new List<DllInfo>();

        private static DllInfo DefaultInfo { get; set; } 

        // TODO
        //static LoadInfo()
        //{
        //    var historyFile = new StreamReader("history");
        //}

        public static void SaveInfo(string bluedllpath, string yellowdllpath)
        {
            InfoList.Add(new DllInfo
            {
                BlueDllPath = bluedllpath,
                YellowDllPath = yellowdllpath
            });
        }

        public static DllInfo GetLastInfo()
        {
            return InfoList.Count > 0 ? InfoList[InfoList.Count - 1] : DefaultInfo;
        }
    }
}

namespace Simuro5v5
{
    /// <summary>
    /// 策略异常
    /// </summary>
    class StrategyException : SystemException
    {
        public StrategyException() { }

        public StrategyException(string message) : base(message) { }

        public StrategyException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 策略还没有连接
    /// </summary>
    class StrategyNeverConnectException : StrategyException
    {
        public StrategyNeverConnectException() { }

        public StrategyNeverConnectException(string message) : base(message) { }

        public StrategyNeverConnectException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 策略异常断开
    /// </summary>
    class StrategyDisconnectException : StrategyException
    {
        public StrategyDisconnectException() { }

        public StrategyDisconnectException(string message) : base(message) { }

        public StrategyDisconnectException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 策略连接失败
    /// </summary>
    class StrategyConnectFailed : StrategyException
    {
        public StrategyConnectFailed() { }

        public StrategyConnectFailed(string message) : base(message) { }

        public StrategyConnectFailed(string message, System.Exception innerException) : base(message, innerException) { }
    }
}

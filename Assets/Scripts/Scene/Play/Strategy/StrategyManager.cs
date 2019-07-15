using System;
using System.Linq;
using System.Net;
using V5RPC;

namespace Simuro5v5.Strategy
{
    public interface IStrategy
    {
        bool IsConnected { get; }
        bool Connect();
        void Close();
        void OnMatchStart();
        void OnMatchStop();
        void OnFirstHalfStart();
        void OnSecondHalfStart();
        void OnOvertimeStart();
        void OnPenaltyShootoutHalfStart();
        void OnJudgeResult(JudgeResult result);
        TeamInfo GetTeamInfo();
        WheelInfo GetInstruction(SideInfo sideInfo);
        PlacementInfo GetPlacement(SideInfo sideInfo);
    }

    public interface IStrategyFactory
    {
        IStrategy CreateBlue();
        IStrategy CreateYellow();
    }

    public class RPCStrategy : IStrategy
    {
        StrategyClient client;
        TeamInfo cachedTeamInfo;

        public RPCStrategy(IPEndPoint endPoint)
        {
            client = new StrategyClient(endPoint)
            {
                Timeout = Configuration.StrategyConfig.ConnectTimeout
            };
        }

        public RPCStrategy(string endPoint) : this(ParseEndPoint(endPoint))
        {
        }

        public bool IsConnected { get; private set; } = false;

        public bool Connect()
        {
            try
            {
                cachedTeamInfo = client.GetTeamInfo().ToNative();
            }
            catch (TimeoutException)
            {
                return false;
            }

            IsConnected = true;
            return true;
        }

        public void Close()
        {
            client.Dispose();
            cachedTeamInfo = null;
            IsConnected = false;
        }

        public void OnMatchStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.MatchStart, new V5RPC.Proto.EventArguments());
        }

        public void OnMatchStop()
        {
            client.OnEvent(V5RPC.Proto.EventType.MatchStop, new V5RPC.Proto.EventArguments());
        }

        public void OnFirstHalfStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.FirstHalfStart, new V5RPC.Proto.EventArguments());
        }

        public void OnSecondHalfStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.SecondHalfStart, new V5RPC.Proto.EventArguments());
        }

        public void OnOvertimeStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.OvertimeStart, new V5RPC.Proto.EventArguments());
        }

        public void OnPenaltyShootoutHalfStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.PenaltyShootoutStart, new V5RPC.Proto.EventArguments());
        }

        public TeamInfo GetTeamInfo()
        {
            if (cachedTeamInfo == null)
                cachedTeamInfo = client.GetTeamInfo().ToNative();
            return cachedTeamInfo;
        }

        public WheelInfo GetInstruction(SideInfo sideInfo)
        {
            var wheels = client.GetInstruction(sideInfo.ToProto());
            return new WheelInfo {Wheels = (from w in wheels select w.ToNative()).ToArray()};
        }

        public PlacementInfo GetPlacement(SideInfo sideInfo)
        {
            var placement = client.GetPlacement(sideInfo.ToProto());
            return new PlacementInfo
            {
                Robots = (from rb in placement.Robots select rb.ToNative()).ToArray(),
                Ball = placement.Ball.ToNative()
            };
        }

        public void OnJudgeResult(JudgeResult result)
        {
            client.OnEvent(V5RPC.Proto.EventType.JudgeResult,
                new V5RPC.Proto.EventArguments {JudgeResult = result.ToProto()});
        }

        //throws exceptions
        static IPEndPoint ParseEndPoint(string endPoint)
        {
            string addr;
            int port = 0;

            int colonIndex = endPoint.IndexOf(':');
            if (colonIndex == -1)
            {
                addr = endPoint;
            }
            else
            {
                addr = endPoint.Substring(0, colonIndex);
                port = int.Parse(endPoint.Substring(colonIndex + 1));
            }

            return new IPEndPoint(IPAddress.Parse(addr), port);
        }
    }

    public class RPCStrategyFactory : IStrategyFactory
    {
        public IStrategy CreateBlue()
        {
            throw new NotImplementedException();
        }

        public IStrategy CreateYellow()
        {
            throw new NotImplementedException();
        }
    }

    public class StrategyManager
    {
        public IStrategy Blue { get; private set; }
        public IStrategy Yellow { get; private set; }
        public IStrategyFactory StrategyFactory { get; set; }

        public bool IsBlueReady
        {
            get => Blue != null && Blue.IsConnected;
        }

        public bool IsYellowReady
        {
            get => Yellow != null && Yellow.IsConnected;
        }

        public bool IsReady
        {
            get => IsBlueReady && IsYellowReady;
        }

        public StrategyManager()
        {
        }

        public bool ConnectBlue()
        {
            if (Blue != null)
            {
                Blue.Close();
            }

            Blue = StrategyFactory.CreateBlue();
            return Blue.Connect();
        }

        public bool ConnectYellow()
        {
            if (Yellow != null)
            {
                Yellow.Close();
            }

            Yellow = StrategyFactory.CreateYellow();
            return Yellow.Connect();
        }

        /// <summary>
        /// 调换双方策略
        /// </summary>
        public void SwitchRole()
        {
            if (!IsBlueReady)
            {
                throw new StrategyException(Side.Blue, "blue strategy is not ready");
            }

            if (!IsYellowReady)
            {
                throw new StrategyException(Side.Blue, "blue strategy is not ready");
            }

            var tmp = Blue;
            Blue = Yellow;
            Yellow = tmp;
        }
    }

    [System.Serializable]
    public class StrategyException : System.Exception
    {
        public Side side;

        public StrategyException(Side side, string message) : base(message)
        {
            this.side = side;
        }

        public StrategyException(Side side, Exception innerException) : base(innerException.Message, innerException)
        {
            this.side = side;
        }
    }
}
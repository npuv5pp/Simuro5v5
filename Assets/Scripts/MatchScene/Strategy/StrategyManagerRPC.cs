using System;
using V5RPC;

namespace Simuro5v5.Strategy
{
    public class StrategyManagerRPC
    {
        int blue_local_port;
        int yellow_local_port;

        public IStrategyRPC BlueStrategy { get; private set; }
        public IStrategyRPC YellowStrategy { get; private set; }

        public TeamInfo BlueTeamInfo { get; set; }
        public TeamInfo YellowTeamInfo { get; set; }

        public bool IsBlueReady => BlueStrategy !=null;
        public bool IsYellowReady => YellowStrategy !=null;

        public StrategyManagerRPC() { }

        public void ConnectBlue(int server_port)
        {
            BlueStrategy = new RPCStrategy(Config.StrategyConfig.BlueStrategyPort);
            BlueTeamInfo = BlueStrategy.GetTeamInfo();
        }

        public void ConnectYellow(int server_port)
        {
            YellowStrategy = new RPCStrategy(Config.StrategyConfig.YellowStrategyPort);
            YellowTeamInfo = YellowStrategy.GetTeamInfo();
        }

        public void CloseBlue()
        {
            BlueStrategy.Close();
            BlueStrategy = null;
        }

        public void CloseYellow()
        {
            YellowStrategy.Close();
            YellowStrategy = null;
        }
    }

    public interface IStrategyRPC
    {
        TeamInfo GetTeamInfo();
        void OnMatchStart();
        void OnMatchStop();
        void OnRoundStart();
        void OnRoundStop();
        WheelInfo GetInstruction(SideInfo sideInfo);
        PlacementInfo GetPlacement(SideInfo sideInfo);

        void Close();
    }

    public class RPCStrategy : IStrategyRPC
    {
        StrategyClient client;

        public RPCStrategy(int server_port)
        {
            client = new StrategyClient(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, server_port));
        }

        public TeamInfo GetTeamInfo()
        {
            return client.GetTeamInfo().ToNative();
        }

        public WheelInfo GetInstruction(SideInfo sideInfo)
        {
            var wheels = client.GetInstruction(sideInfo.ToProto());
            var rv = new WheelInfo { Wheels = new Wheel[Const.RobotsPerTeam] };
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                rv.Wheels[i] = wheels[i].ToNative();
            }
            return rv;
        }

        public PlacementInfo GetPlacement(SideInfo sideInfo)
        {
            var placement = client.GetPlacement(sideInfo.ToProto());
            var rv = new PlacementInfo { Robots = new Robot[Const.RobotsPerTeam] };
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                rv.Robots[i] = placement.Robots[i].ToNative();
                rv.Ball = placement.Ball.ToNative();
            }
            return rv;
        }

        public void OnMatchStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.MatchStart, new V5RPC.Proto.EventArguments());
        }

        public void OnMatchStop()
        {
            client.OnEvent(V5RPC.Proto.EventType.MatchStop, new V5RPC.Proto.EventArguments());
        }

        public void OnRoundStart()
        {
            client.OnEvent(V5RPC.Proto.EventType.RoundStart, new V5RPC.Proto.EventArguments());
        }

        public void OnRoundStop()
        {
            client.OnEvent(V5RPC.Proto.EventType.RoundStop, new V5RPC.Proto.EventArguments());
        }

        public void Close()
        {
            client.Dispose();
        }
    }

    /// <summary>
    /// Protobuf的类和内部使用的类的互相转化
    /// </summary>
    public static class ProtoConverter
    {
        public static Vector2D ToNative(this V5RPC.Proto.Vector2 proto)
        {
            return new Vector2D
            {
                x = proto.X,
                y = proto.Y
            };
        }

        public static Ball ToNative(this V5RPC.Proto.Ball proto)
        {
            return new Ball
            {
                pos = proto.Position.ToNative()
            };
        }

        public static Wheel ToNative(this V5RPC.Proto.Wheel proto)
        {
            return new Wheel
            {
                left = proto.LeftSpeed,
                right = proto.RightSpeed
            };
        }

        public static Robot ToNative(this V5RPC.Proto.Robot proto)
        {
            return new Robot
            {
                pos = proto.Position.ToNative(),
                rotation = proto.Rotation,
                velocityLeft = proto.Wheel.LeftSpeed,
                velocityRight = proto.Wheel.RightSpeed
            };
        }

        public static TeamInfo ToNative(this V5RPC.Proto.TeamInfo proto)
        {
            return new TeamInfo
            {
                Name = proto.TeamName
            };
        }

        public static V5RPC.Proto.Vector2 ToProto(this Vector2D native)
        {
            return new V5RPC.Proto.Vector2 { X = native.x, Y = native.y };
        }

        public static V5RPC.Proto.Ball ToProto(this Ball native)
        {
            return new V5RPC.Proto.Ball { Position = native.pos.ToProto() };
        }

        public static V5RPC.Proto.Robot ToProto(this Robot native)
        {
            return new V5RPC.Proto.Robot
            {
                Position = native.pos.ToProto(),
                Rotation = (float)native.rotation,
                Wheel = new V5RPC.Proto.Wheel
                {
                    LeftSpeed = (float)native.velocityLeft,
                    RightSpeed = (float)native.velocityRight
                }
            };
        }

        public static V5RPC.Proto.Robot ToProto(this OpponentRobot native)
        {
            return new V5RPC.Proto.Robot
            {
                Position = native.pos.ToProto(),
                Rotation = (float)native.rotation,
                Wheel = new V5RPC.Proto.Wheel()
            };
        }

        public static V5RPC.Proto.Field ToProto(this SideInfo native)
        {
            var field = new V5RPC.Proto.Field();
            for (int i = 0; i < 5; i++)
            {
                field.OurRobots.Add(native.home[i].ToProto());
                field.OpponentRobots.Add(native.opp[i].ToProto());
            }
            field.Ball = native.currentBall.ToProto();
            return field;
        }
    }
}


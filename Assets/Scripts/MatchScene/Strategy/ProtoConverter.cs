using Simuro5v5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simuro5v5.Strategy
{
    /// <summary>
    /// Protobuf的类和内部使用的类的互相转化
    /// </summary>
    public static class ProtoConverter
    {
        public static Side ToNative(this V5RPC.Proto.Team side)
        {
            switch (side)
            {
                case V5RPC.Proto.Team.Self:
                    return Side.Blue;
                case V5RPC.Proto.Team.Opponent:
                    return Side.Yellow;
                default:
                    return Side.Nobody;
            }
        }

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
                wheel = new Wheel
                {
                    left = proto.Wheel.LeftSpeed,
                    right = proto.Wheel.RightSpeed
                }
            };
        }

        public static TeamInfo ToNative(this V5RPC.Proto.TeamInfo proto)
        {
            return new TeamInfo
            {
                Name = proto.TeamName
            };
        }

        public static V5RPC.Proto.Team ToProto(this Side side)
        {
            switch (side)
            {
                case Side.Blue:
                    return V5RPC.Proto.Team.Self;
                case Side.Yellow:
                    return V5RPC.Proto.Team.Opponent;
                default:
                    return V5RPC.Proto.Team.Nobody;
            }
        }

        public static V5RPC.Proto.JudgeResultEvent.Types.ResultType ToProto(this ResultType type)
        {
            switch (type)
            {
                case ResultType.FreeKickRightTop:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.FreeKickRightTop;
                case ResultType.FreeKickRightBot:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.FreeKickRightBot;
                case ResultType.FreeKickLeftTop:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.FreeKickLeftTop;
                case ResultType.FreeKickLeftBot:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.FreeKickLeftBot;
                case ResultType.GoalKick:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.GoalKick;
                case ResultType.PenaltyKick:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.PenaltyKick;
                case ResultType.PlaceKick:
                    return V5RPC.Proto.JudgeResultEvent.Types.ResultType.PlaceKick;

                default:
                    throw new ArgumentException($"Error type {type.ToString()}");
            }
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
                Rotation = native.rotation,
                Wheel = new V5RPC.Proto.Wheel
                {
                    LeftSpeed = native.wheel.left,
                    RightSpeed = native.wheel.right
                }
            };
        }

        public static V5RPC.Proto.Robot ToProto(this OpponentRobot native)
        {
            return new V5RPC.Proto.Robot
            {
                Position = native.pos.ToProto(),
                Rotation = native.rotation,
                Wheel = new V5RPC.Proto.Wheel()
            };
        }

        public static V5RPC.Proto.Field ToProto(this SideInfo native)
        {
            var field = new V5RPC.Proto.Field();
            for (int i = 0; i < 5; i++)
            {
                field.SelfRobots.Add(native.home[i].ToProto());
                field.OpponentRobots.Add(native.opp[i].ToProto());
            }
            field.Ball = native.currentBall.ToProto();
            field.Tick = native.TickMatch;
            return field;
        }

        public static V5RPC.Proto.JudgeResultEvent ToProto(this JudgeResult result)
        {
            var rv = new V5RPC.Proto.JudgeResultEvent
            {
                OffensiveTeam = result.Actor.ToProto(),
                Type = result.ResultType.ToProto()
            };
            return rv;
        }
    }
}

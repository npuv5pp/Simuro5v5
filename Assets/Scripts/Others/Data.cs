using UnityEngine;
using Simuro5v5.Config;
using Simuro5v5.Strategy;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Simuro5v5
{
    public enum GameState
    {
        NormalMatch = 0,
        FreeBallTop = 1,
        FreeBallBottom = 2,
        PlaceKick = 3,
        Penalty = 4,
        GoalKick = 5,
    }

    public enum Side
    {
        Yellow,
        Blue,
        Nobody,
    };

    /// <summary>
    /// Completely describe the information of a shot, including:
    /// 1. Information such as the position, angular velocity, and line speed of the object;
    /// 2. Game progress information;
    /// 3. Competition time;
    /// 4. Score;
    /// 5. Referee.
    ///
    /// 完整描述某一拍的信息，包括：
    /// 1. 物体的位置、角速度、线速度等信息；
    /// 2. 比赛进程信息；
    /// 3. 比赛时间；
    /// 4. 比分；
    /// 5. 裁判。
    /// </summary>
    public class MatchInfo : ICloneable
    {
        public Robot[] BlueRobots { get; set; }
        public Robot[] YellowRobots { get; set; }
        public Ball Ball;

        public int TickMatch;
        public int TickRound;

        public MatchScore Score;
        public Referee Referee;

        public MatchInfo()
        {
            BlueRobots = new Robot[Const.RobotsPerTeam];
            YellowRobots = new Robot[Const.RobotsPerTeam];
            Referee = new Referee();
        }

        public MatchInfo(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {
            BlueRobots = new Robot[Const.RobotsPerTeam];
            YellowRobots = new Robot[Const.RobotsPerTeam];
            Referee = new Referee();

            UpdateFrom(ball, blue, yellow);
        }

        public object Clone()
        {
            return new MatchInfo()
            {
                Ball = Ball,
                TickMatch = TickMatch,
                TickRound = TickRound,
                Score = Score,
                Referee = (Referee)Referee.Clone(),
                BlueRobots = (Robot[])BlueRobots.Clone(),
                YellowRobots = (Robot[])YellowRobots.Clone(),
            };
        }

        public static MatchInfo NewDefaultPreset()
        {
            var info = new MatchInfo();
            info.Ball.moveTo(0, 0);
            info.Ball.angularVelocity = 0;
            info.Ball.linearVelocity = Vector2D.Zero;
            //x, y, rotation
            var yellowData = new float[,]
            { { -102.5f, 0, 90 }, { -81.2f, 48, 0 }, { -81.2f, -48, 0 }, { -29.8f, 48, 0 }, { -29.8f, -48, 0 } };
            var blueData = new float[,]
            { { 102.5f, 0, -90 }, { 81.2f, -48, 180 }, { 81.2f, 48, 180 }, { 29.8f, -48, 180 }, { 29.8f, 48, 180 } };
            Robot InitRobot(Robot rb, float[,] data, int elem)
            {
                rb.pos.x = data[elem, 0];
                rb.pos.y = data[elem, 1];
                rb.rotation = data[elem, 2];
                rb.wheel.left = rb.wheel.right = 0;
                rb.linearVelocity = Vector2D.Zero;
                return rb;
            }
            Robot[] InitMe(IEnumerable<Robot> rbs, float[,] data)
            {
                return rbs.Select((rb, i) => InitRobot(rb, data, i)).ToArray();
            }
            info.YellowRobots = InitMe(info.YellowRobots, yellowData);
            info.BlueRobots = InitMe(info.BlueRobots, blueData);

            return info;
        }

        public void UpdateFrom(MatchInfo matchInfo)
        {
            BlueRobots = (Robot[])matchInfo.BlueRobots.Clone();
            YellowRobots = (Robot[])matchInfo.YellowRobots.Clone();
            Ball = matchInfo.Ball;
            TickMatch = matchInfo.TickMatch;
            Score = matchInfo.Score;
            Referee = matchInfo.Referee;
        }

        public void UpdateFrom(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {

            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                BlueRobots[i].UpdateFromRigidbody(blue[i].GetComponent<Rigidbody>());
                YellowRobots[i].UpdateFromRigidbody(yellow[i].GetComponent<Rigidbody>());
            }
            Rigidbody ballTemp = ball.GetComponent<Rigidbody>();
            Ball newBall = new Ball
            {
                mass = ballTemp.mass,
                pos = { x = ballTemp.position.x, y = ballTemp.position.z },
                linearVelocity = { x = ballTemp.velocity.x, y = ballTemp.velocity.z },
                angularVelocity = ballTemp.angularVelocity.y
            };
            //newBall.angularVelocity.x = ballTemp.angularVelocity.x;
            //newBall.angularVelocity.y = ballTemp.angularVelocity.z;
            //newBall.angularVelocity.z = ballTemp.angularVelocity.y;
            Ball = newBall;
        }

        public SideInfo GetSide(Side side)
        {
            SideInfo si = new SideInfo
            {
                currentBall = Ball,
            };
            Robot[] home = null, opp = null;
            if (side == Side.Blue)
            {
                (home, opp) = (BlueRobots, YellowRobots);
            }
            else if (side == Side.Yellow)
            {
                (home, opp) = (YellowRobots, BlueRobots);
            }
            si.home = (Robot[])home.Clone();
            si.opp = (from rb in opp
                      select new OpponentRobot { pos = rb.pos, rotation = rb.rotation }).ToArray();
            if (side == Side.Yellow) si.ConvertToOtherSide();
            si.TickMatch = TickMatch;
            si.TickRound = TickRound;
            return si;
        }
    }

    public struct MatchScore
    {
        public int BlueScore;
        public int YellowScore;
    }

    public class SideInfo
    {
        public Robot[] home = new Robot[Const.RobotsPerTeam];
        public OpponentRobot[] opp = new OpponentRobot[Const.RobotsPerTeam];
        public Ball currentBall;
        public int TickMatch;
        public int TickRound;

        public SideInfo() { }

        public void ConvertToOtherSide()
        {
            float ht = Const.Field.Right + Const.Field.Left;
            float vt = Const.Field.Bottom + Const.Field.Top;

            currentBall.pos.x = ht - currentBall.pos.x;
            currentBall.pos.y = vt - currentBall.pos.y;
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                home[i].pos.x = ht - home[i].pos.x;
                home[i].pos.y = vt - home[i].pos.y;
                if (home[i].rotation > 0)
                {
                    home[i].rotation = home[i].rotation - 180;
                }
                else if (home[i].rotation <= 0)
                {
                    home[i].rotation = home[i].rotation + 180;
                }

                opp[i].pos.x = ht - opp[i].pos.x;
                opp[i].pos.y = vt - opp[i].pos.y;
                if (opp[i].rotation > 0)
                {
                    opp[i].rotation = opp[i].rotation - 180;
                }
                else if (opp[i].rotation <= 0)
                {
                    opp[i].rotation = opp[i].rotation + 180;
                }
            }
        }
    }

    public class PlacementInfo
    {
        public Robot[] Robots = new Robot[Const.RobotsPerTeam];
        public Ball Ball = new Ball();

        public void Normalize()
        {
            // TODO 保证不会出界、不会重叠
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                Robots[i].Normalize(
                    Const.Field.Right - 10 * i + 1,
                    Const.Field.Left - 10 * i + 1,
                    Const.Field.Top - 10 * i + 1,
                    Const.Field.Bottom - 10 * i + 1
                    );
            }
            Ball.Normalize();
        }

        public void ConvertToOtherSide()
        {
            float ht = Const.Field.Right + Const.Field.Left;
            float vt = Const.Field.Bottom + Const.Field.Top;

            Ball.pos.x = ht - Ball.pos.x;
            Ball.pos.y = vt - Ball.pos.y;
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                Robots[i].pos.x = ht - Robots[i].pos.x;
                Robots[i].pos.y = vt - Robots[i].pos.y;
                if (Robots[i].rotation > 0)
                {
                    Robots[i].rotation = Robots[i].rotation - 180;
                }
                else if (Robots[i].rotation <= 0)
                {
                    Robots[i].rotation = Robots[i].rotation + 180;
                }
            }
        }
    }

    [Serializable]
    public class WheelInfo
    {
        public Wheel[] Wheels = new Wheel[Const.RobotsPerTeam];

        public WheelInfo() { }

        public void Normalize()
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                Wheels[i].Normalize();
            }
        }
    }

    public struct Vector2D
    {
        public float x;
        public float y;

        public static Vector2D Zero => new Vector2D();

        public Vector3 GetUnityVector3()
        {
            return new Vector3 { x = x, z = y };
        }

        public Vector2 GetUnityVector2()
        {
            return new Vector2 { x = x, y = y };
        }

        public void ClampToRect(float right, float left, float top, float bottom)
        {
            // normalize to the specified box
            x = Utils.Clamp(x, left, right);
            y = Utils.Clamp(y, bottom, top);
        }

        public void ClampToField()
        {
            ClampToRect(Const.Field.Right, Const.Field.Left, Const.Field.Top, Const.Field.Bottom);
        }
    }

    [Serializable]
    public struct Wheel
    {
        public float left;
        public float right;

        public void Normalize()
        {
            left = Utils.Clamp(left, Const.MinWheelVelocity, Const.MaxWheelVelocity);
            right = Utils.Clamp(right, Const.MinWheelVelocity, Const.MaxWheelVelocity);
        }
    }

    public struct Robot
    {
        public float mass;
        public Vector2D pos;
        public float rotation;
        public Wheel wheel;
        public Vector2D linearVelocity;
        public float angularVelocity;

        public void Normalize(float right, float left, float top, float bottom)
        {
            pos.ClampToRect(right, left, top, bottom);
            wheel.Normalize();
        }

        public void Normalize()
        {
            pos.ClampToField();
            wheel.Normalize();
        }

        public Vector3 GetLinearVelocityVector3() { return linearVelocity.GetUnityVector3(); }
        public Vector3 GetAngularVelocityVector3() { return new Vector3 { y = angularVelocity }; }

        public void UpdateFromRigidbody(Rigidbody rb)
        {
            this.mass = rb.mass;
            this.pos.x = rb.position.x;
            this.pos.y = rb.position.z;
            this.rotation = rb.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
            this.linearVelocity.x = rb.velocity.x;
            this.linearVelocity.y = rb.velocity.z;
            this.angularVelocity = rb.angularVelocity.y;
        }
    }

    public struct OpponentRobot
    {
        public Vector2D pos;
        public float rotation;
    }

    public struct Ball
    {
        public enum WhichDoor
        {
            BlueDoor,
            YellowDoor,
            None,
        }

        public float mass;
        public Vector2D pos;
        public Vector2D linearVelocity;
        public float angularVelocity;

        public void moveTo(float x, float y)
        {
            pos.x = x;
            pos.y = y;
        }

        public WhichDoor IsInDoor()
        {
            if (pos.x > Const.Field.Right)
            {
                return WhichDoor.BlueDoor;
            }
            else if (pos.x < Const.Field.Left)
            {
                return WhichDoor.YellowDoor;
            }
            else
            {
                return WhichDoor.None;
            }
        }

        public void Normalize(float right, float left, float top, float bottom)
        {
            pos.ClampToRect(right, left, top, bottom);
        }

        public void Normalize()
        {
            pos.ClampToField();
        }

        public Vector3 GetLinearVelocityVector3() { return linearVelocity.GetUnityVector3(); }
        public Vector3 GetAngularVelocityVector3() { return new Vector3 { y = angularVelocity }; }
    }

    public class TeamInfo
    {
        public string Name { get; set; }
    }

    public static class Extended
    {
        public static Side ToAnother(this Side side)
        {
            switch (side)
            {
                case Side.Blue:
                    return Side.Yellow;
                case Side.Yellow:
                    return Side.Blue;
                default:
                    return Side.Nobody;
            }
        }
    }
}

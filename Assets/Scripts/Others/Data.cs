using Newtonsoft.Json;
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
        None,
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
    public class MatchInfo
    {
        public Robot[] BlueRobots { get; set; }
        public Robot[] YellowRobots { get; set; }
        public Ball Ball;

        public int PlayTime;

        public GameState GameState;
        public Side WhosBall;

        public MatchScore Score;
        public ControlState ControlState;
        public Referee Referee;

        public MatchInfo()
        {
            BlueRobots = new Robot[Const.RobotsPerTeam];
            YellowRobots = new Robot[Const.RobotsPerTeam];
            ControlState = ControlState.DefaultState;
            Referee = new Referee();
        }

        public MatchInfo(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {
            BlueRobots = new Robot[Const.RobotsPerTeam];
            YellowRobots = new Robot[Const.RobotsPerTeam];
            ControlState = ControlState.DefaultState;
            Referee = new Referee();

            UpdateEntity(ball, blue, yellow);
        }

        public MatchInfo(MatchInfo another)
        {
            Ball = another.Ball;
            GameState = another.GameState;
            WhosBall = another.WhosBall;
            PlayTime = another.PlayTime;
            Score = another.Score;
            ControlState = another.ControlState;
            Referee = another.Referee;
            BlueRobots = (Robot[])another.BlueRobots.Clone();
            YellowRobots = (Robot[])another.YellowRobots.Clone();
        }

        public static MatchInfo newDefaultPreset()
        {
            var info = new MatchInfo();
            info.Ball.moveTo(0, 0);
            info.Ball.angularVelocity = 0;
            info.Ball.linearVelocity = Vector2D.Zero;
            //x, y, rotation
            var yellowData = new double[,]
            { { -102.5, 0, 90 }, { -81.2, 48, 0 }, { -81.2, -48, 0 }, { -29.8, 48, 0 }, { -29.8, -48, 0 } };
            var blueData = new double[,]
            { { 102.5, 0, -90 }, { 81.2, -48, 180 }, { 81.2, 48, 180 }, { 29.8, -48, 180 }, { 29.8, 48, 180 } };
            //C# 7.0 unavailable now
            Func<Robot, double[,], int, Robot> InitRobot = (rb, data, elem) =>
                {
                    rb.pos.x = (float)data[elem, 0];
                    rb.pos.y = (float)data[elem, 1];
                    rb.rotation = data[elem, 2];
                    rb.velocityLeft = rb.velocityRight = 0;
                    rb.linearVelocity = Vector2D.Zero;
                    return rb;
                };
            Func<IEnumerable<Robot>, double[,], Robot[]> InitMe = (rbs, data) =>
                {
                    return rbs.Select((rb, i) => InitRobot(rb, data, i)).ToArray();
                };
            info.YellowRobots = InitMe(info.YellowRobots, yellowData);
            info.BlueRobots = InitMe(info.BlueRobots, blueData);
            return info;
        }

        public void Update(MatchInfo matchInfo)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                BlueRobots[i] = matchInfo.BlueRobots[i];
                YellowRobots[i] = matchInfo.YellowRobots[i];
            }
            Ball = matchInfo.Ball;
            PlayTime = matchInfo.PlayTime;
            GameState = matchInfo.GameState;
            WhosBall = matchInfo.WhosBall;
            Score = matchInfo.Score;
            ControlState = matchInfo.ControlState;
            Referee = matchInfo.Referee;
        }

        public void UpdateEntity(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                Rigidbody blueTemp = blue[i].GetComponent<Rigidbody>();
                BlueRobots[i].mass = blueTemp.mass;
                BlueRobots[i].pos.x = blueTemp.position.x;
                BlueRobots[i].pos.y = blueTemp.position.z;
                BlueRobots[i].rotation = blueTemp.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
                BlueRobots[i].linearVelocity.x = blueTemp.velocity.x;
                BlueRobots[i].linearVelocity.y = blueTemp.velocity.z;
                BlueRobots[i].angularVelocity = blueTemp.angularVelocity.y;
                //BlueRobot[i].angularVelocity.x = blueTemp.angularVelocity.x;
                //BlueRobot[i].angularVelocity.y = blueTemp.angularVelocity.z;
                //BlueRobot[i].angularVelocity.z = blueTemp.angularVelocity.y;

                Rigidbody yellowTemp = yellow[i].GetComponent<Rigidbody>();
                YellowRobots[i].mass = yellowTemp.mass;
                YellowRobots[i].pos.x = yellowTemp.position.x;
                YellowRobots[i].pos.y = yellowTemp.position.z;
                YellowRobots[i].rotation = yellowTemp.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
                YellowRobots[i].linearVelocity.x = yellowTemp.velocity.x;
                YellowRobots[i].linearVelocity.y = yellowTemp.velocity.z;
                YellowRobots[i].angularVelocity = yellowTemp.angularVelocity.y;
                //YellowRobot[i].angularVelocity.x = yellowTemp.angularVelocity.x;
                //YellowRobot[i].angularVelocity.y = yellowTemp.angularVelocity.z;
                //YellowRobot[i].angularVelocity.z = yellowTemp.angularVelocity.y;
            }

            Rigidbody ballTemp = ball.GetComponent<Rigidbody>();
            Ball newBall = new Ball
            {
                mass = ballTemp.mass,
                pos = {x = ballTemp.position.x, y = ballTemp.position.z},
                linearVelocity = {x = ballTemp.velocity.x, y = ballTemp.velocity.z},
                angularVelocity = ballTemp.angularVelocity.y
            };
            //newBall.angularVelocity.x = ballTemp.angularVelocity.x;
            //newBall.angularVelocity.y = ballTemp.angularVelocity.z;
            //newBall.angularVelocity.z = ballTemp.angularVelocity.y;
            Ball = newBall;
        }

        public void UpdateState(GameState gameState, Side whosBall)
        {
            GameState = gameState;
            if (gameState != 0)
            {
                WhosBall = whosBall;
            }
        }

        public SideInfo GetSide(Side side)
        {
            SideInfo si = new SideInfo
            {
                currentBall = Ball,
                whosBall = (int)WhosBall,
                gameState = (int)GameState
            };
            Robot[] home = null, opp = null;
            if (side == Side.Blue)
            {
                (home, opp) = (BlueRobots, YellowRobots);
            }
            else if(side==Side.Yellow)
            {
                (home, opp) = (YellowRobots, BlueRobots);
            }
            si.home = (Robot[])home.Clone();
            si.opp = (from rb in opp
                      select new OpponentRobot { pos = rb.pos, rotation = rb.rotation }).ToArray();
            if (side == Side.Yellow) si.ConvertToOtherSide();
            return si;
        }
    }

    public struct ControlState
    {
        // 比赛已经开始
        public bool StartedMatch { get; set; }
        // 回合已经开始
        public bool InRound { get; set; }
        // 回合已经暂停
        public bool PausedRound { get; set; }
        // 在摆位中
        public bool InPlacement { get; set; }

        public void Reset()
        {
            StartedMatch = false;
            //LoadSucceed = false;
            InRound = false;
            PausedRound = true;
            InPlacement = false;
        }

        public static ControlState DefaultState =>
            new ControlState
            {

                StartedMatch = false,
                InRound = false,
                PausedRound = true,
                InPlacement = false,
            };
    }

    public struct MatchScore
    {
        public int BlueScore;
        public int YellowScore;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SideInfo
    {
        [JsonProperty("home")]
        public Robot[] home = new Robot[Const.RobotsPerTeam];
        [JsonProperty("opp")]
        public OpponentRobot[] opp = new OpponentRobot[Const.RobotsPerTeam];
        [JsonProperty("currentBall")]
        public Ball currentBall;
        [JsonProperty("gameState")]
        public int gameState;
        [JsonProperty("whosBall")]
        public int whosBall;

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

    [JsonObject(MemberSerialization.OptIn)]
    public class PlacementInfo
    {
        [JsonProperty("robot")]
        public Robot[] Robot = new Robot[Const.RobotsPerTeam];
        [JsonProperty("ball")]
        public Ball Ball = new Ball();

        public void Normalize()
        {
            // TODO 保证不会出界、不会重叠
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                Robot[i].Normalize(
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
                Robot[i].pos.x = ht - Robot[i].pos.x;
                Robot[i].pos.y = vt - Robot[i].pos.y;
                if (Robot[i].rotation > 0)
                {
                    Robot[i].rotation = Robot[i].rotation - 180;
                }
                else if (Robot[i].rotation <= 0)
                {
                    Robot[i].rotation = Robot[i].rotation + 180;
                }
            }
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class WheelInfo
    {
        [JsonProperty("wheels")]
        public Wheel[] Wheels = new Wheel[Const.RobotsPerTeam];

        public WheelInfo() { }

        public void Normalize()
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                Wheels[i].Normalize();
                //w.left *= Const.inch2cm;
                //w.right *= Const.inch2cm;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector2D
    {
        [JsonProperty("x")]
        public float x;
        [JsonProperty("y")]
        public float y;
        public float rotation
        {
            get
            {
                return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            }
        }// 根据x,y坐标计算的相对原点的方向

        public static Vector2D Zero => new Vector2D();

        public Vector3 GetUnityVector3()
        {
            return new Vector3 { x = x, z = y };
        }

        public Vector2 GetUnityVector2()
        {
            return new Vector2 { x = x, y = y };
        }

        public void NormalizeAsPosition(float right, float left, float top, float bottom)
        {
            // normalize to the specified box
            if (x > right)
            {
                x = right;
            }
            else if (x < left)
            {
                x = left;
            }
            if (y > top)
            {
                y = top;
            }
            else if (y < bottom)
            {
                y = bottom;
            }
            
        }

        public void NormalizeAsPosition()
        {
            NormalizeAsPosition(Const.Field.Right, Const.Field.Left, Const.Field.Top, Const.Field.Bottom);
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public struct Wheel
    {
        [JsonProperty("velocityLeft")]
        public double left;
        [JsonProperty("velocityRight")]
        public double right;

        public void Normalize()
        {
            if (left > Const.MaxWheelVelocity) { left = Const.MaxWheelVelocity; }
            if (right > Const.MaxWheelVelocity) { right = Const.MaxWheelVelocity; }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Robot
    {
        public float mass;
        [JsonProperty("pos")]
        public Vector2D pos;
        [JsonProperty("rotation")]
        public double rotation;
        [JsonProperty("velocityLeft")]
        public double velocityLeft;
        [JsonProperty("velocityRight")]
        public double velocityRight;
        public Vector2D linearVelocity;
        public float angularVelocity;

        public void Normalize(float right, float left, float top, float bottom)
        {
            pos.NormalizeAsPosition(right, left, top, bottom);
            var wv = new Wheel { left = velocityLeft, right = velocityRight };
            wv.Normalize();
            velocityLeft = wv.left;
            velocityRight = wv.right;
        }

        public void Normalize()
        {
            pos.NormalizeAsPosition();
            var wv = new Wheel { left = velocityLeft, right = velocityRight };
            wv.Normalize();
            velocityLeft = wv.left;
            velocityRight = wv.right;
        }

        public Vector3 GetLinearVelocityVector3() { return linearVelocity.GetUnityVector3(); }
        public Vector3 GetAngularVelocityVector3() { return new Vector3 { y = angularVelocity }; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct OpponentRobot
    {
        [JsonProperty("pos")]
        public Vector2D pos;
        [JsonProperty("rotation")]
        public double rotation;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Ball
    {
        public enum WhichDoor
        {
            BlueDoor,
            YellowDoor,
            None,
        }

        public float mass;
        [JsonProperty("pos")]
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
            pos.NormalizeAsPosition(right, left, top, bottom);
        }

        public void Normalize()
        {
            pos.NormalizeAsPosition();
        }

        public Vector3 GetLinearVelocityVector3() { return linearVelocity.GetUnityVector3(); }
        public Vector3 GetAngularVelocityVector3() { return new Vector3 { y = angularVelocity }; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class TeamInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

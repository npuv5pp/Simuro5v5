using Newtonsoft.Json;
using UnityEngine;
using Simuro5v5.Config;
using Simuro5v5.Strategy;
using System;

namespace Simuro5v5
{
    public enum GameState
    {
        NormalMatch = 0,
        FreeBallTop = 1,
        FreeBallBottom = 2,
        PlaceKick = 3,
        Plenalty = 4,
        GoalKick = 5,
    }

    public enum Side
    {
        Yellow,
        Blue,
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
        public Robot[] BlueRobot { get; set; }
        public Robot[] YellowRobot { get; set; }
        public Ball Ball;

        public int PlayTime;

        public GameState GameState;
        public Side WhosBall;

        public MatchScore Score;
        public ControlState ControlState;
        public Referee Referee;

        public MatchInfo()
        {
            BlueRobot = new Robot[5];
            YellowRobot = new Robot[5];
            ControlState = ControlState.DefaultState;
            Referee = new Referee();
        }

        public MatchInfo(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {
            BlueRobot = new Robot[5];
            YellowRobot = new Robot[5];
            ControlState = ControlState.DefaultState;
            Referee = new Referee();

            UpdateEntity(ball, blue, yellow);
        }

        public static MatchInfo DefaultMatch
        {
            get
            {
                var rv = new MatchInfo();

                rv.Ball.pos.x = 0;
                rv.Ball.pos.y = 0;
                rv.Ball.angularVelocity = 0;
                rv.Ball.linearVelocity = Vector2D.zero;

                rv.BlueRobot[0].pos.x = 102.5F;
                rv.BlueRobot[0].pos.y = 0;
                rv.BlueRobot[0].rotation = -90;
                rv.BlueRobot[1].pos.x = 81.2F;
                rv.BlueRobot[1].pos.y = -48F;
                rv.BlueRobot[1].rotation = 180;
                rv.BlueRobot[2].pos.x = 81.2F;
                rv.BlueRobot[2].pos.y = 48F;
                rv.BlueRobot[2].rotation = 180;
                rv.BlueRobot[3].pos.x = 29.8F;
                rv.BlueRobot[3].pos.y = -48F;
                rv.BlueRobot[3].rotation = 180;
                rv.BlueRobot[4].pos.x = 29.8F;
                rv.BlueRobot[4].pos.y = 48F;
                rv.BlueRobot[4].rotation = 180;
                for (int i = 0; i < 5; i++)
                {
                    rv.BlueRobot[i].velocityLeft = rv.BlueRobot[i].velocityRight = 0;
                    rv.BlueRobot[i].linearVelocity = Vector2D.zero;
                }

                rv.YellowRobot[0].pos.x = -102.5F;
                rv.YellowRobot[0].pos.y = 0;
                rv.YellowRobot[0].rotation = 90;
                rv.YellowRobot[1].pos.x = -81.2F;
                rv.YellowRobot[1].pos.y = 48F;
                rv.YellowRobot[1].rotation = 0;
                rv.YellowRobot[2].pos.x = -81.2F;
                rv.YellowRobot[2].pos.y = -48F;
                rv.YellowRobot[2].rotation = 0;
                rv.YellowRobot[3].pos.x = -29.8F;
                rv.YellowRobot[3].pos.y = 48F;
                rv.YellowRobot[3].rotation = 0;
                rv.YellowRobot[4].pos.x = -29.8F;
                rv.YellowRobot[4].pos.y = -48F;
                rv.YellowRobot[4].rotation = 0;
                for (int i = 0; i < 5; i++)
                {
                    rv.YellowRobot[i].velocityLeft = rv.YellowRobot[i].velocityRight = 0;
                    rv.YellowRobot[i].linearVelocity = Vector2D.zero;
                }

                return rv;
            }
        }

        public MatchInfo Clone()
        {
            MatchInfo rv = new MatchInfo
            {
                Ball = Ball,
                GameState = GameState,
                WhosBall = WhosBall,
                PlayTime = PlayTime,
                Score = Score,
                ControlState = ControlState,
                Referee = Referee,
            };
            for (int i = 0; i < 5; i++)
            {
                rv.BlueRobot[i] = BlueRobot[i];
                rv.YellowRobot[i] = YellowRobot[i];
            }
            return rv;
        }
        
        public void Update(MatchInfo matchInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                BlueRobot[i] = matchInfo.BlueRobot[i];
                YellowRobot[i] = matchInfo.YellowRobot[i];
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
            for (int i = 0; i < 5; i++)
            {
                Rigidbody blueTemp = blue[i].GetComponent<Rigidbody>();
                BlueRobot[i].mass = blueTemp.mass;
                BlueRobot[i].pos.x = blueTemp.position.x;
                BlueRobot[i].pos.y = blueTemp.position.z;
                BlueRobot[i].rotation = blueTemp.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
                BlueRobot[i].linearVelocity.x = blueTemp.velocity.x;
                BlueRobot[i].linearVelocity.y = blueTemp.velocity.z;
                BlueRobot[i].angularVelocity = blueTemp.angularVelocity.y;
                //BlueRobot[i].angularVelocity.x = blueTemp.angularVelocity.x;
                //BlueRobot[i].angularVelocity.y = blueTemp.angularVelocity.z;
                //BlueRobot[i].angularVelocity.z = blueTemp.angularVelocity.y;

                Rigidbody yellowTemp = yellow[i].GetComponent<Rigidbody>();
                YellowRobot[i].mass = yellowTemp.mass;
                YellowRobot[i].pos.x = yellowTemp.position.x;
                YellowRobot[i].pos.y = yellowTemp.position.z;
                YellowRobot[i].rotation = yellowTemp.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
                YellowRobot[i].linearVelocity.x = yellowTemp.velocity.x;
                YellowRobot[i].linearVelocity.y = yellowTemp.velocity.z;
                YellowRobot[i].angularVelocity = yellowTemp.angularVelocity.y;
                //YellowRobot[i].angularVelocity.x = yellowTemp.angularVelocity.x;
                //YellowRobot[i].angularVelocity.y = yellowTemp.angularVelocity.z;
                //YellowRobot[i].angularVelocity.z = yellowTemp.angularVelocity.y;
            }

            Rigidbody ballTemp = ball.GetComponent<Rigidbody>();
            Ball newBall = new Ball();
            newBall.mass = ballTemp.mass;
            newBall.pos.x = ballTemp.position.x;
            newBall.pos.y = ballTemp.position.z;
            newBall.linearVelocity.x = ballTemp.velocity.x;
            newBall.linearVelocity.y = ballTemp.velocity.z;
            newBall.angularVelocity = ballTemp.angularVelocity.y;
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

        public SideInfo GetBlueSide()
        {
            SideInfo rv = new SideInfo {
                currentBall = Ball,
                whosBall = (int)WhosBall,
                gameState = (int)GameState
            };
            for (int i = 0; i < 5; i++)
            {
                rv.home[i] = BlueRobot[i];
                rv.opp[i] = new OpponentRobot {
                    pos = YellowRobot[i].pos,
                    rotation = YellowRobot[i].rotation
                };
            }
            return rv;
        }

        public SideInfo GetYellowSide()
        {
            SideInfo rv = new SideInfo {
                currentBall = Ball,
                whosBall = (int)WhosBall,
                gameState = (int)GameState
            };
            for (int i = 0; i < 5; i++)
            {
                rv.home[i] = YellowRobot[i];
                rv.opp[i] = new OpponentRobot {
                    pos = BlueRobot[i].pos,
                    rotation = BlueRobot[i].rotation
                };
            }
            // 转换坐标
            if (GeneralConfig.EnableConvertYellowData)
            {
                rv.ConvertToOtherSide();
            }
            return rv;
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

        public static ControlState DefaultState
        {
            get
            {
                return new ControlState
                {

                    StartedMatch = false,
                    InRound = false,
                    PausedRound = true,
                    InPlacement = false,
                };
            }
        }
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
        public Robot[] home = new Robot[5];
        [JsonProperty("opp")]
        public OpponentRobot[] opp = new OpponentRobot[5];
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
            for (int i = 0; i < 5; i++)
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
        public Robot[] Robot = new Robot[5];
        [JsonProperty("ball")]
        public Ball Ball = new Ball();

        public void Normalize()
        {
            // TODO 保证不会出界、不会重叠
            for (int i = 0; i < 5; i++)
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
            for (int i = 0; i < 5; i++)
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
        public Wheel[] Wheels = new Wheel[5];

        public WheelInfo() { }

        public void Normalize()
        {
            for (int i = 0; i < 5; i++)
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

        public static Vector2D zero { get { return new Vector2D(); } }

        public Vector3 GetUnityVector3()
        {
            return new Vector3 { x = x, z = y };
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
            if (left > Const.max_vec) { left = Const.max_vec; }
            if (right > Const.max_vec) { right = Const.max_vec; }
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
    public class Teaminfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

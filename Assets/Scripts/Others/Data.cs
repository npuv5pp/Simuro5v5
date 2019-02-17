using Newtonsoft.Json;
using UnityEngine;
using Simuro5v5.Config;
using Simuro5v5.Strategy;

namespace Simuro5v5
{
    public enum GameState
    {
        NormalMatch,
        FreeBallTop,
        FreeBallBottom,
        PlaceKick,
        Plenalty,
        GoalKick,
    }

    public enum Side
    {
        DefensiveSide,
        OffensiveSide,
    };

    public class MatchInfo
    {
        public Robot[] BlueRobot { get; private set; }
        public Robot[] YellowRobot { get; private set; }
        public Ball CurrentBall { get; private set; }
        public int GameState { get; private set; }
        public int WhosBall { get; private set; }

        public int PlayTime { get; set; }
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
            UpdateEntity(ball, blue, yellow);
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
                BlueRobot[i].angularVelocity.x = blueTemp.angularVelocity.x;
                BlueRobot[i].angularVelocity.y = blueTemp.angularVelocity.z;
                BlueRobot[i].angularVelocity.z = blueTemp.angularVelocity.y;

                Rigidbody yellowTemp = yellow[i].GetComponent<Rigidbody>();
                YellowRobot[i].mass = yellowTemp.mass;
                YellowRobot[i].pos.x = yellowTemp.position.x;
                YellowRobot[i].pos.y = yellowTemp.position.z;
                YellowRobot[i].rotation = yellowTemp.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
                YellowRobot[i].linearVelocity.x = yellowTemp.velocity.x;
                YellowRobot[i].linearVelocity.y = yellowTemp.velocity.z;
                YellowRobot[i].angularVelocity.x = yellowTemp.angularVelocity.x;
                YellowRobot[i].angularVelocity.y = yellowTemp.angularVelocity.z;
                YellowRobot[i].angularVelocity.z = yellowTemp.angularVelocity.y;
            }

            Rigidbody ballTemp = ball.GetComponent<Rigidbody>();
            Ball newBall = new Ball();
            newBall.mass = ballTemp.mass;
            newBall.pos.x = ballTemp.position.x;
            newBall.pos.y = ballTemp.position.z;
            newBall.linearVelocity.x = ballTemp.velocity.x;
            newBall.linearVelocity.y = ballTemp.velocity.z;
            newBall.angularVelocity.x = ballTemp.angularVelocity.x;
            newBall.angularVelocity.y = ballTemp.angularVelocity.z;
            newBall.angularVelocity.z = ballTemp.angularVelocity.y;
            CurrentBall = newBall;
        }

        public void UpdateState(GameState gameState, Side whosBall)
        {
            GameState = (int)gameState;
            if (gameState != 0)
            {
                WhosBall = (int)whosBall;
            }
        }

        public MatchInfo Clone()
        {
            MatchInfo rv = new MatchInfo
            {
                CurrentBall = CurrentBall,
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

        public SideInfo GetBlueSide()
        {
            SideInfo rv = new SideInfo {
                currentBall = CurrentBall,
                whosBall = WhosBall,
                gameState = GameState
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
                currentBall = CurrentBall,
                whosBall = WhosBall,
                gameState = GameState
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
                rv.ConvertToOtherSide();
            return rv;
        }
    }

    public struct ControlState
    {
        // 比赛已经开始
        public bool StartedMatch { get; set; }
        // 策略已经加载成功
        public bool LoadSucceed { get; set; }
        // 回合已经开始
        public bool InRound { get; set; }
        // 回合已经暂停
        public bool PausedRound { get; set; }
        // 在摆位中
        public bool InPlacement { get; set; }

        public static ControlState DefaultState
        {
            get
            {
                return new ControlState
                {

                    StartedMatch = false,
                    LoadSucceed = false,
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
        
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PlacementInfo
    {
        [JsonProperty("robot")]
        public Robot[] Robot = new Robot[5];
        [JsonProperty("ball")]
        public Ball Ball = new Ball();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class WheelInfo
    {
        [JsonProperty("wheels")]
        public Wheel[] Wheels = new Wheel[5];

        public WheelInfo() { }

        public void Normalize()
        {
            Wheel w;
            for (int i = 0; i < 5; i++)
            {
                w = Wheels[i];
                if (w.left > Const.max_vec) { w.left = Const.max_vec; }
                if (w.right > Const.max_vec) { w.right = Const.max_vec; }
                w.left *= Const.inch2cm;
                w.right *= Const.inch2cm;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Vector3D
    {
        [JsonProperty("x")]
        public float x;
        [JsonProperty("y")]
        public float y;
        public float z;
        public float rotation { get { return Mathf.Atan2(y, x) * Mathf.Rad2Deg; } }// 根据x,y坐标计算的相对原点的方向

        public Vector3 GetUnityVector3()
        {
            return new Vector3
            {
                x = x,
                z = y,
                y = z
            };
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Wheel
    {
        [JsonProperty("velocityLeft")]
        public double left;
        [JsonProperty("velocityRight")]
        public double right;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Robot
    {
        public float mass;
        [JsonProperty("pos")]
        public Vector3D pos;
        [JsonProperty("rotation")]
        public double rotation;
        [JsonProperty("velocityLeft")]
        public double velocityLeft;
        [JsonProperty("velocityRight")]
        public double velocityRight;
        public Vector3D linearVelocity;
        public Vector3D angularVelocity;  
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct OpponentRobot
    {
        [JsonProperty("pos")]
        public Vector3D pos;
        [JsonProperty("rotation")]
        public double rotation;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public struct Ball
    {
        public float mass;
        [JsonProperty("pos")]
        public Vector3D pos;
        public Vector3D linearVelocity;
        public Vector3D angularVelocity;
    }
}

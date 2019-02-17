using UnityEngine;
using UnityEngine.SceneManagement;

namespace Simuro5v5
{
    using Simuro5v5.EventSystem;

    public static class ObjectManager
    {
        public static GameObject ballObject { get; private set; }
        public static GameObject[] blueObject { get; private set; }
        public static GameObject[] yellowObject { get; private set; }

        public static ControlBall ballComponent { get; private set; }
        public static ControlRobot[] blueComponent { get; private set; }
        public static ControlRobot[] yellowComponent { get; private set; }

        public static MatchInfo OutputMatchInfo { get; private set; }

        public static void RebindMatchInfo(MatchInfo matchInfo)
        {
            OutputMatchInfo = matchInfo;
        }

        public static void RebindObject()
        {
            ballObject = GameObject.Find("Ball");
            blueObject = new GameObject[5] {
                GameObject.Find("Blue0"),
                GameObject.Find("Blue1"),
                GameObject.Find("Blue2"),
                GameObject.Find("Blue3"),
                GameObject.Find("Blue4")
            };
            yellowObject = new GameObject[5] {
                GameObject.Find("Yellow0"),
                GameObject.Find("Yellow1"),
                GameObject.Find("Yellow2"),
                GameObject.Find("Yellow3"),
                GameObject.Find("Yellow4"),
            };

            ballComponent = ballObject.GetComponent<ControlBall>();
            blueComponent = new ControlRobot[5] {
                blueObject[0].GetComponent<ControlRobot>(),
                blueObject[1].GetComponent<ControlRobot>(),
                blueObject[2].GetComponent<ControlRobot>(),
                blueObject[3].GetComponent<ControlRobot>(),
                blueObject[4].GetComponent<ControlRobot>(),
            };
            yellowComponent = new ControlRobot[5] {
                yellowObject[0].GetComponent<ControlRobot>(),
                yellowObject[1].GetComponent<ControlRobot>(),
                yellowObject[2].GetComponent<ControlRobot>(),
                yellowObject[3].GetComponent<ControlRobot>(),
                yellowObject[4].GetComponent<ControlRobot>(),
            };
        }

        public static void SetBlueWheelInfo(WheelInfo ws)
        {
            for (int i = 0; i < 5; i++)
            {
                OutputMatchInfo.BlueRobot[i].velocityLeft = ws.Wheels[i].left;
                OutputMatchInfo.BlueRobot[i].velocityRight = ws.Wheels[i].right;
                blueComponent[i].SetWheelVelocity(ws.Wheels[i]);
            }
        }

        public static void SetYellowWheelInfo(WheelInfo ws)
        {
            for (int i = 0; i < 5; i++)
            {
                OutputMatchInfo.YellowRobot[i].velocityLeft = ws.Wheels[i].left;
                OutputMatchInfo.YellowRobot[i].velocityRight = ws.Wheels[i].right;
                yellowComponent[i].SetWheelVelocity(ws.Wheels[i]);
            }
        }

        public static void SetBluePlacement(PlacementInfo sInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].SetPlacement(sInfo.Robot[i]);
            }
        }

        public static void SetBluePlacement(Robot[] robots)
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].SetPlacement(robots[i]);
            }
        }

        public static void SetYellowPlacement(PlacementInfo sInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                yellowComponent[i].SetPlacement(sInfo.Robot[i]);
            }
        }

        public static void SetYellowPlacement(Robot[] robots)
        {
            for (int i = 0; i < 5; i++)
            {
                yellowComponent[i].SetPlacement(robots[i]);
            }
        }

        public static void SetBallPlacement(PlacementInfo sInfo)
        {
            ballComponent.SetPlacement(sInfo.Ball);
        }

        public static void SetBallPlacement(Ball ball)
        {
            ballComponent.SetPlacement(ball);
        }

        public static void SetStill()
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].SetStill();
                yellowComponent[i].SetStill();
            }
            ballComponent.SetStill();
        }

        public static void SetDefaultPostion()
        {
            // 这三个结构体是对于策略来说的，使用的策略坐标系
            // 在下面的SetPlacement系列函数中，会自动转为平台坐标系
            Ball ball = new Ball();
            Robot[] blueInfo = new Robot[5];
            Robot[] yellowInfo = new Robot[5];

            ball.pos.x = 0;
            ball.pos.y = 0;
            ball.pos.z = 2.521712F;

            blueInfo[0].pos.x = 102.5F;
            blueInfo[0].pos.y = 0;
            blueInfo[0].pos.z = 0;
            blueInfo[0].rotation = -90;
            blueInfo[1].pos.x = 81.2F;
            blueInfo[1].pos.y = -48F;
            blueInfo[1].pos.z = 0;
            blueInfo[1].rotation = 180;

            blueInfo[2].pos.x = 81.2F;
            blueInfo[2].pos.y = 48F;
            blueInfo[2].pos.z = 0;
            blueInfo[2].rotation = 180;
            blueInfo[3].pos.x = 29.8F;
            blueInfo[3].pos.y = -48F;
            blueInfo[3].pos.z = 0;
            blueInfo[3].rotation = 180;
            blueInfo[4].pos.x = 29.8F;
            blueInfo[4].pos.y = 48F;
            blueInfo[4].pos.z = 0;
            blueInfo[4].rotation = 180;

            yellowInfo[0].pos.x = -102.5F;
            yellowInfo[0].pos.y = 0;
            yellowInfo[0].pos.z = 0;
            yellowInfo[0].rotation = 90;
            yellowInfo[1].pos.x = -81.2F;
            yellowInfo[1].pos.y = 48F;
            yellowInfo[1].pos.z = 0;
            yellowInfo[1].rotation = 0;
            yellowInfo[2].pos.x = -81.2F;
            yellowInfo[2].pos.y = -48F;
            yellowInfo[2].pos.z = 0;
            yellowInfo[2].rotation = 0;
            yellowInfo[3].pos.x = -29.8F;
            yellowInfo[3].pos.y = 48F;
            yellowInfo[3].pos.z = 0;
            yellowInfo[3].rotation = 0;
            yellowInfo[4].pos.x = -29.8F;
            yellowInfo[4].pos.y = -48F;
            yellowInfo[4].pos.z = 0;
            yellowInfo[4].rotation = 0;
            SetBallPlacement(ball);
            SetBluePlacement(blueInfo);
            SetYellowPlacement(yellowInfo);
            SetStill();

            UpdateFromScene();
        }

        /// <summary>
        /// 从指定MatchInfo还原场景
        /// </summary>
        /// <param name="matchInfo"></param>
        public static void RevertScene(MatchInfo matchInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].Revert(matchInfo.BlueRobot[i]);
                yellowComponent[i].Revert(matchInfo.YellowRobot[i]);
                ballComponent.Revert(matchInfo.CurrentBall);
            }
        }

        /// <summary>
        /// 从默认MatchInfo还原场景
        /// </summary>
        public static void RevertScene()
        {
            RevertScene(OutputMatchInfo);
        }

        public static void UpdateFromScene()
        {
            OutputMatchInfo.UpdateEntity(ballObject, blueObject, yellowObject);
        }

        public static void Resume()
        {
            Time.timeScale = Const.TimeScale;
        }

        public static void Pause()
        {
            Time.timeScale = 0.0f;
        }

        public static void RegisterRePlay()
        {
            Event.Register(Event.EventType1.ReplayInfoUpdate, SetReplayInfo);
        }

        public static void SetReplayInfo(object obj)
        {
            MatchInfo matchInfo = obj as MatchInfo;
            if (matchInfo != null)
            {
                Debug.Log("new matchinfo in replaying");
                RevertScene(matchInfo);
            }
            else
            {
                Debug.Log("error matchinfo in replaying");
            }
        }
    }
}

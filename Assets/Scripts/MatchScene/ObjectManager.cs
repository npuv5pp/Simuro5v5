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

        /// <summary>
        /// 重新绑定一个用来输出的MatchInfo
        /// </summary>
        /// <param name="matchInfo"></param>
        public static void RebindMatchInfo(MatchInfo matchInfo)
        {
            OutputMatchInfo = matchInfo;
        }

        /// <summary>
        /// 重新绑定场景中的对象
        /// </summary>
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

        public static void SetToDefault()
        {
            RevertScene(MatchInfo.DefaultMatch);
        }

        public static void SetBlueWheels(WheelInfo ws)
        {
            for (int i = 0; i < 5; i++)
            {
                OutputMatchInfo.BlueRobot[i].velocityLeft = ws.Wheels[i].left;
                OutputMatchInfo.BlueRobot[i].velocityRight = ws.Wheels[i].right;
                blueComponent[i].SetWheelVelocity(ws.Wheels[i]);
            }
        }

        public static void SetYellowWheels(WheelInfo ws)
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
            var b = ball;
            b.Normalize();
            ballComponent.SetPlacement(b);
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
                ballComponent.Revert(matchInfo.Ball);
            }
        }

        /// <summary>
        /// 从默认MatchInfo还原场景
        /// </summary>
        public static void RevertScene()
        {
            RevertScene(OutputMatchInfo);
        }

        /// <summary>
        /// 从场景更新到绑定的MatchInfo中
        /// </summary>
        public static void UpdateFromScene()
        {
            OutputMatchInfo.UpdateEntity(ballObject, blueObject, yellowObject);
        }

        /// <summary>
        /// 继续运行世界
        /// </summary>
        public static void Resume()
        {
            Time.timeScale = Const.TimeScale;
        }

        /// <summary>
        /// 暂停世界
        /// </summary>
        public static void Pause()
        {
            Time.timeScale = 0.0f;
        }

        public static void RegisterReplay()
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

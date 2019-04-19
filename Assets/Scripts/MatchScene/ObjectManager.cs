using System;
using UnityEngine;

namespace Simuro5v5
{
    public class ObjectManager
    {
        public GameObject ballObject { get; private set; }
        public GameObject[] blueObject { get; private set; }
        public GameObject[] yellowObject { get; private set; }

        public ControlBall ballComponent { get; private set; }
        public ControlRobot[] blueComponent { get; private set; }
        public ControlRobot[] yellowComponent { get; private set; }

        public MatchInfo OutputMatchInfo { get; private set; }

        public ObjectManager() { }

        public ObjectManager(MatchInfo matchInfo)
        {
            RebindMatchInfo(matchInfo);
            RebindObject();
        }

        /// <summary>
        /// 重新绑定一个用来输出的MatchInfo
        /// </summary>
        /// <param name="matchInfo"></param>
        public void RebindMatchInfo(MatchInfo matchInfo)
        {
            OutputMatchInfo = matchInfo;
        }

        public void RebindObject(GameObject entity)
        {
            ballObject = entity.transform.Find("Ball").gameObject;
            blueObject = new GameObject[5]
            {
                entity.transform.Find("Robot/Blue/Blue0").gameObject,
                entity.transform.Find("Robot/Blue/Blue1").gameObject,
                entity.transform.Find("Robot/Blue/Blue2").gameObject,
                entity.transform.Find("Robot/Blue/Blue3").gameObject,
                entity.transform.Find("Robot/Blue/Blue4").gameObject
            };
            yellowObject = new GameObject[5]
            {
                entity.transform.Find("Robot/Yellow/Yellow0").gameObject,
                entity.transform.Find("Robot/Yellow/Yellow1").gameObject,
                entity.transform.Find("Robot/Yellow/Yellow2").gameObject,
                entity.transform.Find("Robot/Yellow/Yellow3").gameObject,
                entity.transform.Find("Robot/Yellow/Yellow4").gameObject
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

        /// <summary>
        /// 重新绑定场景中的对象
        /// </summary>
        public void RebindObject()
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

        public void SetToDefault()
        {
            RevertScene(MatchInfo.NewDefaultPreset());
        }

        public void SetBlueWheels(WheelInfo ws)
        {
            for (int i = 0; i < 5; i++)
            {
                if (OutputMatchInfo != null)
                {
                    OutputMatchInfo.BlueRobots[i].velocityLeft = ws.Wheels[i].left;
                    OutputMatchInfo.BlueRobots[i].velocityRight = ws.Wheels[i].right;
                }
                blueComponent[i].SetWheelVelocity(ws.Wheels[i]);
            }
        }

        public void SetYellowWheels(WheelInfo ws)
        {
            for (int i = 0; i < 5; i++)
            {
                if (OutputMatchInfo != null)
                {
                    OutputMatchInfo.YellowRobots[i].velocityLeft = ws.Wheels[i].left;
                    OutputMatchInfo.YellowRobots[i].velocityRight = ws.Wheels[i].right;
                }
                yellowComponent[i].SetWheelVelocity(ws.Wheels[i]);
            }
        }

        public void SetBluePlacement(PlacementInfo sInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].SetPlacement(sInfo.Robots[i]);
            }
        }

        public void SetBluePlacement(Robot[] robots)
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].SetPlacement(robots[i]);
            }
        }

        public void SetYellowPlacement(PlacementInfo sInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                yellowComponent[i].SetPlacement(sInfo.Robots[i]);
            }
        }

        public void SetYellowPlacement(Robot[] robots)
        {
            for (int i = 0; i < 5; i++)
            {
                yellowComponent[i].SetPlacement(robots[i]);
            }
        }

        public void SetBallPlacement(PlacementInfo sInfo)
        {
            ballComponent.SetPlacement(sInfo.Ball);
        }

        public void SetBallPlacement(Ball ball)
        {
            var b = ball;
            b.Normalize();
            ballComponent.SetPlacement(b);
        }

        /// <summary>
        /// 所有物体静止
        /// </summary>
        public void SetStill()
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
        public void RevertScene(MatchInfo matchInfo)
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].Revert(matchInfo.BlueRobots[i]);
                yellowComponent[i].Revert(matchInfo.YellowRobots[i]);
            }
            ballComponent.Revert(matchInfo.Ball);
            OutputMatchInfo?.UpdateFrom(matchInfo);
        }

        /// <summary>
        /// 从场景更新到绑定的MatchInfo中
        /// </summary>
        public void UpdateFromScene()
        {
            OutputMatchInfo?.UpdateFrom(ballObject, blueObject, yellowObject);
        }

        /// <summary>
        /// 启用物理计算
        /// </summary>
        public void EnablePhysics()
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].physicsEnabled = true;
                yellowComponent[i].physicsEnabled = true;
            }
            ballComponent.physicsEnabled = true;
        }

        /// <summary>
        /// 禁用物理计算
        /// </summary>
        public void DisablePhysics()
        {
            for (int i = 0; i < 5; i++)
            {
                blueComponent[i].physicsEnabled = false;
                yellowComponent[i].physicsEnabled = false;
            }
            ballComponent.physicsEnabled = false;
        }

        /// <summary>
        /// 继续运行世界
        /// </summary>
        public void Resume()
        {
            Time.timeScale = Const.TimeScale;
        }

        /// <summary>
        /// 暂停世界
        /// </summary>
        public void Pause()
        {
            Time.timeScale = 0.0f;
        }
    }
}

public class PhysicsDisabledException : Exception
{
    public PhysicsDisabledException() { }
    public PhysicsDisabledException(string message) : base(message) { }
}

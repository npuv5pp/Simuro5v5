using System;
using System.Collections;
using System.Text;
using Assets.Scripts.MatchScene.Referee;
using Newtonsoft.Json;
using Simuro5v5;
using UnityEngine;
using Event = Simuro5v5.EventSystem.Event;

public class Referee : ICloneable
{
    //Class
    private GameObject[] BlueObject => PlayMain.ObjectManager.blueObject;
    private GameObject[] YellowObject => PlayMain.ObjectManager.yellowObject;
    private MatchInfo matchInfo;
    private Robot[] blueRobots;
    private Robot[] yellowRobots;

    private int goalieBlueId;
    private int goalieYellowId;

    /// <summary>
    /// 停滞时间
    /// </summary>
    [JsonProperty]
    private int standoffTime;

    [JsonProperty]
    public JudgeResult lastJudge;

    public Referee()
    {
        standoffTime = 0;
        lastJudge = new JudgeResult
        {
            Actor = Side.None,
            ResultType = ResultType.NormalMatch,
            Reason = "",
        };
    }

    public object Clone()
    {
        return new Referee
        {
            standoffTime = standoffTime,
            lastJudge = lastJudge
        };
    }

    private static readonly Square yellowGoalState = new Square(-125, -110, 20, -20);
    private static readonly Square yellowBigState = new Square(-125, -75, 40, -40);
    private static readonly Square yellowSmallState = new Square(-125, -95, 25, -25);
    private static readonly Square blueGoalState = new Square(110, 125, 20, -20);
    private static readonly Square blueBigState = new Square(75, 125, 40, -40);
    private static readonly Square blueSmallState = new Square(95, 125, 25, -25);

    ///<summary>
    ///判断比赛状态，返回JudgeResult类
    /// </summary>
    public JudgeResult Judge(MatchInfo matchInfo)
    {
        this.matchInfo = matchInfo;
        this.blueRobots = matchInfo.BlueRobots;
        this.yellowRobots = matchInfo.YellowRobots;
        this.goalieBlueId = FindBlueGoalie();
        this.goalieYellowId = FindYellowGoalie();

        var result = CollectJudge();
        lastJudge = result;
        return result;
    }

    private JudgeResult CollectJudge()
    {
        JudgeResult judgeResult = default;

        if (JudgePlace(ref judgeResult))
            return judgeResult;

        if (JudgePenalty(ref judgeResult))
            return judgeResult;

        if (JudgeGoalie(ref judgeResult))
            return judgeResult;

        if (JudgeFree(ref judgeResult))
            return judgeResult;

        return new JudgeResult
        {
            ResultType = ResultType.NormalMatch,
            Actor = Side.None,
            Reason = "Normal competition"
        };
    }

    private int FindBlueGoalie()
    {
        int id = -1;
        // TODO: use foreach loop over blueRobots
        for (int i = 0; i <= 4; i++)
        {
            if (blueSmallState.InSquare(blueRobots[i].pos))
            {
                id = i;
            }
        }
        return id;
    }

    private int FindYellowGoalie()
    {
        int id = -1;
        for (int i = 0; i <= 4; i++)
        {
            if (yellowSmallState.InSquare(yellowRobots[i].pos))
            {
                id = i;
            }
        }
        return id;
    }

    private bool JudgePlace(ref JudgeResult judgeResult)
    {
        if (yellowGoalState.InSquare(matchInfo.Ball.pos))
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Yellow,
                Reason = "Be scored and PlaceKick again",
                ResultType = ResultType.PlaceKick
            };
            Event.Send(Event.EventType1.GetGoal, Side.Blue); //黄方被进球
            return true;
        }

        if (blueGoalState.InSquare(matchInfo.Ball.pos))
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Blue,
                Reason = "Be scored and PlaceKick again",
                ResultType = ResultType.PlaceKick
            };
            Event.Send(Event.EventType1.GetGoal, Side.Yellow); //蓝方被进球
            return true;
        }

        return false;
    }

    private bool JudgePenalty(ref JudgeResult judgeResult)
    {
        if (matchInfo.Ball.pos.x > 0)
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (blueBigState.InSquare(blueRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (blueSmallState.InSquare(blueRobots[i].pos))
                {
                    smallStateNum++;
                }
            }
            if (bigStateNum >= 4)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Actor = Side.Yellow,
                    Reason = "Defenders have four robots in BigState"
                };
                return true;
            }
            if (smallStateNum >= 2)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Actor = Side.Blue,
                    Reason = "Defenders have two robots in SmallState"
                };
                return true;
            }
        }
        else
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (yellowBigState.InSquare(yellowRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (yellowSmallState.InSquare(yellowRobots[i].pos))
                {
                    smallStateNum++;
                }
            }
            if (bigStateNum >= 4)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Actor = Side.Blue,
                    Reason = "Defenders have four robots in BigState"
                };
                return true;
            }
            if (smallStateNum >= 2)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Actor = Side.Blue,
                    Reason = "Defenders have two robots in SmallState"
                };
                return true;
            }
        }
        return false;
    }

    private bool JudgeGoalie(ref JudgeResult judgeResult)
    {
        if (blueBigState.InSquare(matchInfo.Ball.pos))
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (goalieBlueId != -1 && JudgeCollision(BlueObject[goalieBlueId], YellowObject[i]))
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GoalKick,
                        Actor = Side.Blue,
                        Reason = "Attacker hit the Goalie"
                    };
                    return true;
                }
                if (blueBigState.InSquare(yellowRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (blueSmallState.InSquare(yellowRobots[i].pos))
                {
                    smallStateNum++;
                }
            }
            if (bigStateNum >= 4)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.GoalKick,
                    Actor = Side.Blue,
                    Reason = "Attacker have four robots in BigState"
                };
                return true;
            }
            if (smallStateNum >= 2)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.GoalKick,
                    Actor = Side.Blue,
                    Reason = "Attacker have two robots in SmallState"
                };
                return true;
            }
        }
        else if (yellowBigState.InSquare(matchInfo.Ball.pos))
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (goalieYellowId != -1 && JudgeCollision(YellowObject[goalieYellowId], BlueObject[i]))
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GoalKick,
                        Actor = Side.Yellow,
                        Reason = "Attacker hit the Goalie"
                    };
                    return true;
                }
                if (yellowBigState.InSquare(blueRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (yellowSmallState.InSquare(blueRobots[i].pos))
                {
                    smallStateNum++;
                }
            }
            if (bigStateNum >= 4)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.GoalKick,
                    Actor = Side.Yellow,
                    Reason = "Attacker have four robots in BigState"
                };
                return true;
            }
            if (smallStateNum >= 2)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.GoalKick,
                    Actor = Side.Yellow,
                    Reason = "Attacker have two robots in SmallState"
                };
                return true;
            }
        }

        return false;

    }
    private bool JudgeFree(ref JudgeResult judgeResult)
    {
        if (matchInfo.Ball.linearVelocity.GetUnityVector2().magnitude < 5)
        {
            standoffTime++;
            if (standoffTime > 500)
            {
                if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKick,
                        Actor = Side.Blue,
                        Reason = "RightTop Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
                else if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y < 0)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKick,
                        Actor = Side.Blue,
                        Reason = "RightBot Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
                else if (matchInfo.Ball.pos.x < 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKick,
                        Actor = Side.Yellow,
                        Reason = "LeftTop Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
                else
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKick,
                        Actor = Side.Yellow,
                        Reason = "LeftBot Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            standoffTime = 0;
            return false;
        }

    }

    /// <summary>
    /// whether Object1 collide Object2
    /// </summary>
    /// <param name="object1"></param>
    /// <param name="object2"></param>
    /// <returns></returns>
    private bool JudgeCollision(GameObject object1, GameObject object2)
    {
        ArrayList touchObject = object1.GetComponent<BoxColliderEvent>().TouchObject;
        if (touchObject.IndexOf(object2) == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


}

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

    private int blueScore;
    private int yellowScore;

    /// <summary>
    /// 上下半场游戏比赛时间 5分钟
    /// </summary>
    private int endOfHalfgametime;

    /// <summary>
    /// 加时赛游戏比赛时间 3分钟
    /// </summary>
    private int endOfOvergametime;

    /// <summary>
    /// 点球大战中点球方，依次交换顺序
    /// </summary>
    private Side penaltySide;

    /// <summary>
    /// 点球大战中每次罚球点球限制时间 5秒
    /// </summary>
    private int penaltyLimitTime;

    /// <summary>
    /// 点球大战中所执行的时间 
    /// </summary>
    private int penaltyTime;
    
    /// <summary>
    /// 记录点球是否在5次内
    /// </summary>
    private int penaltyOfNum;

    /// <summary>
    /// 停滞时间
    /// </summary>
    [JsonProperty]
    private int standoffTime;

    [JsonProperty]
    public JudgeResult lastJudge;

    public Referee()
    {
        endOfHalfgametime = 15000;
        endOfOvergametime = 9000;
        penaltySide = Side.Blue;
        penaltyLimitTime = 250;
        penaltyOfNum = 1;
        penaltyTime = 0;
        standoffTime = 0;
        blueScore = matchInfo.Score.BlueScore;
        yellowScore = matchInfo.Score.YellowScore;
        lastJudge = new JudgeResult
        {
            Actor = Side.Nobody,
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
        
        //正常比赛状态：上半场、下半场、加时赛
        if (matchInfo.MatchState == MatchState.FirstHalf || matchInfo.MatchState == MatchState.SecondHalf 
            || matchInfo.MatchState == MatchState.OverTime)
        {
            if (JudgePlace(ref judgeResult))
                return judgeResult;

            if (JudgePenalty(ref judgeResult))
                return judgeResult;

            if (JudgeGoalie(ref judgeResult))
                return judgeResult;

            if (JudgeFree(ref judgeResult))
                return judgeResult;

            //判断上下半场、加时赛结束，如果此时游戏分出胜负，则返回gameover
            if (JudgeHalfOrGameEnd(ref judgeResult))
                return judgeResult;
            //默认返回正常比赛
            return new JudgeResult
            {
                ResultType = ResultType.NormalMatch,
                Actor = Side.Nobody,
                Reason = "Normal competition"
            };
        }
        //点球大战状态
        else
        {
            //点球限制时间未结束
            if (penaltyTime < penaltyLimitTime)
            {
                if (JudgePenaltyGoal(ref judgeResult))
                    return judgeResult;

                //未进球，拍数增加
                penaltyTime = penaltyTime + 1;
                return new JudgeResult
                {
                    ResultType = ResultType.NormalMatch,
                    Actor = Side.Nobody,
                    Reason = "Normal competition"
                };
            }
            //点球限制时间结束，更新状态
            else
            {
                //五轮结束，进行结算
                if (penaltyOfNum == 5 && penaltySide == Side.Yellow)
                {
                    JudgeFiveRoundPenalty(ref judgeResult);
                }
                else
                {
                    UpdatePenaltyState(ref judgeResult);
                }
                return judgeResult;
            }
        }
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

    //第五轮点球，且黄方已经点完，进行判断是否该结束比赛,同时更新数据
    private void JudgeFiveRoundPenalty(ref JudgeResult judgeResult)
    {
        if (blueScore > yellowScore)
        {
            judgeResult = new JudgeResult
            {
                ResultType = ResultType.GameOver,
                Reason = "Blue team win the game",
                Actor = Side.Nobody
            };
        }
        else if (blueScore < yellowScore)
        {
            judgeResult = new JudgeResult
            {
                ResultType = ResultType.GameOver,
                Reason = "Yellow team win the game",
                Actor = Side.Nobody
            };
        }
        else
        {
            penaltyTime = 0;
            penaltySide = Side.Blue;
            penaltyOfNum = penaltyOfNum + 1;
            judgeResult = new JudgeResult
            {
                ResultType = ResultType.PenaltyKick,
                Actor = Side.Blue,
                Reason = "Over 10 second and turn to Blue penalty"
            };
        }
    }

    private void UpdatePenaltyState(ref JudgeResult judgeResult)
    {
        penaltyTime = 0;
        if (penaltySide == Side.Blue)
        {
            penaltySide = Side.Yellow;
            judgeResult = new JudgeResult
            {
                ResultType = ResultType.PenaltyKick,
                Actor = Side.Yellow,
                Reason = "Over 10 second and turn to yellow penalty"
            };
        }
        else
        {
            penaltySide = Side.Blue;
            penaltyOfNum = penaltyOfNum + 1;
            judgeResult = new JudgeResult
            {
                ResultType = ResultType.PenaltyKick,
                Actor = Side.Blue,
                Reason = "Over 10 second and turn to Blue penalty"
            };
        }

    }
    private bool JudgePenaltyGoal(ref JudgeResult judgeResult)
    {
        //若比赛超过五轮后，采用“突然死亡法”，先进球者先获胜 
        if (yellowGoalState.InSquare(matchInfo.Ball.pos))
        {
            //点球大战超过五轮后，进球直接胜利
            if (penaltyOfNum > 5)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.GameOver,
                    Reason = "In penalty , Blue team win the game",
                    Actor = Side.Nobody
                };
            }
            else
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Reason = "Blue penalty successfully and turn to yellow penalty",
                    Actor = Side.Yellow
                };
            }
            penaltyTime = 0;
            penaltySide = Side.Yellow;
            Event.Send(Event.EventType1.GetGoal, Side.Blue); //黄方被进球
            return true;
        }
        else if (blueGoalState.InSquare(matchInfo.Ball.pos))
        {
            //点球大战超过五轮后，进球直接胜利
            if (penaltyOfNum > 5)
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.GameOver,
                    Reason = "In penalty , Yellow team win the game",
                    Actor = Side.Nobody
                };
            }
            //特殊情况：黄方点球时，且为第五轮点球，进行结算
            if (penaltyOfNum == 5)
            {
                if (blueScore > yellowScore)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GameOver,
                        Reason = "Blue team win the game",
                        Actor = Side.Nobody
                    };
                }
                else if (blueScore < yellowScore)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GameOver,
                        Reason = "Yellow team win the game",
                        Actor = Side.Nobody
                    };
                }
                else
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.PenaltyKick,
                        Reason = "Yellow penalty successfully and trun to Blue penalty",
                        Actor = Side.Blue
                    };
                }
            }
            else
            {
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Reason = "Yellow penalty successfully and trun to Blue penalty",
                    Actor = Side.Blue
                };
            }
            penaltyTime = 0;
            penaltySide = Side.Blue;
            Event.Send(Event.EventType1.GetGoal, Side.Yellow); //蓝方被进球
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool JudgePlace(ref JudgeResult judgeResult)
    {
        //首拍，执行开球
        if (matchInfo.TickMatch == 0)
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Blue,
                Reason = "New game start and first PlaceKick",
                ResultType = ResultType.PlaceKick
            };
            return true;
        }
        //进球
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
                standoffTime = 0;
                if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKickRightTop,
                        Actor = Side.Blue,
                        Reason = "RightTop Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
                else if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y < 0)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKickRightBot,
                        Actor = Side.Blue,
                        Reason = "RightBot Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
                else if (matchInfo.Ball.pos.x < 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKickLeftTop,
                        Actor = Side.Yellow,
                        Reason = "LeftTop Standoff time longer than 10 seconds in game"
                    };
                    return true;
                }
                else
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.FreeKickLeftBot,
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

    private bool JudgeHalfOrGameEnd(ref JudgeResult judgeResult)
    {
        if (matchInfo.MatchState == MatchState.FirstHalf)
        {
            if (matchInfo.TickMatch == endOfHalfgametime)
            {
                matchInfo.MatchState = MatchState.SecondHalf;
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.EndHalf,
                    Actor = Side.Nobody,
                    Reason = "FirstHalf Game end"
                };
                return true;
            }
            else return false;
        }
        else if (matchInfo.MatchState == MatchState.SecondHalf)
        {
            //下半场结束，判断比分是否结束
            if (matchInfo.TickMatch == endOfHalfgametime)
            {
                if (blueScore > yellowScore)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GameOver,
                        Reason = "Blue team win the game",
                        Actor = Side.Nobody
                    };
                }
                else if (blueScore < yellowScore)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GameOver,
                        Reason = "Yellow team win the game",
                        Actor = Side.Nobody
                    };
                }
                else
                {
                    matchInfo.MatchState = MatchState.OverTime;
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.EndHalf,
                        Actor = Side.Nobody,
                        Reason = "SecondHalf Game end and start"
                    };
                }
                return true;
            }
            else return false;
        }
        //加时赛结束，同样判断比分
        else if (matchInfo.MatchState == MatchState.OverTime)
        {
            if (matchInfo.TickMatch == endOfOvergametime)
            {
                if (blueScore > yellowScore)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GameOver,
                        Reason = "Game over, Blue team win the game",
                        Actor = Side.Nobody
                    };
                }
                else if (blueScore < yellowScore)
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GameOver,
                        Reason = "Game over, Yellow team win the game",
                        Actor = Side.Nobody
                    };
                }
                else
                {
                    matchInfo.MatchState = MatchState.Penalty;
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.EndHalf,
                        Actor = Side.Nobody,
                        Reason = "Overtime Game end ,and start Penalty game"
                    };
                }
                return true;
            }    
            else return false;
        }
        else
        {
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

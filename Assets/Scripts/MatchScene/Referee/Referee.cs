using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Simuro5v5;
using UnityEngine;
using Event = Simuro5v5.EventSystem.Event;
using Simuro5v5.Util;

/// <summary>
/// 裁判根据比赛规则对场地信息进行判断。
/// 有两个主要的对外接口：Judge和JudgeAutoPlacement
/// Judge接口根据传入的MatchInfo，做出对下一拍动作的指示；
/// JudgeAutoPlacement接口用于更正摆位的位置，将不合法的位置合法化。
/// </summary>
public class Referee : ICloneable
{
    //Class
    private GameObject[] BlueObject;
    private GameObject[] YellowObject;
    private GameObject BallObject;
    private MatchInfo matchInfo;
    private Robot[] blueRobots;
    private Robot[] yellowRobots;

    private UprightRectangle defenderHalfState;
    private UprightRectangle offensiveHalfState;
    private static readonly UprightRectangle yellowHalfState = new UprightRectangle(-110, 0, 90, -90);
    private static readonly UprightRectangle yellowGoalState = new UprightRectangle(-125, -110, 20, -20);
    private static readonly UprightRectangle yellowBigState = new UprightRectangle(-125, -75, 40, -40);
    private static readonly UprightRectangle yellowSmallState = new UprightRectangle(-125, -95, 25, -25);
    private static readonly UprightRectangle blueHalfState = new UprightRectangle(0, 110, 90, -90);
    private static readonly UprightRectangle blueGoalState = new UprightRectangle(110, 125, 20, -20);
    private static readonly UprightRectangle blueBigState = new UprightRectangle(75, 125, 40, -40);
    private static readonly UprightRectangle blueSmallState = new UprightRectangle(95, 125, 25, -25);
    private static readonly UprightRectangle stadiumState = new UprightRectangle(-110, 110, 90, -90);

    private int goalieBlueId;
    private int goalieYellowId;

    private int blueScore;
    private int yellowScore;

    private Vector2D[] BlueRobotsPos;
    private Vector2D[] YellowRobotsPos;

    private Vector2D[] OffensiveRobotsPos;
    private Vector2D[] DefenderRobotsPos;

    private Vector2D BallPos;

    /// <summary>
    /// 上下半场游戏比赛时间 5分钟
    /// </summary>
    private int endOfHalfgametime;

    /// <summary>
    /// 加时赛游戏比赛时间 3分钟
    /// </summary>
    private int endOfOvergametime;

    /// <summary>
    /// 点球大战中每次罚球点球限制时间5秒
    /// </summary>
    private int penaltyLimitTime;

    /// <summary>
    /// 点球大战中点球方，依次交换顺序
    /// TODO: 将这些信息暴露出去，显示在UI上
    /// </summary>
    private Side penaltySide;

    /// <summary>
    /// 点球大战中所执行的时间
    /// </summary>
    private int penaltyTime;

    /// <summary>
    /// 记录点球次数
    /// TODO: 将这些信息暴露出去，显示在UI上
    /// </summary>
    private int penaltyOfNum;

    /// <summary>
    /// 球的最大的停滞时间
    /// </summary>
    private int maxStandoffTime;

    /// <summary>
    /// 球的停滞时间
    /// </summary>
    [JsonProperty]
    private int standoffTime;

    /// <summary>
    /// 保存最近一次的判决结果
    /// </summary>
    [JsonProperty]
    public JudgeResult savedJudge;

    public Referee()
    {
#if UNITY_EDITOR
        // 编辑器中调试的时候将时间设置短一点
        endOfHalfgametime = 1000;
        endOfOvergametime = 1000;
#else
        endOfHalfgametime = 15000;
        endOfOvergametime = 9000;
#endif
        penaltySide = Side.Blue;
        penaltyLimitTime = 250;
        penaltyTime = 0;
        standoffTime = 0;
        maxStandoffTime = 500;

        BlueRobotsPos = new Vector2D[Const.RobotsPerTeam];
        YellowRobotsPos = new Vector2D[Const.RobotsPerTeam];
        OffensiveRobotsPos = new Vector2D[Const.RobotsPerTeam];
        DefenderRobotsPos = new Vector2D[Const.RobotsPerTeam];

        ObjectManager.FindObjects(out BlueObject, out YellowObject, out BallObject);

        savedJudge = new JudgeResult
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
            savedJudge = savedJudge
        };
    }

    /// <summary>
    /// 根据传入的matchInfo，结合已保存的信息，给出下一拍应有的动作（JudgeResult）。<br/>
    /// 这个接口不会对<parmref name="matchInfo">作任何修改，所有的信息由返回值给出
    /// </summary>
    /// <param name="matchInfo">需要被判断的比赛信息</param>
    /// <returns>下一拍应有的动作信息</returns>
    public JudgeResult Judge(MatchInfo matchInfo)
    {
        this.blueScore = matchInfo.Score.BlueScore;
        this.yellowScore = matchInfo.Score.YellowScore;
        this.matchInfo = matchInfo;
        this.blueRobots = matchInfo.BlueRobots;
        this.yellowRobots = matchInfo.YellowRobots;
        this.goalieBlueId = FindGoalie(Side.Blue);
        this.goalieYellowId = FindGoalie(Side.Yellow);

        var result = CollectJudge();
        savedJudge = result;
        return result;
    }

    /// <summary>
    /// 判断一个摆位是否合法，不合法则根据规则矫正
    /// </summary>
    /// <param name="matchInfo">摆位的信息</param>
    /// <param name="judgeResult">上次摆位的信息</param>
    public void JudgeAutoPlacement(MatchInfo matchInfo, JudgeResult judgeResult)
    {
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            this.BlueRobotsPos[i] = matchInfo.BlueRobots[i].pos;
            this.YellowRobotsPos[i] = matchInfo.YellowRobots[i].pos;
        }
        this.BallPos = matchInfo.Ball.pos;


        switch (judgeResult.ResultType)
        {
            case ResultType.PenaltyKick:
                JudgePenaltyPlacement(matchInfo, judgeResult);
                break;
            case ResultType.PlaceKick:
                JudgePlacePlacement(matchInfo, judgeResult);
                break;
        }
    }

    private JudgeResult CollectJudge()
    {
        JudgeResult judgeResult = default;

        //正常比赛状态：上半场、下半场、加时赛
        if (matchInfo.MatchPhase == MatchPhase.FirstHalf || matchInfo.MatchPhase == MatchPhase.SecondHalf
            || matchInfo.MatchPhase == MatchPhase.OverTime)
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
            if (matchInfo.TickMatch == 0)
            {
                return new JudgeResult
                {
                    ResultType = ResultType.PenaltyKick,
                    Actor = Side.Blue,
                    Reason = "Penalty competition start , Blue first"
                };
            }
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

    private int FindGoalie(Side side)
    {
        int id = -1;
        if (side == Side.Blue)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (blueSmallState.PointIn(blueRobots[i].pos))
                {
                    id = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (yellowSmallState.PointIn(yellowRobots[i].pos))
                {
                    id = i;
                }
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
                Reason = "Over 5 second and turn to Blue penalty"
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
                Reason = "Over 5 second and turn to yellow penalty"
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
                Reason = "Over 5 second and turn to Blue penalty"
            };
        }

    }
    private bool JudgePenaltyGoal(ref JudgeResult judgeResult)
    {
        //若比赛超过五轮后，采用“突然死亡法”，先进球者先获胜
        if (yellowGoalState.PointIn(matchInfo.Ball.pos))
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
            judgeResult.WhoGoal = Side.Blue;
            return true;
        }
        else if (blueGoalState.PointIn(matchInfo.Ball.pos))
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
            judgeResult.WhoGoal = Side.Yellow;
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
            string matchState;
            switch (matchInfo.MatchPhase)
            {
                case MatchPhase.FirstHalf:
                    matchState = "First Half";
                    break;
                case MatchPhase.SecondHalf:
                    matchState = "Second Half";
                    break;
                case MatchPhase.OverTime:
                    matchState = "OverTime";
                    break;
                default:
                    matchState = "";
                    break;
            }
            judgeResult = new JudgeResult
            {
                Actor = Side.Blue,
                Reason = matchState + " start and first PlaceKick",
                ResultType = ResultType.PlaceKick
            };
            return true;

        }
        //进球
        if (yellowGoalState.PointIn(matchInfo.Ball.pos))
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Yellow,
                Reason = "Be scored and PlaceKick again",
                ResultType = ResultType.PlaceKick
            };
            judgeResult.WhoGoal = Side.Blue;
            return true;
        }
        if (blueGoalState.PointIn(matchInfo.Ball.pos))
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Blue,
                Reason = "Be scored and PlaceKick again",
                ResultType = ResultType.PlaceKick
            };
            judgeResult.WhoGoal = Side.Yellow;
            return true;
        }
        return false;
    }

    private bool JudgePenalty(ref JudgeResult judgeResult)
    {
        //考虑进入点球大战中，且首拍为0.进行点球
        if (matchInfo.MatchPhase == MatchPhase.Penalty && matchInfo.TickMatch == 0)
        {
            judgeResult = new JudgeResult
            {
                ResultType = ResultType.PenaltyKick,
                Actor = Side.Blue,
                Reason = "Penalty start and blue penalty"
            };
            return true;
        }
        if (matchInfo.Ball.pos.x > 0)
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (blueBigState.PointIn(blueRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (blueSmallState.PointIn(blueRobots[i].pos))
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
                if (yellowBigState.PointIn(yellowRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (yellowSmallState.PointIn(yellowRobots[i].pos))
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
        if (blueBigState.PointIn(matchInfo.Ball.pos))
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
                if (blueBigState.PointIn(yellowRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (blueSmallState.PointIn(yellowRobots[i].pos))
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
        else if (yellowBigState.PointIn(matchInfo.Ball.pos))
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
                if (yellowBigState.PointIn(blueRobots[i].pos))
                {
                    bigStateNum++;
                }
                if (yellowSmallState.PointIn(blueRobots[i].pos))
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
            if (standoffTime > maxStandoffTime)
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
        if (matchInfo.MatchPhase == MatchPhase.FirstHalf)
        {
            if (matchInfo.TickMatch == endOfHalfgametime)
            {
                // matchInfo.MatchPhase = MatchPhase.SecondHalf;
                judgeResult = new JudgeResult
                {
                    ResultType = ResultType.NextPhase,
                    Actor = Side.Nobody,
                    Reason = "FirstHalf Game end"
                };
                return true;
            }
            else return false;
        }
        else if (matchInfo.MatchPhase == MatchPhase.SecondHalf)
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
                    // matchInfo.MatchPhase = MatchPhase.OverTime;
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.NextPhase,
                        Actor = Side.Nobody,
                        Reason = "SecondHalf Game end and start"
                    };
                }
                return true;
            }
            else return false;
        }
        //加时赛结束，同样判断比分
        else if (matchInfo.MatchPhase == MatchPhase.OverTime)
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
                    // matchInfo.MatchPhase = MatchPhase.Penalty;
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.NextPhase,
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

    public struct SafePos
    {
        public Vector2D safePos;
        public bool occupy;
        public UprightRectangle safeSquare;
        public SafePos(Vector2D pos)
        {
            safePos = pos;
            occupy = false;
            safeSquare = UprightRectangle.RobotSquare(safePos);
        }
    }

    private void JudgePenaltyPlacement(MatchInfo matchInfo, JudgeResult judgeResult)
    {
        Vector2D PenaltyBallPos;//点球坐标
        Vector2D PenaltyDefenderGoaliePos;//守门员坐标
                                          //黄方的安全区域点
        SafePos[] PenaltyDefenderSafePos;

        //蓝方的安全区域点
        SafePos[] PenaltyOffensiveSafePos;
        int GoalieId = FindGoalie(Side.Blue);
        if (judgeResult.Actor == Side.Blue)
        {
            //蓝方执行点球时相关坐标
            PenaltyBallPos = new Vector2D(-72.5f, 0f);//点球坐标
            PenaltyDefenderGoaliePos = new Vector2D(-106f, 0f);//守门员坐标

            PenaltyDefenderSafePos = new SafePos[5] {  //黄方的安全区域点
                new SafePos(new Vector2D(5f, 6f)),
                new SafePos(new Vector2D(5f, 16f)),
                new SafePos(new Vector2D(5f, 26f)),
                new SafePos(new Vector2D(5f, 36f)),
                new SafePos(new Vector2D(5f, 46f)) };

            //蓝方的安全区域点
            PenaltyOffensiveSafePos = new SafePos[5] {
                new SafePos(new Vector2D(5f, -6f)),
                new SafePos(new Vector2D(5f, -16f)),
                new SafePos(new Vector2D(5f, -26f)),
                new SafePos(new Vector2D(5f, -36f)),
                new SafePos(new Vector2D(5f, -46f)) };

            JudgeSatePosOverlap(PenaltyOffensiveSafePos, PenaltyDefenderSafePos, Side.Blue, Side.Yellow);
            GoalieId = FindGoalie(Side.Blue);
            UpdateOffAndDefInfo(Side.Blue);
        }
        else
        {
            //黄方执行点球时相关坐标
            PenaltyBallPos = new Vector2D(72.5f, 0f);//点球坐标
            PenaltyDefenderGoaliePos = new Vector2D(106f, 0f);//守门员坐标
                                                              //黄方的安全区域点
            PenaltyDefenderSafePos = new SafePos[5] {
                new SafePos(new Vector2D(-5f, -6f)),
                new SafePos(new Vector2D(-5f, -16f)),
                new SafePos(new Vector2D(5f, -26f)),
                new SafePos(new Vector2D(-5f, -36f)),
                new SafePos(new Vector2D(-5f, -46f)) };

            //蓝方的安全区域点
            PenaltyOffensiveSafePos = new SafePos[5] {
                new SafePos(new Vector2D(-5f, 6f)),
                new SafePos(new Vector2D(-5f, 16f)),
                new SafePos(new Vector2D(-5f, 26f)),
                new SafePos(new Vector2D(-5f, 36f)),
                new SafePos(new Vector2D(-5f, 46f)) };

            JudgeSatePosOverlap(PenaltyOffensiveSafePos, PenaltyDefenderSafePos, Side.Yellow, Side.Blue);
            GoalieId = FindGoalie(Side.Yellow);
            UpdateOffAndDefInfo(Side.Yellow);
        }
        BallPos = PenaltyBallPos;
        if (GoalieId == -1)
        {
            DefenderRobotsPos[0] = PenaltyDefenderGoaliePos;
            GoalieId = 0;
        }
        else
        {
            //如果守门员没有压在球门线，将其压线
            if (judgeResult.Actor == Side.Blue)
            {
                if (OffensiveRobotsPos[GoalieId].x >= -106 || OffensiveRobotsPos[GoalieId].x <= -120)
                {
                    OffensiveRobotsPos[GoalieId].x = -106;
                }
            }
            else
            {
                if (OffensiveRobotsPos[GoalieId].x <= 106 || OffensiveRobotsPos[GoalieId].x >= 120)
                {
                    OffensiveRobotsPos[GoalieId].x = 106;
                }
            }

        }
        //先对防守方检测
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            if (i == GoalieId)
            {
                continue;
            }
            else
            {
                //如果位置在防守方半场内，规范化位置
                if (defenderHalfState.PointIn(DefenderRobotsPos[i]) || !stadiumState.PointIn(DefenderRobotsPos[i]))
                {
                    for (int j = 0; j < Const.RobotsPerTeam; j++)
                    {
                        if (!PenaltyDefenderSafePos[j].occupy)
                        {
                            DefenderRobotsPos[i] = PenaltyDefenderSafePos[j].safePos;
                            PenaltyDefenderSafePos[j].occupy = true;
                            break;
                        }
                    }
                }
            }
        }
        int attackRobotID=-1;

        //再对进攻方进行检测
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            //只允许有一个进攻球员在防守半场中
            if(defenderHalfState.PointIn(OffensiveRobotsPos[i] )&&attackRobotID ==-1)
            {
                attackRobotID = i;
                continue;
            }
            if (defenderHalfState.PointIn(OffensiveRobotsPos[i]) || !stadiumState.PointIn(OffensiveRobotsPos[i]))
            {
                for (int j = 0; j < Const.RobotsPerTeam; j++)
                {
                    if (!PenaltyOffensiveSafePos[j].occupy)
                    {
                        OffensiveRobotsPos[i] = PenaltyOffensiveSafePos[j].safePos;
                        PenaltyOffensiveSafePos[j].occupy = true;
                        break;
                    }
                }
            }
        }

        if (judgeResult.Actor == Side.Blue)
        {
            UpdatePlacementPos(matchInfo, Side.Blue);
        }
        else
        {
            UpdatePlacementPos(matchInfo, Side.Yellow);
        }

    }

    private void JudgePlacePlacement(MatchInfo matchInfo, JudgeResult judgeResult)
    {
        Vector2D PlaceBall = new Vector2D(0, 0);
    }

    //更新进攻方和防守方信息
    private void UpdateOffAndDefInfo(Side Defender)
    {
        if (Defender == Side.Blue)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                DefenderRobotsPos[i] = BlueRobotsPos[i];
                OffensiveRobotsPos[i] = YellowRobotsPos[i];
            }
            offensiveHalfState = blueHalfState;
            defenderHalfState = yellowHalfState;
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                DefenderRobotsPos[i] = YellowRobotsPos[i];
                OffensiveRobotsPos[i] = BlueRobotsPos[i];
            }
            offensiveHalfState = yellowHalfState;
            defenderHalfState = blueHalfState;
        }
    }

    private void UpdatePlacementPos(MatchInfo matchInfo, Side Offensive)
    {
        matchInfo.Ball.pos = BallPos;
        if (Offensive == Side.Blue)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                matchInfo.BlueRobots[i].pos = OffensiveRobotsPos[i];
                matchInfo.YellowRobots[i].pos = DefenderRobotsPos[i];
            }
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                matchInfo.YellowRobots[i].pos = OffensiveRobotsPos[i];
                matchInfo.BlueRobots[i].pos = DefenderRobotsPos[i];
            }
        }
    }

    //判断是否占用安全区域情况
    private void JudgeSatePosOverlap(SafePos[] OffensiveSafePos, SafePos[] DefenderSafePos, Side Offensive, Side Defender)
    {
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            for (int j = 0; j < Const.RobotsPerTeam; j++)
            {
                //warning Point in 需要是两个区域进行判断
                //进攻方是蓝方时
                if (Offensive == Side.Blue)
                {
                    //检验蓝方安全区域
                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].safeSquare.PointIn(BlueRobotsPos[j]))
                    {
                        OffensiveSafePos[i].occupy = true;
                    }
                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].safeSquare.PointIn(YellowRobotsPos[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        OffensiveSafePos[i].safePos = DefenderSafePos[i].safePos;
                        DefenderSafePos[i].occupy = true;
                    }
                    //检验黄方安全区域
                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].safeSquare.PointIn(YellowRobotsPos[j]))
                    {
                        DefenderSafePos[i].occupy = true;
                    }
                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].safeSquare.PointIn(BlueRobotsPos[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        DefenderSafePos[i].safePos = OffensiveSafePos[i].safePos;
                        OffensiveSafePos[i].occupy = true;
                    }
                }
                //进攻方是黄方时
                else
                {
                    //检验黄方安全区域
                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].safeSquare.PointIn(YellowRobotsPos[j]))
                    {
                        OffensiveSafePos[i].occupy = true;
                    }
                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].safeSquare.PointIn(BlueRobotsPos[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        OffensiveSafePos[i].safePos = DefenderSafePos[i].safePos;
                        DefenderSafePos[i].occupy = true;
                    }
                    //检验蓝方安全区域
                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].safeSquare.PointIn(BlueRobotsPos[j]))
                    {
                        DefenderSafePos[i].occupy = true;
                    }
                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].safeSquare.PointIn(YellowRobotsPos[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        DefenderSafePos[i].safePos = OffensiveSafePos[i].safePos;
                        OffensiveSafePos[i].occupy = true;
                    }
                }
            }
        }
    }
}



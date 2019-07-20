using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Simuro5v5;
using UnityEngine;
using NUnit.Framework;
using Simuro5v5.Util;
using System.IO;
using Random = System.Random;

/// <summary>
/// 裁判根据比赛规则对场地信息进行判断。
/// 有两个主要的对外接口：Judge和JudgeAutoPlacement
/// Judge接口根据传入的MatchInfo，做出对下一拍动作的指示；
/// JudgeAutoPlacement接口用于更正摆位的位置，将不合法的位置合法化。
/// </summary>
public class Referee : ICloneable
{
    private GameObject[] BlueObject;
    private GameObject[] YellowObject;
    private GameObject BallObject;
    private MatchInfo matchInfo;
    private Robot[] blueRobots;
    private Robot[] yellowRobots;

    private UprightRectangle FreeState;
    private UprightRectangle defenderSmallState;
    private UprightRectangle offensiveSmallState;
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
    private Square[] BlueRobotSquare;
    private Vector2D[] YellowRobotsPos;
    private Square[] YellowRobotSquare;

    //private RobotPosSquare[] DefenderPosSquare;
    //private RobotPosSquare[] OffensivePosSquare;
    //private RobotPosSquare[] SafePosSquare;

    //private Vector2D[] OffensiveRobotsPos;
    //private Vector2D[] DefenderRobotsPos;

    private Vector2D BallPos;

    /// <summary>
    /// 上下半场游戏比赛时间 5分钟
    /// </summary>
    private int endOfHalfGameTime;

    /// <summary>
    /// 加时赛游戏比赛时间 3分钟
    /// </summary>
    private int endOfOverGameTime;

    //点球大战有关信息开始

    /// <summary>
    /// 点球大战中每次罚球点球限制时间3秒
    /// </summary>
    private int penaltyLimitTime;

    /// <summary>
    /// 点球大战根据踢一次球后判断有没有进球，保存是否进行了踢球
    /// </summary>
    private bool penaltyTouchBall;


    /// <summary>
    /// 点球大战中点球方，依次交换顺序
    /// TODO: 将这些信息暴露出去，显示在UI上
    /// </summary>
    private Side penaltySide;

    /// <summary>
    /// 点球大战未踢球时间
    /// </summary>
    private int penaltyTime;

    /// <summary>
    /// 记录点球次数
    /// TODO: 将这些信息暴露出去，显示在UI上
    /// </summary>
    private int penaltyOfNum;

    /// <summary>
    /// 点球大战中点球球员的序号
    /// </summary>
    private int penaltyAttackID;

    /// <summary>
    /// 点球大战中进攻方守门员的序号
    /// </summary>
    private int penaltyGoalID;
    private UprightRectangle defenderPenaltySmallState;
    private UprightRectangle offensivePenaltySmallState;
    ///点球大战有关信息结束 

    /// <summary>
    /// 球的最大的停滞时间
    /// </summary>
    private int maxStandoffTime;

    /// <summary>
    /// 球的停滞时间
    /// </summary>
    [JsonProperty] private int standoffTime;

    /// <summary>
    /// 保存最近一次的判决结果
    /// </summary>
    [JsonProperty] public JudgeResult savedJudge;

    public Referee()
    {
        //// 编辑器中调试的时候将时间设置短一点
        //endOfHalfGameTime = 20 * Const.FramePerSecond;
        //endOfOverGameTime = 20 * Const.FramePerSecond;
        endOfHalfGameTime = 5 * 60 * Const.FramePerSecond;
        endOfOverGameTime = 5 * 60 * Const.FramePerSecond;
        penaltySide = Side.Blue;
        penaltyLimitTime = 3 * Const.FramePerSecond;
        penaltyTime = 0;
        standoffTime = 0;
        //test three attack
        maxStandoffTime = 10 * Const.FramePerSecond;

        BlueRobotsPos = new Vector2D[Const.RobotsPerTeam];
        YellowRobotsPos = new Vector2D[Const.RobotsPerTeam];
        BlueRobotSquare = new Square[Const.RobotsPerTeam];
        YellowRobotSquare = new Square[Const.RobotsPerTeam];

        ObjectManager.FindObjects(out BlueObject, out YellowObject, out BallObject);

        penaltySide = Side.Nobody;
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
    /// 这个接口不会对<paramref name="matchInfo"/>作任何修改，所有的信息由返回值给出
    /// </summary>
    /// <param name="matchInfo">需要被判断的比赛信息</param>
    /// <returns>下一拍应有的动作信息</returns>
    public JudgeResult Judge(MatchInfo matchInfo)
    {
        ////test penalty shootout
        //if(matchInfo.MatchPhase == MatchPhase.FirstHalf)
        //{
        //    matchInfo.MatchPhase = MatchPhase.Penalty;
        //}
        //test three attack
        for (int i = 1; i < Const.RobotsPerTeam; i++)
        {
            //    matchInfo.YellowRobots[i].pos.y = 200;
        }
        /////
        this.blueScore = matchInfo.Score.BlueScore;
        this.yellowScore = matchInfo.Score.YellowScore;
        this.matchInfo = matchInfo;
        this.blueRobots = matchInfo.BlueRobots;
        this.yellowRobots = matchInfo.YellowRobots;
        this.goalieBlueId = FindGoalie(Side.Blue);
        this.goalieYellowId = FindGoalie(Side.Yellow);
        this.BallPos = matchInfo.Ball.pos;
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            BlueRobotSquare[i] = new Square(BlueRobotsPos[i], matchInfo.BlueRobots[i].rotation);
            YellowRobotSquare[i] = new Square(YellowRobotsPos[i], matchInfo.YellowRobots[i].rotation);
        }
        var result = CollectJudge();
        savedJudge = result;
        // 判罚之后应该清零计时
        if (result.ResultType != ResultType.NormalMatch)
        {
            this.standoffTime = 0;
        }
        return result;
    }

    /// <summary>
    /// 判断一个摆位是否合法，不合法则根据规则矫正
    /// </summary>
    /// <param name="matchInfo">摆位的信息</param>
    /// <param name="judgeResult">上次摆位的信息</param>
    public void JudgeAutoPlacement(MatchInfo matchInfo, JudgeResult judgeResult, Side target)
    {
        this.matchInfo = matchInfo;
        this.blueRobots = matchInfo.BlueRobots;
        this.yellowRobots = matchInfo.YellowRobots;

        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            this.BlueRobotsPos[i] = matchInfo.BlueRobots[i].pos;
            this.YellowRobotsPos[i] = matchInfo.YellowRobots[i].pos;
            BlueRobotSquare[i] = new Square(BlueRobotsPos[i], matchInfo.BlueRobots[i].rotation);
            YellowRobotSquare[i] = new Square(YellowRobotsPos[i], matchInfo.YellowRobots[i].rotation);
        }

        //该情况一般只用于门球情况，进攻方自定义球的坐标
        this.BallPos = matchInfo.Ball.pos;
        switch (judgeResult.ResultType)
        {
            case ResultType.PenaltyKick:
                JudgePenaltyPlacement(judgeResult, target);
                break;
            case ResultType.PlaceKick:
                JudgePlacePlacement(judgeResult, target);
                break;
            case ResultType.GoalKick:
                JudgeGoaliePlacement(judgeResult, target);
                break;
            case ResultType.FreeKickLeftBot:
            case ResultType.FreeKickLeftTop:
            case ResultType.FreeKickRightBot:
            case ResultType.FreeKickRightTop:
                JudgeFreePlacement(judgeResult, target);
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
        //点球大战情况
        else
        {
            JudgePenaltyShootOut(ref judgeResult);
            return judgeResult;
        }
    }

    private void JudgePenaltyShootOut(ref JudgeResult judgeResult)
    {
        Side IllegalMoveSide = Side.Nobody;
        //刚开始罚球的第一拍
        if (penaltyTime == 0)
        {
            if (penaltySide == Side.Nobody)
            {
                penaltySide = Side.Blue;
            }
            //判罚发现点球大战结束
            if (JudgePenaltyOver(ref judgeResult))
            {
                return;
            }
            //未结束的话更新相关信息
            if (penaltySide == Side.Blue)
            {
                offensivePenaltySmallState = blueSmallState;
                defenderPenaltySmallState = yellowSmallState;
            }
            else
            {
                offensivePenaltySmallState = yellowSmallState;
                defenderSmallState = blueSmallState;
            }
            penaltyTouchBall = false;
            penaltyAttackID = FindPenaltyAttackID(penaltySide);
            penaltyGoalID = FindGoalie(penaltySide);
            penaltyTime++;
        }
        //判断有没有非法移动
        else if (JudgePenaltyIllegalMove(ref IllegalMoveSide, penaltySide))
        {
            if (IllegalMoveSide == penaltySide)
            {
                judgeResult = new JudgeResult
                {
                    Actor = penaltySide.ToAnother(),
                    ResultType = ResultType.PenaltyKick,
                    Reason = "Offensive side illegal move,switch the penalty side"
                };
            }
            else
            {
                judgeResult = new JudgeResult
                {
                    Actor = penaltySide.ToAnother(),
                    ResultType = ResultType.PenaltyKick,
                    Reason = "Defensive side illegal move,offensive get score and switch penalty side",
                    WhoGoal = penaltySide
                };
            }
            UpdateNewPenalty();
        }
        //未踢球且在限制时间内，寻找踢球球员
        else if (!penaltyTouchBall && penaltyTime < penaltyLimitTime)
        {
            if (BlueRobotSquare[penaltyAttackID].OverlapWithCircle(BallPos))
            {
                penaltyTouchBall = true;
            }
            penaltyTime++;
        }
        //超过限制时间内仍未踢球
        else if (!penaltyTouchBall && penaltyTime >= penaltyLimitTime)
        {
            judgeResult = new JudgeResult
            {
                Actor = penaltySide.ToAnother(),
                ResultType = ResultType.PenaltyKick,
                Reason = "overtime with penalty and switch the penalty side"
            };
            UpdateNewPenalty();
        }
        //已踢球，但未到规定时间,判断有没有进球
        else if (penaltyTouchBall && penaltyTime < penaltyLimitTime)
        {
            if (offensivePenaltySmallState.ContainsPoint(BallPos))
            {
                judgeResult = new JudgeResult
                {
                    Actor = penaltySide.ToAnother(),
                    ResultType = ResultType.PenaltyKick,
                    Reason = "offensive team goal and switch the peanlty side",
                    WhoGoal = penaltySide
                };
                UpdateNewPenalty();
            }
            else
            {
                judgeResult = new JudgeResult
                {
                    Actor = Side.Nobody,
                    ResultType = ResultType.NormalMatch,
                    Reason = "Normal competition"
                };
                penaltyTime++;
            }
        }
        else if (penaltyTouchBall && penaltyTime > penaltyLimitTime)
        {
            judgeResult = new JudgeResult
            {
                Actor = penaltySide.ToAnother(),
                ResultType = ResultType.PenaltyKick,
                Reason = "overtime with penalty and switch the penalty side"
            };
            UpdateNewPenalty();
        }
        else
        {
            Debug.LogError("Unreachable code");
        }
    }

    //更新新一轮点球信息
    private void UpdateNewPenalty()
    {
        penaltySide = penaltySide.ToAnother();
        penaltyTime = 0;
        if (penaltySide == Side.Blue)
        {
            penaltyOfNum++;
        }
    }

    private bool JudgePenaltyOver(ref JudgeResult judgeResult)
    {
        if (penaltyOfNum == 5 && penaltySide == Side.Blue)
        {
            if (blueScore > yellowScore)
            {
                judgeResult = new JudgeResult
                {
                    Actor = Side.Blue,
                    ResultType = ResultType.GameOver,
                    Reason = "Blue team win the game"
                };
                return true;
            }
            else if (yellowScore > blueScore)
            {
                judgeResult = new JudgeResult
                {
                    Actor = Side.Yellow,
                    ResultType = ResultType.GameOver,
                    Reason = "Yellow team win the game"
                };
                return true;
            }
            else
            {
                judgeResult = new JudgeResult
                {
                    Actor = Side.Nobody,
                    ResultType = ResultType.NormalMatch,
                    Reason = "In the sudden death model"
                };
                return false;
            }
        }
        return false;
    }

    private int FindGoalie(Side side)
    {
        int id = -1;
        if (side == Side.Blue)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (blueSmallState.ContainsPoint(blueRobots[i].pos))
                {
                    id = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (yellowSmallState.ContainsPoint(yellowRobots[i].pos))
                {
                    id = i;
                }
            }
        }
        return id;
    }

    //判断有没有非法移动，点球大战除了一个进攻球员和一个守门员可以移动，其余球员不可以移动
    private bool JudgePenaltyIllegalMove(ref Side IllegalMoveSide, Side OffensiveSide)
    {
        Side defensiveSide = OffensiveSide.ToAnother();
        Robot[] offensiveRobots, defensiveRobots;
        if (OffensiveSide == Side.Blue)
        {
            offensiveRobots = blueRobots;
            defensiveRobots = yellowRobots;
        }
        else
        {
            offensiveRobots = yellowRobots;
            defensiveRobots = blueRobots;
        }
        IllegalMoveSide = Side.Nobody;
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            if (i == penaltyAttackID)
            {
                continue;
            }
            if (offensiveRobots[i].wheel.right != 0 || offensiveRobots[i].wheel.left != 0)
            {
                IllegalMoveSide = OffensiveSide;
                return true;
            }
        }
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            if (i == penaltyGoalID)
            {
                continue;
            }
            if (defensiveRobots[i].wheel.right != 0 || defensiveRobots[i].wheel.left != 0)
            {
                IllegalMoveSide = defensiveSide;
                return true;
            }
        }
        return false;
    }

    private int FindPenaltyAttackID(Side side)
    {
        if (side == Side.Blue)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (yellowHalfState.ContainsPoint(blueRobots[i].pos))
                {
                    penaltyAttackID = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (blueHalfState.ContainsPoint(yellowRobots[i].pos))
                {
                    penaltyAttackID = i;
                }
            }
        }
        return penaltyAttackID;
    }
    private bool JudgePlace(ref JudgeResult judgeResult)
    {
        //首拍，执行开球
        if (matchInfo.TickPhase == 0)
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
        if (yellowGoalState.ContainsCircle(matchInfo.Ball.pos, Const.Ball.HBL))
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Yellow,
                Reason = "Be scored and PlaceKick again",
                ResultType = ResultType.PlaceKick,
                WhoGoal = Side.Blue
            };
            return true;
        }

        if (blueGoalState.ContainsCircle(matchInfo.Ball.pos, Const.Ball.HBL))
        {
            judgeResult = new JudgeResult
            {
                Actor = Side.Blue,
                Reason = "Be scored and PlaceKick again",
                ResultType = ResultType.PlaceKick,
                WhoGoal = Side.Yellow
            };
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
            for (int i = 0; i < 5; i++)
            {
                if (blueBigState.ContainsPoint(blueRobots[i].pos))
                {
                    bigStateNum++;
                }

                if (blueSmallState.ContainsPoint(blueRobots[i].pos))
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
                    Actor = Side.Yellow,
                    Reason = "Defenders have two robots in SmallState"
                };
                return true;
            }
        }
        else
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            //test three attack
            //for (int i = 0; i < 2; i++)
            for(int i = 0 ;i<5;i++)
            {
                if (yellowBigState.ContainsPoint(yellowRobots[i].pos))
                {
                    bigStateNum++;
                }

                if (yellowSmallState.ContainsPoint(yellowRobots[i].pos))
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
        if (blueBigState.ContainsPoint(matchInfo.Ball.pos))
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (goalieBlueId != -1 &&
                    JudgeCollision2(matchInfo.BlueRobots[goalieBlueId], matchInfo.YellowRobots[i]))
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GoalKick,
                        Actor = Side.Blue,
                        Reason = "Attacker hit the Goalie"
                    };
                    return true;
                }

                if (blueBigState.ContainsPoint(yellowRobots[i].pos))
                {
                    bigStateNum++;
                }

                if (blueSmallState.ContainsPoint(yellowRobots[i].pos))
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
        else if (yellowBigState.ContainsPoint(matchInfo.Ball.pos))
        {
            int smallStateNum = 0;
            int bigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (goalieYellowId != -1 &&
                    JudgeCollision2(matchInfo.YellowRobots[goalieYellowId], matchInfo.BlueRobots[i]))
                {
                    judgeResult = new JudgeResult
                    {
                        ResultType = ResultType.GoalKick,
                        Actor = Side.Yellow,
                        Reason = "Attacker hit the Goalie"
                    };
                    return true;
                }

                if (yellowBigState.ContainsPoint(blueRobots[i].pos))
                {
                    bigStateNum++;
                }

                if (yellowSmallState.ContainsPoint(blueRobots[i].pos))
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
        if (matchInfo.Ball.linearVelocity.GetUnityVector2().magnitude < 50)
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
            if (matchInfo.TickPhase == endOfHalfGameTime)
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

            return false;
        }

        if (matchInfo.MatchPhase == MatchPhase.SecondHalf)
        {
            //下半场结束，判断比分是否结束
            if (matchInfo.TickPhase == endOfHalfGameTime)
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

            return false;
        }

        //加时赛结束，同样判断比分
        if (matchInfo.MatchPhase == MatchPhase.OverTime)
        {
            if (matchInfo.TickPhase == endOfOverGameTime)
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

            return false;
        }

        return false;
    }

    /// <summary>
    /// whether Object1 collide Object2
    /// </summary>
    /// <param name="object1"></param>
    /// <param name="object2"></param>
    /// <returns></returns>
    [Obsolete("Use JudgeCollision2")]
    private bool JudgeCollision(GameObject object1, GameObject object2)
    {
        List<GameObject> touchObject = object1.GetComponent<BoxColliderEvent>().TouchObject;
        if (touchObject.IndexOf(object2) == -1)
        {
            //touchObject.Clear();
            return false;
        }
        else
        {
            //touchObject.Clear();
            return true;
        }
    }

    /// <summary>
    /// whether robot1 collide robot2
    /// </summary>
    private bool JudgeCollision2(Robot robot1, Robot robot2)
    {
        var square1 = new Square(robot1.pos, robot1.rotation);
        var square2 = new Square(robot2.pos, robot2.rotation);
        return square1.IsCrossedBy(square2);
    }

    public struct RobotPosSquare
    {
        public Vector2D Pos;
        public bool occupy;
        public Square square;

        public RobotPosSquare(Vector2D pos, float angle = 0)
        {
            Pos = pos;
            occupy = false;
            square = new Square(Pos, angle);
        }

        public void GetNewPosSquare(Vector2D pos, float angle = 0)
        {
            Pos = pos;
            occupy = false;
            square = new Square(Pos, angle);
        }
    }

    private void JudgePenaltyPlacement(JudgeResult judgeResult, Side target)
    {
        //InNeedChange判断是否需要更改摆位
        Vector2D PenaltyBallPos; //点球坐标
        Vector2D PenaltyDefenderGoaliePos; //守门员坐标
        Vector2D PenaltyAttakcPos; //进攻方的点球球员坐标
        //机器人所占区域
        RobotPosSquare[] DefenderPosSquare = new RobotPosSquare[5];
        RobotPosSquare[] OffensivePosSquare = new RobotPosSquare[5];
        RobotPosSquare[] SafePosSquare;
        ////防守方的安全区域点
        //RobotPosSquare[] PenaltyDefenderSafePosSquare;
        ////进攻方的安全区域点
        //RobotPosSquare[] PenaltyOffensiveSafePosSquare;
        int GoalieId;

        if (judgeResult.Actor == Side.Blue)
        {
            SafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(5f, 6f)),
                new RobotPosSquare(new Vector2D(5f, 16f)),
                new RobotPosSquare(new Vector2D(5f, 26f)),
                new RobotPosSquare(new Vector2D(5f, 36f)),
                new RobotPosSquare(new Vector2D(5f, 46f)),
                new RobotPosSquare(new Vector2D(5f, -6f)),
                new RobotPosSquare(new Vector2D(5f, -16f)),
                new RobotPosSquare(new Vector2D(5f, -26f)),
                new RobotPosSquare(new Vector2D(5f, -36f)),
                new RobotPosSquare(new Vector2D(5f, -46f))
            };
            //蓝方执行点球时相关坐标
            PenaltyBallPos = new Vector2D(-72.5f, 0f); //点球坐标
            PenaltyDefenderGoaliePos = new Vector2D(-106f, 0f); //守门员坐标
            PenaltyAttakcPos = new Vector2D(-50f, 0f); //进攻点球员坐标
            //PenaltyDefenderSafePosSquare = new RobotPosSquare[5] {  //黄方的安全区域点
            //new RobotPosSquare(new Vector2D(5f, 6f)),
            //new RobotPosSquare(new Vector2D(5f, 16f)),
            //new RobotPosSquare(new Vector2D(5f, 26f)),
            //new RobotPosSquare(new Vector2D(5f, 36f)),
            //new RobotPosSquare(new Vector2D(5f, 46f)) };

            ////蓝方的安全区域点
            //PenaltyOffensiveSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(5f, -6f)),
            //    new RobotPosSquare(new Vector2D(5f, -16f)),
            //    new RobotPosSquare(new Vector2D(5f, -26f)),
            //    new RobotPosSquare(new Vector2D(5f, -36f)),
            //    new RobotPosSquare(new Vector2D(5f, -46f)) };

            //JudgeSatePosOverlap(PenaltyOffensiveSafePosSquare, PenaltyDefenderSafePosSquare, Side.Blue, Side.Yellow);
            GoalieId = FindGoalie(Side.Yellow);
            UpdateOffAndDefInfo(DefenderPosSquare, OffensivePosSquare, Side.Blue);
        }
        else
        {
            SafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-5f, 5f)),
                new RobotPosSquare(new Vector2D(-5f, 20f)),
                new RobotPosSquare(new Vector2D(-5f, 35f)),
                new RobotPosSquare(new Vector2D(-5f, 50f)),
                new RobotPosSquare(new Vector2D(-5f, 65f)),
                new RobotPosSquare(new Vector2D(-5f, -5f)),
                new RobotPosSquare(new Vector2D(-5f, -20f)),
                new RobotPosSquare(new Vector2D(-5f, -35f)),
                new RobotPosSquare(new Vector2D(-5f, -50f)),
                new RobotPosSquare(new Vector2D(-5f, -65f))
            };
            //黄方执行点球时相关坐标
            PenaltyBallPos = new Vector2D(72.5f, 0f); //点球坐标
            PenaltyDefenderGoaliePos = new Vector2D(106f, 0f); //守门员坐标
            PenaltyAttakcPos = new Vector2D(50f, 0f); //进攻点球员坐标
            ////黄方的安全区域点
            //PenaltyDefenderSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(-5f, -6f)),
            //    new RobotPosSquare(new Vector2D(-5f, -16f)),
            //    new RobotPosSquare(new Vector2D(-5f, -26f)),
            //    new RobotPosSquare(new Vector2D(-5f, -36f)),
            //    new RobotPosSquare(new Vector2D(-5f, -46f)) };

            ////蓝方的安全区域点
            //PenaltyOffensiveSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(-5f, 6f)),
            //    new RobotPosSquare(new Vector2D(-5f, 16f)),
            //    new RobotPosSquare(new Vector2D(-5f, 26f)),
            //    new RobotPosSquare(new Vector2D(-5f, 36f)),
            //    new RobotPosSquare(new Vector2D(-5f, 46f)) };

            //JudgeSatePosOverlap(PenaltyOffensiveSafePosSquare, PenaltyDefenderSafePosSquare, Side.Yellow, Side.Blue);
            GoalieId = FindGoalie(Side.Blue);
            UpdateOffAndDefInfo(DefenderPosSquare, OffensivePosSquare, Side.Yellow);
        }

        //刚开始先来一个检测
        JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
        BallPos = PenaltyBallPos;
        //下面if是对第一次检测
        if (judgeResult.WhoisFirst == target)
        {
            if (GoalieId == -1)
            {
                DefenderPosSquare[0].GetNewPosSquare(PenaltyDefenderGoaliePos);
                JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
                GoalieId = 0;
            }
            else
            {
                //如果守门员没有压在球门线，将其压线
                if (judgeResult.Actor == Side.Blue)
                {
                    if (DefenderPosSquare[GoalieId].Pos.x >= -106 || DefenderPosSquare[GoalieId].Pos.x <= -120)
                    {
                        DefenderPosSquare[GoalieId].GetNewPosSquare
                            (new Vector2D(-106, DefenderPosSquare[GoalieId].Pos.y));
                    }
                }
                else
                {
                    if (DefenderPosSquare[GoalieId].Pos.x <= 106 || DefenderPosSquare[GoalieId].Pos.x >= 120)
                    {
                        DefenderPosSquare[GoalieId].GetNewPosSquare
                            (new Vector2D(106, DefenderPosSquare[GoalieId].Pos.y));
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
                    //两种不规范情况
                    //1.位置在防守方半场内 2. 未在球场内
                    if (DefenderPosSquare[i].square.IsOverlapWithRectangle(defenderHalfState)
                        || !DefenderPosSquare[i].square.IsOverlapWithRectangle(stadiumState))
                    {
                        ChangeRobotSafePos(ref DefenderPosSquare[i], SafePosSquare);
                        JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
                    }
                }
            }
            //对防守方摆位规范完成后再进行自我检测，有没有与自己重叠
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (RobotCrossByRobots(DefenderPosSquare[i], DefenderPosSquare, true, i))
                {
                    ChangeRobotSafePos(ref DefenderPosSquare[i], SafePosSquare);
                    JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
                }
            }
        }
        //TODO以后考虑之前第一次摆位是否合法
        else
        {
            //对进攻球员选择
            int attackRobotID = -1;
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                //只允许有一个进攻球员在防守半场中
                if (OffensivePosSquare[i].square.IsOverlapWithRectangle(defenderHalfState) && attackRobotID == -1)
                {
                    attackRobotID = i;
                    break;
                }
            }
            if (attackRobotID == -1)
            {
                attackRobotID = 1;
                OffensivePosSquare[1].GetNewPosSquare(PenaltyAttakcPos);
                JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
            }

            //对进攻球员与球检测，不能重叠
            if (OffensivePosSquare[attackRobotID].square.ContainsPoint(BallPos))
            {
                OffensivePosSquare[attackRobotID].GetNewPosSquare(PenaltyAttakcPos);
                JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
            }

            //再对进攻方进行检测
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                //对进攻的特定球员跳过
                if (i == attackRobotID)
                {
                    continue;
                }

                // 三种不规范情况:
                //1.在防守方半场 2.未在球场内 3.与之前摆完位的球员发生了重叠
                if (OffensivePosSquare[i].square.IsOverlapWithRectangle(defenderHalfState)
                    || !OffensivePosSquare[i].square.IsOverlapWithRectangle(stadiumState)
                    || RobotCrossByRobots(OffensivePosSquare[i], DefenderPosSquare))
                {
                    ChangeRobotSafePos(ref OffensivePosSquare[i], SafePosSquare);
                    JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
                }
            }

            //最后进攻方再和自己队球员进行判断
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (RobotCrossByRobots(OffensivePosSquare[i], OffensivePosSquare, true, i))
                {
                    ChangeRobotSafePos(ref OffensivePosSquare[i], SafePosSquare);
                    JudgeSafePosSquare(SafePosSquare, OffensivePosSquare, DefenderPosSquare);
                }
            }
        }
        if (judgeResult.Actor == Side.Blue)
        {
            UpdatePlacementPos(OffensivePosSquare, DefenderPosSquare, matchInfo, Side.Blue);
        }
        else
        {
            UpdatePlacementPos(OffensivePosSquare, DefenderPosSquare, matchInfo, Side.Yellow);
        }
    }


    private void JudgePlacePlacement(JudgeResult judgeResult, Side target)
    {
        //由于开球情况特殊，进攻方防守方安全区域不是同一个地方，即采用进攻方防守方两个安全区域

        Vector2D PlaceBallPos = new Vector2D(0, 0); //开球坐标
        //防守方的安全区域点
        RobotPosSquare[] PlaceDefenderSafePosSquare;
        //进攻方的安全区域点
        RobotPosSquare[] PlaceOffensiveSafePosSquare;
        //RobotPosSquare[] PlaceSafePosSquare;
        //进攻方机器人开球点
        Vector2D PlaceOffensivePos = new Vector2D(0, 12);

        RobotPosSquare[] PlaceDefenderPosSquare = new RobotPosSquare[5];
        RobotPosSquare[] PlaceOffensivePosSquare = new RobotPosSquare[5];
        if (judgeResult.Actor == Side.Blue)
        {
            //PlaceSafePosSquare = new RobotPosSquare[10] {
            //    new RobotPosSquare(new Vector2D(32, 0)),
            //    new RobotPosSquare(new Vector2D(32, 20)),
            //    new RobotPosSquare(new Vector2D(32,40)),
            //    new RobotPosSquare(new Vector2D(32,-20)),
            //    new RobotPosSquare(new Vector2D(32,-40)),
            //    new RobotPosSquare(new Vector2D(-32, 0)),
            //    new RobotPosSquare(new Vector2D(-32, 20)),
            //    new RobotPosSquare(new Vector2D(-32,40)),
            //    new RobotPosSquare(new Vector2D(-32,-20)),
            //    new RobotPosSquare(new Vector2D(-32,-40))};

            PlaceOffensiveSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(32, 0)),
                new RobotPosSquare(new Vector2D(32, 15)),
                new RobotPosSquare(new Vector2D(32, -15)),
                new RobotPosSquare(new Vector2D(32, 30)),
                new RobotPosSquare(new Vector2D(32, -30)),
                new RobotPosSquare(new Vector2D(32, 45)),
                new RobotPosSquare(new Vector2D(32, -45)),
                new RobotPosSquare(new Vector2D(32, 60)),
                new RobotPosSquare(new Vector2D(32, -60)),
                new RobotPosSquare(new Vector2D(45, 0))
            };

            PlaceDefenderSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-32, 0)),
                new RobotPosSquare(new Vector2D(-32, 15)),
                new RobotPosSquare(new Vector2D(-32, -15)),
                new RobotPosSquare(new Vector2D(-32, 30)),
                new RobotPosSquare(new Vector2D(-32, -30)),
                new RobotPosSquare(new Vector2D(-32, 45)),
                new RobotPosSquare(new Vector2D(-32, -45)),
                new RobotPosSquare(new Vector2D(-32, 60)),
                new RobotPosSquare(new Vector2D(-32, -60)),
                new RobotPosSquare(new Vector2D(-45, 0))
            };

            UpdateOffAndDefInfo(PlaceDefenderPosSquare, PlaceOffensivePosSquare, Side.Blue);
            JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
            JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
        }
        else
        {
            PlaceDefenderSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(32, 0)),
                new RobotPosSquare(new Vector2D(32, 15)),
                new RobotPosSquare(new Vector2D(32, -15)),
                new RobotPosSquare(new Vector2D(32, 30)),
                new RobotPosSquare(new Vector2D(32, -30)),
                new RobotPosSquare(new Vector2D(32, 45)),
                new RobotPosSquare(new Vector2D(32, -45)),
                new RobotPosSquare(new Vector2D(32, 60)),
                new RobotPosSquare(new Vector2D(32, -60)),
                new RobotPosSquare(new Vector2D(45, 0))
            };

            PlaceOffensiveSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-32, 0)),
                new RobotPosSquare(new Vector2D(-32, 15)),
                new RobotPosSquare(new Vector2D(-32, -15)),
                new RobotPosSquare(new Vector2D(-32, 30)),
                new RobotPosSquare(new Vector2D(-32, -30)),
                new RobotPosSquare(new Vector2D(-32, 45)),
                new RobotPosSquare(new Vector2D(-32, -45)),
                new RobotPosSquare(new Vector2D(-32, 60)),
                new RobotPosSquare(new Vector2D(-32, -60)),
                new RobotPosSquare(new Vector2D(-45, 0))
            };

            UpdateOffAndDefInfo(PlaceDefenderPosSquare, PlaceOffensivePosSquare, Side.Yellow);
            JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
            JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
        }

        BallPos = PlaceBallPos;
        int PlaceAttackId = -1;
        if(judgeResult.WhoisFirst == target)
        {
            //寻找开球球员
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (PlaceOffensivePosSquare[i].square.OverlapWithCircle(new Vector2D(0, 0), 25))
                {
                    PlaceAttackId = i;
                    break;
                }
            }
            //未找到进攻球员，则选取一个放入里面进攻
            if (PlaceAttackId == -1)
            {
                PlaceAttackId = 1;
                PlaceOffensivePosSquare[PlaceAttackId].GetNewPosSquare(PlaceOffensivePos);
                JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
            }
            //再检测开球球员是否与球重叠
            if (PlaceOffensivePosSquare[PlaceAttackId].square.ContainsPoint(BallPos))
            {
                PlaceOffensivePosSquare[PlaceAttackId].GetNewPosSquare(PlaceOffensivePos);
                JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
            }

            //先对进攻方摆位判断
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (i == PlaceAttackId) continue;
                //两种不规范情况：
                //1.不能再进入开球圆区域 2. 不能进入防守方半场 3.未在球场内
                if (PlaceOffensivePosSquare[i].square.OverlapWithCircle(new Vector2D(0, 0), 25)
                    || PlaceOffensivePosSquare[i].square.IsOverlapWithRectangle(defenderHalfState)
                    || !PlaceOffensivePosSquare[i].square.IsOverlapWithRectangle(stadiumState))
                {
                    ChangeRobotSafePos(ref PlaceOffensivePosSquare[i], PlaceOffensiveSafePosSquare);
                    JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                    JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                }
            }

            //再对自身摆位判断没有重叠
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (RobotCrossByRobots(PlaceOffensivePosSquare[i], PlaceOffensivePosSquare, true, i))
                {
                    ChangeRobotSafePos(ref PlaceOffensivePosSquare[i], PlaceOffensiveSafePosSquare);
                    JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                    JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                }
            }

        }
        //TODO考虑第一次进攻有没有合法
        //再对防守方摆位检测
        //需要注意的是，不需要和对面球员检测是否有重叠
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                //三种不规范情况
                //1.进入开球圆区域内 2.不能进入进攻方半场 3.未在球场内
                if (PlaceDefenderPosSquare[i].square.OverlapWithCircle(new Vector2D(0, 0), 25)
                    || PlaceDefenderPosSquare[i].square.IsOverlapWithRectangle(offensiveHalfState)
                    || !PlaceDefenderPosSquare[i].square.IsOverlapWithRectangle(stadiumState))
                {
                    ChangeRobotSafePos(ref PlaceDefenderPosSquare[i], PlaceDefenderSafePosSquare);
                    JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                    JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                }
            }

            //再对防守方自身进行检测
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (RobotCrossByRobots(PlaceDefenderPosSquare[i], PlaceDefenderPosSquare, true, i))
                {
                    ChangeRobotSafePos(ref PlaceDefenderPosSquare[i], PlaceDefenderSafePosSquare);
                    JudgeSafePosSquare(PlaceDefenderSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                    JudgeSafePosSquare(PlaceOffensiveSafePosSquare, PlaceOffensivePosSquare, PlaceDefenderPosSquare);
                }
            }

        }
        if (judgeResult.Actor == Side.Blue)
        {
            UpdatePlacementPos(PlaceOffensivePosSquare, PlaceDefenderPosSquare, matchInfo, Side.Blue);
        }
        else
        {
            UpdatePlacementPos(PlaceOffensivePosSquare, PlaceDefenderPosSquare, matchInfo, Side.Yellow);
        }
    }

    private void JudgeGoaliePlacement(JudgeResult judgeResult,Side target)
    {
        //下面是默认的球的坐标
        Vector2D GoalieBallPos;
        //门球判断需要注意的是，球的坐标由玩家摆位，不可以与球重叠，需要设置三个守门员的坐标点来避免重叠
        RobotPosSquare[] GoaliePosSafeSquare;
        //防守方的安全区域点
        RobotPosSquare[] GoalieDefenderSafePosSquare;
        //进攻方的安全区域点
        RobotPosSquare[] GoalieOffensiveSafePosSquare;

        RobotPosSquare[] GoalieDefenderPosSquare = new RobotPosSquare[5];
        RobotPosSquare[] GoalieOffensivePosSquare = new RobotPosSquare[5];
        int GoalieId;
        if (judgeResult.Actor == Side.Blue)
        {
            GoalieBallPos = new Vector2D(98f, -20f);

            GoaliePosSafeSquare = new RobotPosSquare[3]
            {
                new RobotPosSquare(new Vector2D(105, 0)),
                new RobotPosSquare(new Vector2D(105, -12)),
                new RobotPosSquare(new Vector2D(105, 12))
            };

            GoalieOffensiveSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(10, 0)),
                new RobotPosSquare(new Vector2D(10, 15)),
                new RobotPosSquare(new Vector2D(10, -15)),
                new RobotPosSquare(new Vector2D(10, 30)),
                new RobotPosSquare(new Vector2D(10, -30)),
                new RobotPosSquare(new Vector2D(10, 45)),
                new RobotPosSquare(new Vector2D(10, -45)),
                new RobotPosSquare(new Vector2D(10, 60)),
                new RobotPosSquare(new Vector2D(10, -60)),
                new RobotPosSquare(new Vector2D(25, 0))
            };

            GoalieDefenderSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-10, 0)),
                new RobotPosSquare(new Vector2D(-10, 15)),
                new RobotPosSquare(new Vector2D(-10, -15)),
                new RobotPosSquare(new Vector2D(-10, 30)),
                new RobotPosSquare(new Vector2D(-10, -30)),
                new RobotPosSquare(new Vector2D(-10, 45)),
                new RobotPosSquare(new Vector2D(-10, -45)),
                new RobotPosSquare(new Vector2D(-10, 60)),
                new RobotPosSquare(new Vector2D(-10, -60)),
                new RobotPosSquare(new Vector2D(-25, 0))
            };

            UpdateOffAndDefInfo(GoalieDefenderPosSquare, GoalieOffensivePosSquare, Side.Blue);
            JudgeSafePosSquare(GoalieOffensiveSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
            JudgeSafePosSquare(GoalieDefenderSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
            GoalieId = FindGoalie(Side.Blue);
        }
        else
        {
            GoalieBallPos = new Vector2D(-98f, 20f);

            GoaliePosSafeSquare = new RobotPosSquare[3]
            {
                new RobotPosSquare(new Vector2D(-105, 0)),
                new RobotPosSquare(new Vector2D(-105, -12)),
                new RobotPosSquare(new Vector2D(-105, 12))
            };

            GoalieDefenderSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(10, 0)),
                new RobotPosSquare(new Vector2D(10, 15)),
                new RobotPosSquare(new Vector2D(10, -15)),
                new RobotPosSquare(new Vector2D(10, 30)),
                new RobotPosSquare(new Vector2D(10, -30)),
                new RobotPosSquare(new Vector2D(10, 45)),
                new RobotPosSquare(new Vector2D(10, -45)),
                new RobotPosSquare(new Vector2D(10, 60)),
                new RobotPosSquare(new Vector2D(10, -60)),
                new RobotPosSquare(new Vector2D(25, 0))
            };

            GoalieOffensiveSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-10, 0)),
                new RobotPosSquare(new Vector2D(-10, 15)),
                new RobotPosSquare(new Vector2D(-10, -15)),
                new RobotPosSquare(new Vector2D(-10, 30)),
                new RobotPosSquare(new Vector2D(-10, -30)),
                new RobotPosSquare(new Vector2D(-10, 45)),
                new RobotPosSquare(new Vector2D(-10, -45)),
                new RobotPosSquare(new Vector2D(-10, 60)),
                new RobotPosSquare(new Vector2D(-10, -60)),
                new RobotPosSquare(new Vector2D(-25, 0))
            };

            UpdateOffAndDefInfo(GoalieDefenderPosSquare, GoalieOffensivePosSquare, Side.Yellow);
            JudgeSafePosSquare(GoalieOffensiveSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
            JudgeSafePosSquare(GoalieDefenderSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
            GoalieId = FindGoalie(Side.Yellow);
        }
        if(judgeResult.WhoisFirst == target)
        {
            //先对球进行摆位判断
            if (!offensiveSmallState.ContainsPoint(BallPos))
            {
                BallPos = GoalieBallPos;
            }
            //没有守门员的话，放置守门员
            if (GoalieId == -1)
            {
                GoalieId = 0;
                GoalieOffensivePosSquare[GoalieId].GetNewPosSquare(GoaliePosSafeSquare[0].Pos);
                GoaliePosSafeSquare[0].occupy = true;
                JudgeSafePosSquare(GoalieOffensiveSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                JudgeSafePosSquare(GoalieDefenderSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
            }
            //下一步对守门员与球判断有没有重叠
            for (int i = 0; i < 3; i++)
            {
                if (GoalieOffensivePosSquare[GoalieId].square.ContainsPoint(BallPos))
                {
                    GoalieOffensivePosSquare[GoalieId].GetNewPosSquare(GoaliePosSafeSquare[i].Pos);
                    JudgeSafePosSquare(GoalieOffensiveSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                    JudgeSafePosSquare(GoalieDefenderSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                }
                else
                    break;
            }
            //对进攻方摆位判断
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (i == GoalieId) continue;
                //两种情况不规范
                //需要注意的是：进攻方可以到达对方半场，不受限制
                //1.未在球场内 2.与自身重叠
                if (!GoalieOffensivePosSquare[i].square.IsOverlapWithRectangle(stadiumState)
                    || RobotCrossByRobots(GoalieOffensivePosSquare[i], GoalieOffensivePosSquare, true, i))
                {
                    ChangeRobotSafePos(ref GoalieOffensivePosSquare[i], GoalieOffensiveSafePosSquare);
                    JudgeSafePosSquare(GoalieOffensiveSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                    JudgeSafePosSquare(GoalieDefenderSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                }
            }
        }
        //TODO考虑第一次有没有规范正确
        else
        {
            //接着对防守方摆位判断
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                //三种情况不规范
                //1.到对方半场，2.未在球场内 3.与自身重叠 4.与敌方重叠
                if (GoalieDefenderPosSquare[i].square.IsOverlapWithRectangle(offensiveHalfState)
                    || !GoalieDefenderPosSquare[i].square.IsOverlapWithRectangle(stadiumState)
                    || RobotCrossByRobots(GoalieDefenderPosSquare[i], GoalieDefenderPosSquare, true, i)
                    || RobotCrossByRobots(GoalieDefenderPosSquare[i], GoalieOffensivePosSquare))
                {
                    ChangeRobotSafePos(ref GoalieDefenderPosSquare[i], GoalieDefenderSafePosSquare);
                    JudgeSafePosSquare(GoalieOffensiveSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                    JudgeSafePosSquare(GoalieDefenderSafePosSquare, GoalieOffensivePosSquare, GoalieDefenderPosSquare);
                }
            }
        }
        if (judgeResult.Actor == Side.Blue)
        {
            UpdatePlacementPos(GoalieOffensivePosSquare, GoalieDefenderPosSquare, matchInfo, Side.Blue);
        }
        else
        {
            UpdatePlacementPos(GoalieOffensivePosSquare, GoalieDefenderPosSquare, matchInfo, Side.Yellow);
        }
    }

    private void JudgeFreePlacement(JudgeResult judgeResult,Side target)
    {
        //下面是默认的球的坐标
        Vector2D FreeBallPos;
        //防守方守门员的坐标
        Vector2D FreeOffensiveGoaliePos;
        //争球区域其他球员不可进入

        //争球情况特有的情况
        //进攻方防守方在争球点进行争球
        int OffensiveFreeId;
        int DefenderFreeId;
        Vector2D FreeOffensivePos;
        Vector2D FreeDefenderPos;

        ////争球安全区域有两个部分组成，黄蓝方以此交替放在两部分里
        ////防守方的安全区域点
        //RobotPosSquare[] FreeDefenderSafePosSquare;
        ////进攻方的安全区域点
        //RobotPosSquare[] FreeOffensiveSafePosSquare;

        //争球安全区域
        RobotPosSquare[] FreeSafePosSquare;

        RobotPosSquare[] FreeDefenderPosSquare = new RobotPosSquare[5];
        RobotPosSquare[] FreeOffensivePosSquare = new RobotPosSquare[5];
        int GoalieId;
        if (judgeResult.ResultType == ResultType.FreeKickLeftTop)
        {
            FreeOffensiveGoaliePos = new Vector2D(-102.5f, 0f);
            GoalieId = FindGoalie(Side.Yellow);
            FreeState = new UprightRectangle(-110, 0, 90, 0);
            FreeBallPos = new Vector2D(-55f, 60f);

            OffensiveFreeId = FindFreeRobotId(Side.Yellow, true, GoalieId);
            DefenderFreeId = FindFreeRobotId(Side.Blue);
            FreeOffensivePos = new Vector2D(-85f, 60f);
            FreeDefenderPos = new Vector2D(-25f, 60f);

            //安全区域点
            FreeSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-80f, -10f)),
                new RobotPosSquare(new Vector2D(10f, 65f)),
                new RobotPosSquare(new Vector2D(-50f, -10f)),
                new RobotPosSquare(new Vector2D(10f, 35f)),
                new RobotPosSquare(new Vector2D(-20f, -10f)),
                new RobotPosSquare(new Vector2D(10f, 80f)),
                new RobotPosSquare(new Vector2D(-65f, -10f)),
                new RobotPosSquare(new Vector2D(10f, 50f)),
                new RobotPosSquare(new Vector2D(-35f, -10f)),
                new RobotPosSquare(new Vector2D(10f, 20f))
            };

            //蓝方的安全区域点
            //FreeOffensiveSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(10f, 80f)),
            //    new RobotPosSquare(new Vector2D(-65f, -10f)),
            //    new RobotPosSquare(new Vector2D(10f, 50f)),
            //    new RobotPosSquare(new Vector2D(-35f, -10f)),
            //    new RobotPosSquare(new Vector2D(10f, 20f))};
            UpdateOffAndDefInfo(FreeDefenderPosSquare, FreeOffensivePosSquare, Side.Yellow);
            JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
        }
        else if (judgeResult.ResultType == ResultType.FreeKickLeftBot)
        {
            FreeOffensiveGoaliePos = new Vector2D(-102.5f, 0f);
            GoalieId = FindGoalie(Side.Yellow);
            FreeState = new UprightRectangle(-110, 0, 0, -90);
            FreeBallPos = new Vector2D(-55f, -60f);

            OffensiveFreeId = FindFreeRobotId(Side.Yellow, true, GoalieId);
            DefenderFreeId = FindFreeRobotId(Side.Blue);
            FreeOffensivePos = new Vector2D(-85f, -60f);
            FreeDefenderPos = new Vector2D(-25f, -60f);

            //安全区域点
            FreeSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(-10f, 10f)),
                new RobotPosSquare(new Vector2D(10f, -65f)),
                new RobotPosSquare(new Vector2D(-40f, 10f)),
                new RobotPosSquare(new Vector2D(10f, -35f)),
                new RobotPosSquare(new Vector2D(-70f, 10f)),
                new RobotPosSquare(new Vector2D(10f, -80f)),
                new RobotPosSquare(new Vector2D(-25f, 10f)),
                new RobotPosSquare(new Vector2D(10f, -50f)),
                new RobotPosSquare(new Vector2D(-55f, 10f)),
                new RobotPosSquare(new Vector2D(10f, -20f))
            };

            ////蓝方的安全区域点
            //FreeOffensiveSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(10f, -80f)),
            //    new RobotPosSquare(new Vector2D(-25f, 10f)),
            //    new RobotPosSquare(new Vector2D(10f, -50f)),
            //    new RobotPosSquare(new Vector2D(-55f, 10f)),
            //    new RobotPosSquare(new Vector2D(10f, -20f))};
            UpdateOffAndDefInfo(FreeDefenderPosSquare, FreeOffensivePosSquare, Side.Yellow);
            JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
        }
        else if (judgeResult.ResultType == ResultType.FreeKickRightTop)
        {
            FreeOffensiveGoaliePos = new Vector2D(102.5f, 0f);
            GoalieId = FindGoalie(Side.Blue);
            FreeState = new UprightRectangle(0, 110, 90, 0);
            FreeBallPos = new Vector2D(55f, 60f);

            OffensiveFreeId = FindFreeRobotId(Side.Blue, true, GoalieId);
            DefenderFreeId = FindFreeRobotId(Side.Yellow);
            FreeDefenderPos = new Vector2D(25f, 60f);
            FreeOffensivePos = new Vector2D(85f, 60f);

            //安全区域点
            FreeSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(10f, -10f)),
                new RobotPosSquare(new Vector2D(-10f, 65f)),
                new RobotPosSquare(new Vector2D(40f, -10f)),
                new RobotPosSquare(new Vector2D(-10f, 35f)),
                new RobotPosSquare(new Vector2D(70f, -10f)),
                new RobotPosSquare(new Vector2D(-10f, 80f)),
                new RobotPosSquare(new Vector2D(25f, -10f)),
                new RobotPosSquare(new Vector2D(-10f, 50f)),
                new RobotPosSquare(new Vector2D(45f, -10f)),
                new RobotPosSquare(new Vector2D(-10f, 20f))
            };

            ////黄方的安全区域点
            //FreeOffensiveSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(-10f, 80f)),
            //    new RobotPosSquare(new Vector2D(25f, -10f)),
            //    new RobotPosSquare(new Vector2D(-10f, 50f)),
            //    new RobotPosSquare(new Vector2D(45f, -10f)),
            //    new RobotPosSquare(new Vector2D(-10f, 20f))};
            UpdateOffAndDefInfo(FreeDefenderPosSquare, FreeOffensivePosSquare, Side.Blue);
            JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
        }
        else
        {
            FreeOffensiveGoaliePos = new Vector2D(102.5f, 0f);
            GoalieId = FindGoalie(Side.Blue);
            FreeState = new UprightRectangle(0, 110, 0, -90);
            FreeBallPos = new Vector2D(55f, -60f);

            OffensiveFreeId = FindFreeRobotId(Side.Blue, true, GoalieId);
            DefenderFreeId = FindFreeRobotId(Side.Yellow);
            FreeDefenderPos = new Vector2D(25f, -60f);
            FreeOffensivePos = new Vector2D(85f, -60f);

            //安全区域点
            FreeSafePosSquare = new RobotPosSquare[10]
            {
                new RobotPosSquare(new Vector2D(10f, 10f)),
                new RobotPosSquare(new Vector2D(-10f, -65f)),
                new RobotPosSquare(new Vector2D(40f, 10f)),
                new RobotPosSquare(new Vector2D(-10f, -35f)),
                new RobotPosSquare(new Vector2D(70f, 10f)),
                new RobotPosSquare(new Vector2D(-10f, -80f)),
                new RobotPosSquare(new Vector2D(25f, 10f)),
                new RobotPosSquare(new Vector2D(-10f, -50f)),
                new RobotPosSquare(new Vector2D(55f, 10f)),
                new RobotPosSquare(new Vector2D(-10f, -20f))
            };

            ////黄方的安全区域点
            //FreeOffensiveSafePosSquare = new RobotPosSquare[5] {
            //    new RobotPosSquare(new Vector2D(-10f, -80f)),
            //    new RobotPosSquare(new Vector2D(25f, 10f)),
            //    new RobotPosSquare(new Vector2D(-10f, -50f)),
            //    new RobotPosSquare(new Vector2D(55f, 10f)),
            //    new RobotPosSquare(new Vector2D(-10f, -20f))};
            UpdateOffAndDefInfo(FreeDefenderPosSquare, FreeOffensivePosSquare, Side.Blue);
            JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
        }
        //先对球进行摆位
        BallPos = FreeBallPos;
        if (judgeResult.WhoisFirst == target)
        {
            //再对防守方进行摆位判断
            //先寻找是否有争球球员
            if (DefenderFreeId == -1)
            {
                DefenderFreeId = 1;
                FreeDefenderPosSquare[DefenderFreeId].GetNewPosSquare(FreeDefenderPos);
                JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
            }

            //如果在争球半场内但没有在争球点
            if (FreeDefenderPosSquare[DefenderFreeId].Pos.IsNotNear(FreeDefenderPos))
            {
                FreeDefenderPosSquare[DefenderFreeId].GetNewPosSquare(FreeDefenderPos);
                JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
            }

            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (i == DefenderFreeId) continue;
                //三种不规范情况
                //1.除了进攻球员在争球区域内 2. 未在球场内 3. 与自身队伍球员重叠 
                if (FreeDefenderPosSquare[i].square.IsOverlapWithRectangle(FreeState)
                    || !FreeDefenderPosSquare[i].square.IsOverlapWithRectangle(stadiumState)
                    || RobotCrossByRobots(FreeDefenderPosSquare[i], FreeDefenderPosSquare, true, i)
                    )
                {
                    ChangeRobotSafePos(ref FreeDefenderPosSquare[i], FreeSafePosSquare);
                    JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
                }
            }
        }
        //TODO考虑第一次有没有规范争取
        else
        {
            //再寻找守门员
            if (GoalieId == -1)
           {
                //如果争球球员是0号，则不选择0号为守门员
                if(OffensiveFreeId == 0)
                {
                    GoalieId = 1;
                }
                else
                {
                    GoalieId = 0;
                }
                FreeOffensivePosSquare[GoalieId].GetNewPosSquare(FreeOffensiveGoaliePos);
                JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
            }
            //再对进攻方规范
            //如果没有进攻球员，则安排一个
            if (OffensiveFreeId == -1)
            {
                OffensiveFreeId = 1;
                FreeOffensivePosSquare[OffensiveFreeId].GetNewPosSquare(FreeOffensivePos);
                JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
            }
            //如果在争球半场内，但是没有规范位置，则放置摆位点
            if (FreeOffensivePosSquare[OffensiveFreeId].Pos.IsNotNear(FreeOffensivePos))
            {
                FreeOffensivePosSquare[OffensiveFreeId].GetNewPosSquare(FreeOffensivePos);
                JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
            }
            //再对进攻方先进行摆位判断
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (i == GoalieId) continue;
                if (i == OffensiveFreeId) continue;
                //三种不规范情况
                //1.除了进攻球员在争球区域内 2. 未在球场内 3. 与自身队伍球员重叠
                if (FreeOffensivePosSquare[i].square.IsOverlapWithRectangle(FreeState)
                    || !FreeOffensivePosSquare[i].square.IsOverlapWithRectangle(stadiumState)
                    || RobotCrossByRobots(FreeOffensivePosSquare[i], FreeOffensivePosSquare, true, i)
                    || RobotCrossByRobots(FreeOffensivePosSquare[i], FreeOffensivePosSquare))
                {
                    ChangeRobotSafePos(ref FreeOffensivePosSquare[i], FreeSafePosSquare);
                    JudgeSafePosSquare(FreeSafePosSquare, FreeOffensivePosSquare, FreeDefenderPosSquare);
                }
            }
        }
        if (judgeResult.Actor == Side.Blue)
        {
            UpdatePlacementPos(FreeOffensivePosSquare, FreeDefenderPosSquare, matchInfo, Side.Blue);
        }
        else
        {
            UpdatePlacementPos(FreeOffensivePosSquare, FreeDefenderPosSquare, matchInfo, Side.Yellow);
        }
    }

    /// <summary>
    /// 用来对争球进行判断，寻找争球球员,由于守门员的位置可能放在争球区域内，默认是不需要考虑守门员ID，
    /// </summary>
    private int FindFreeRobotId(Side side, bool HaveGoalie = false, int GoalieId = -1)
    {
        int FreeRobotId = -1;
        if (side == Side.Blue)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (HaveGoalie)
                {
                    if (i == GoalieId) continue;
                }

                if (FreeState.ContainsPoint(blueRobots[i].pos))
                {
                    FreeRobotId = i;
                    return FreeRobotId;
                }
            }
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (HaveGoalie)
                {
                    if (i == GoalieId) continue;
                }

                if (FreeState.ContainsPoint(yellowRobots[i].pos))
                {
                    FreeRobotId = i;
                    return FreeRobotId;
                }
            }
        }

        return -1;
    }

    private void ChangeRobotSafePos(ref RobotPosSquare robotPosSquare, RobotPosSquare[] robotsSafePosSquares)
    {
        for (int i = 0; i < 2 * Const.RobotsPerTeam; i++)
        {
            if (!robotsSafePosSquares[i].occupy)
            {
                robotPosSquare.GetNewPosSquare(robotsSafePosSquares[i].Pos);
                robotsSafePosSquares[i].occupy = true;
                break;
            }
        }
    }

    /// <summary>
    /// 默认情况是检测对面组别，第三个参数为true表示检测自身组别
    /// </summary>
    private bool RobotCrossByRobots(RobotPosSquare robotPosSquare, RobotPosSquare[] robotsPosSquare, bool self = false,
        int selfid = -1)
    {
        //默认是检测对面组别
        //如果是检查与对面组，对所有都检查
        if (!self)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (robotPosSquare.square.IsCrossedBy(robotsPosSquare[i].square))
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (i == selfid) continue;
                if (robotPosSquare.square.IsCrossedBy(robotsPosSquare[i].square))
                {
                    return true;
                }
            }

            return false;
        }
    }


    //更新进攻方和防守方信息
    private void UpdateOffAndDefInfo(RobotPosSquare[] DefenderPosSquare, RobotPosSquare[] OffensivePosSquare,
        Side Offensive)
    {
        if (Offensive == Side.Yellow)
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                DefenderPosSquare[i] = new RobotPosSquare(BlueRobotsPos[i], matchInfo.BlueRobots[i].rotation);
                OffensivePosSquare[i] = new RobotPosSquare(YellowRobotsPos[i], matchInfo.YellowRobots[i].rotation);
            }

            offensiveHalfState = yellowHalfState;
            defenderHalfState = blueHalfState;

            offensiveSmallState = yellowSmallState;
            defenderSmallState = blueSmallState;
        }
        else
        {
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                DefenderPosSquare[i] = new RobotPosSquare(YellowRobotsPos[i], matchInfo.YellowRobots[i].rotation);
                OffensivePosSquare[i] = new RobotPosSquare(BlueRobotsPos[i], matchInfo.BlueRobots[i].rotation);
            }

            offensiveHalfState = blueHalfState;
            defenderHalfState = yellowHalfState;

            offensiveSmallState = blueSmallState;
            defenderSmallState = yellowSmallState;
        }
    }

    private void UpdatePlacementPos(RobotPosSquare[] OffensiveRobotsPosSquare,
        RobotPosSquare[] DefensiveRobotsPosSquare, MatchInfo matchInfo, Side Offensive)
    {
        matchInfo.Ball.pos = BallPos;
        for (int i = 0; i < Const.RobotsPerTeam; i++)
        {
            if (Offensive == Side.Blue)
            {
                matchInfo.BlueRobots[i].pos = OffensiveRobotsPosSquare[i].Pos;
                matchInfo.YellowRobots[i].pos = DefensiveRobotsPosSquare[i].Pos;
            }
            else
            {
                matchInfo.YellowRobots[i].pos = OffensiveRobotsPosSquare[i].Pos;
                matchInfo.BlueRobots[i].pos = DefensiveRobotsPosSquare[i].Pos;
            }
        }
    }

    //New判断安全区占用问题
    private void JudgeSafePosSquare(RobotPosSquare[] SafePosSquare, RobotPosSquare[] OffensivePosSquare,
        RobotPosSquare[] DefenderPosSquare, int NumOfSafePos = 10)
    {
        for (int i = 0; i < NumOfSafePos; i++)
        {
            for (int j = 0; j < Const.RobotsPerTeam; j++)
            {
                if (SafePosSquare[i].square.IsCrossedBy(OffensivePosSquare[j].square) ||
                    SafePosSquare[i].square.IsCrossedBy(DefenderPosSquare[j].square))
                {
                    SafePosSquare[i].occupy = true;
                    break;
                }

                //当j=4时，到达这一步，说明五个都没有占用，将其置为false
                if (j == 4)
                {
                    SafePosSquare[i].occupy = false;
                }
            }
        }
    }

    //判断是否占用安全区域情况
    private void JudgeSatePosOverlap(RobotPosSquare[] OffensiveSafePos, RobotPosSquare[] DefenderSafePos,
        Side Offensive, Side Defender)
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
                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].square.IsCrossedBy(BlueRobotSquare[j]))
                    {
                        OffensiveSafePos[i].occupy = true;
                    }

                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].square.IsCrossedBy(YellowRobotSquare[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        OffensiveSafePos[i].Pos = DefenderSafePos[i].Pos;
                        DefenderSafePos[i].occupy = true;
                    }

                    //检验黄方安全区域
                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].square.IsCrossedBy(YellowRobotSquare[j]))
                    {
                        DefenderSafePos[i].occupy = true;
                    }

                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].square.IsCrossedBy(BlueRobotSquare[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        DefenderSafePos[i].Pos = OffensiveSafePos[i].Pos;
                        OffensiveSafePos[i].occupy = true;
                    }
                }
                //进攻方是黄方时
                else
                {
                    //检验黄方安全区域
                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].square.IsCrossedBy(YellowRobotSquare[j]))
                    {
                        OffensiveSafePos[i].occupy = true;
                    }

                    if (!OffensiveSafePos[i].occupy && OffensiveSafePos[i].square.IsCrossedBy(BlueRobotSquare[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        OffensiveSafePos[i].Pos = DefenderSafePos[i].Pos;
                        DefenderSafePos[i].occupy = true;
                    }

                    //检验蓝方安全区域
                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].square.IsCrossedBy(BlueRobotSquare[j]))
                    {
                        DefenderSafePos[i].occupy = true;
                    }

                    if (!DefenderSafePos[i].occupy && DefenderSafePos[i].square.IsCrossedBy(YellowRobotSquare[j]))
                    {
                        //如果一方安全区被另一方占用，抢夺另一方对应的安全区
                        DefenderSafePos[i].Pos = OffensiveSafePos[i].Pos;
                        OffensiveSafePos[i].occupy = true;
                    }
                }
            }
        }
    }
}

namespace TestReferee
{
    [TestFixture]
    public class TestPlacement
    {
        public GameObject entity;
        public MouseDrag mouseDrag;
        ObjectManager objectManager;
        MatchInfo matchInfo;
        MatchInfo preMatchInfo;
        RefereeTestMain refereeTestMain = new RefereeTestMain();

        public bool ComparePlaceMatchInfo(MatchInfo matchInfo1, MatchInfo matchInfo2)
        {
            if (!matchInfo1.Ball.pos.Equals(matchInfo2.Ball.pos))
                return false;
            for (int i = 0; i < Const.RobotsPerTeam; i++)
            {
                if (!matchInfo1.BlueRobots[i].Equals(matchInfo2.BlueRobots[i]) ||
                    !matchInfo2.YellowRobots[i].Equals(matchInfo2.YellowRobots[i]))
                    return false;
            }

            return true;
        }

        [Test]
        public void TestAuto()
        {
            ResultType[] resultTypes = Enum.GetValues(typeof(ResultType)) as ResultType[];
            Side[] sides = Enum.GetValues(typeof(Side)) as Side[];
            Random random = new Random();
            int testtimes = 1000;
            int NeedChange = 0;
            for (int i = 0; i < testtimes; i++)
            {
                matchInfo = refereeTestMain.RandomMatchInfo();
                preMatchInfo = matchInfo;
                JudgeResult judgeResult = new JudgeResult
                {
                    //产生1-2
                    Actor = sides[random.Next(1, 3)],
                    //产生3-9
                    ResultType = resultTypes[random.Next(3, 10)]
                };

                if (judgeResult.ResultType == ResultType.FreeKickLeftBot)
                {
                    judgeResult.Actor = Side.Yellow;
                }

                if (judgeResult.ResultType == ResultType.FreeKickLeftTop)
                {
                    judgeResult.Actor = Side.Yellow;
                }

                if (judgeResult.ResultType == ResultType.FreeKickRightBot)
                {
                    judgeResult.Actor = Side.Blue;
                }

                if (judgeResult.ResultType == ResultType.FreeKickRightBot)
                {
                    judgeResult.Actor = Side.Blue;
                }

                matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult,Side.Nobody);
                //出现错误，输入到文件中
                if (!ComparePlaceMatchInfo(preMatchInfo, matchInfo))
                {
                    NeedChange++;
                    var failInfo = JsonConvert.SerializeObject(preMatchInfo);
                    string filepath = "H:\\V5++\\UnityProject\\Bug\\" + judgeResult.ResultType + "--" +
                                      judgeResult.Actor +
                                      $"{DateTime.Now:yyyy-MM-dd_hhmmss}" + "--" + i + ".json";
                    FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(fs);
                    fs.SetLength(0);
                    sw.Write(failInfo);
                    sw.Close();
                }
                else
                {
                    Debug.Log(i + "次测试正确");
                }
            }

            Debug.Log("错误率为：" + NeedChange / testtimes);
            //错误率小于0.1
            Assert.IsTrue(NeedChange / testtimes < 0.1);
        }

        //[Test]
        //public void TestPenaltyPlacement1()
        //{
        //    var referee = new Referee();
        //    var matchInfo = new MatchInfo()
        //    {
        //        Ball = new Ball() { pos = new Vector2D(0, 0) },
        //        BlueRobots = new Robot[]
        //        {
        //        new Robot() { pos = new Vector2D(-72.5f, 0f) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        },
        //        YellowRobots = new Robot[]
        //        {
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        },
        //    };
        //    var judgeResult = new JudgeResult()
        //    {
        //        Actor = Side.Blue,
        //        ResultType = ResultType.PenaltyKick,
        //    };
        //    //该情况测试 点球摆位时，与球重叠后把球员移开
        //    referee.JudgeAutoPlacement(matchInfo, judgeResult);

        //    Assert.AreEqual(new Vector2D(-50f, 0f), matchInfo.BlueRobots[0].pos);
        //}

        //[Test]
        //public void TestPenaltyPlacement2()
        //{
        //    var referee = new Referee();
        //    var matchInfo = new MatchInfo()
        //    {
        //        Ball = new Ball() { pos = new Vector2D(0, 0) },
        //        BlueRobots = new Robot[]
        //        {
        //            //没有球员在守门员内
        //        new Robot() { pos = new Vector2D(100, 50) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        },
        //        YellowRobots = new Robot[]
        //        {
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        new Robot() { pos = new Vector2D(0, 0) },
        //        },
        //    };
        //    var judgeResult = new JudgeResult()
        //    {
        //        Actor = Side.Yellow,
        //        ResultType = ResultType.PenaltyKick,
        //    };
        //    //该情况测试 点球摆位时，与球重叠后把球员移开
        //    referee.JudgeAutoPlacement(matchInfo, judgeResult);

        //    Assert.AreEqual(new Vector2D(106f, 0f), matchInfo.BlueRobots[0].pos);
        //}

        //[Test]
        //public void TestPlacePlacement()
        //{
        //    var referee = new Referee();
        //    var matchInfo = new MatchInfo()
        //    {
        //        Ball = new Ball() { pos = new Vector2D(0, 0) },
        //        BlueRobots = new Robot[]
        //        {
        //            //测试机器人0号与球重叠
        //        new Robot() { pos = new Vector2D(0f, 0f) },
        //        new Robot() { pos = new Vector2D(30, 0) },
        //        new Robot() { pos = new Vector2D(40, 0) },
        //        new Robot() { pos = new Vector2D(50, 0) },
        //        new Robot() { pos = new Vector2D(60, 0) },
        //        },
        //        YellowRobots = new Robot[]
        //        {
        //        new Robot() { pos = new Vector2D(-10, 0) },
        //        new Robot() { pos = new Vector2D(-20, 0) },
        //        new Robot() { pos = new Vector2D(-30, 0) },
        //        new Robot() { pos = new Vector2D(-40, 0) },
        //        new Robot() { pos = new Vector2D(-50, 0) },
        //        },
        //    };
        //    var judgeResult = new JudgeResult()
        //    {
        //        Actor = Side.Blue,
        //        ResultType = ResultType.PlaceKick,
        //    };
        //    //该情况测试 开球摆位时，与球重叠后把球员移开
        //    referee.JudgeAutoPlacement(matchInfo, judgeResult);

        //    Assert.AreEqual(new Vector2D(0, 12), matchInfo.BlueRobots[0].pos);
        //}


        //[Test]
        //public void TestFreePlacement()
        //{
        //    var referee = new Referee();
        //    var matchInfo = new MatchInfo()
        //    {
        //        Ball = new Ball() { pos = new Vector2D(0, 0) },
        //        BlueRobots = new Robot[]
        //        {
        //            //测试机器人0号与球重叠
        //        new Robot() { pos = new Vector2D(0f, 0f) },
        //        new Robot() { pos = new Vector2D(30, 0) },
        //        new Robot() { pos = new Vector2D(40, 0) },
        //        new Robot() { pos = new Vector2D(50, 0) },
        //        new Robot() { pos = new Vector2D(60, 0) },
        //        },
        //        YellowRobots = new Robot[]
        //        {
        //        new Robot() { pos = new Vector2D(-10, 0) },
        //        new Robot() { pos = new Vector2D(-20, 0) },
        //        new Robot() { pos = new Vector2D(-30, 0) },
        //        new Robot() { pos = new Vector2D(-40, 0) },
        //        new Robot() { pos = new Vector2D(-50, 0) },
        //        },
        //    };
        //    var judgeResult = new JudgeResult()
        //    {
        //        Actor = Side.Blue,
        //        ResultType = ResultType.FreeKickLeftBot,
        //    };
        //    //该情况测试 开球摆位时，与球重叠后把球员移开
        //    referee.JudgeAutoPlacement(matchInfo, judgeResult);

        //    Assert.AreEqual(new Vector2D(-25f, -60f), matchInfo.BlueRobots[0].pos);
        //}

        //[UnityTest]
        //public IEnumerator TestScene()
        //{
        //    SceneManager.LoadScene("RefereePlaceTest");
        //    ObjectManager o = new ObjectManager();
        //    var go = GameObject.Find("/Entity");
        //    Debug.Assert(go != null, "go == null");
        //    o.RebindObject(go);
        //    o.RevertScene(new MatchInfo());
        //    while (true)
        //        yield return null;
        //}
    }
}
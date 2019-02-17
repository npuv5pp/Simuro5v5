using System;
using System.Collections;
using System.Linq;
using System.Text;
using Simuro5v5;
using System.Runtime.InteropServices;
using UnityEngine;
using Event = Simuro5v5.EventSystem.Event;


public enum PlayMode
{
    PM_PlayOn = 0,           //  
    PM_FreeBall_LeftTop,     //   1
    PM_FreeBall_LeftBot,     //   2 
    PM_FreeBall_RightTop,    //   3
    PM_FreeBall_RightBot,    //   4
    PM_PlaceKick_Yellow,     //   5
    PM_PlaceKick_Blue,       //   6
    PM_PenaltyKick_Yellow,   //   7
    PM_PenaltyKick_Blue,     //   8
    PM_FreeKick_Yellow,      //   9
    PM_FreeKick_Blue,        //   10
    PM_GoalKick_Yellow,      //   11
    PM_GoalKick_Blue         //   12
};


public struct StateSpace
{
    public Robot[] home;
    public OpponentRobot[] opp;
    public Ball currentBall;
    public PlayMode gameState;

    public StateSpace(SideInfo sideInfo)
    {
        home = sideInfo.home;
        opp = sideInfo.opp;
        currentBall = sideInfo.currentBall;
        gameState = new PlayMode();
    }
};


public class Referee
{
    // State
    private int ballslow;
    private int pushball;
    private int Foul_pushball;

    // Record
    private int recordtime1;
    private int recordtime2;
    private int recordtime3;
    private int recordtime4;
    private int recordrobot;
    private int recordzong;
    private int recordrobot3;
    private int recordrobot4;
    private int recordzong4;

    // Opp
    private int op_recordtime1;
    private int op_recordtime2;
    private int op_recordtime3;
    private int op_recordtime4;
    private int op_recordrobot;
    private int op_recordzong;
    private int op_recordrobot3;
    private int op_recordrobot4;
    private int op_recordzong4;

    // Const
    private const int delayTime = 10;
    private const int Time = 10;

    // Class
    private GameObject[] BlueObject { get { return ObjectManager.blueObject; } }
    private GameObject[] YellowObject { get { return ObjectManager.yellowObject; } }
    private StateSpace statespace;
    private SideInfo pEnv;


    /// <summary>
    /// 待解决：黄蓝方同时犯相同规，怎么判？
    /// </summary>
    /// <returns></returns>
    public bool Judge(MatchInfo matchInfo)
    {
        pEnv = matchInfo.GetBlueSide();
        ChangeSystem(pEnv);
        statespace = new StateSpace(pEnv);

        if (PlcaeKick() == 0)
        {
            if (GoalKick1() == 0)
            {
                if (PenaltyKick() == 0)
                {
                    if (GoalKick2() == 0)
                    {
                        FreeBall();
                    }
                }
            }
        }
        else
        {
            if (matchInfo.WhosBall == 0)    // 黄方被进球，开球进攻
            {
                Event.Send(Event.EventType1.Goal, true);
            }
            else
            {
                Event.Send(Event.EventType1.Goal, false);
            }
        }

        switch (statespace.gameState)
        {
            case PlayMode.PM_FreeBall_LeftTop:
                matchInfo.UpdateState(GameState.FreeBallTop, Side.OffensiveSide);
                break;
            case PlayMode.PM_FreeBall_LeftBot:
                matchInfo.UpdateState(GameState.FreeBallBottom, Side.OffensiveSide);
                break;
            case PlayMode.PM_FreeBall_RightTop:
                matchInfo.UpdateState(GameState.FreeBallTop, Side.DefensiveSide);
                break;
            case PlayMode.PM_FreeBall_RightBot:
                matchInfo.UpdateState(GameState.FreeBallBottom, Side.DefensiveSide);
                break;
            case PlayMode.PM_PlaceKick_Yellow:
                matchInfo.UpdateState(GameState.PlaceKick, Side.DefensiveSide);
                break;
            case PlayMode.PM_PlaceKick_Blue:
                matchInfo.UpdateState(GameState.PlaceKick, Side.OffensiveSide);
                break;
            case PlayMode.PM_PenaltyKick_Yellow:
                matchInfo.UpdateState(GameState.Plenalty, Side.DefensiveSide);
                break;
            case PlayMode.PM_PenaltyKick_Blue:
                matchInfo.UpdateState(GameState.Plenalty, Side.OffensiveSide);
                break;
            case PlayMode.PM_GoalKick_Yellow:
                matchInfo.UpdateState(GameState.GoalKick, Side.DefensiveSide);  //黄方门球蓝方进攻
                break;
            case PlayMode.PM_GoalKick_Blue:
                matchInfo.UpdateState(GameState.GoalKick, Side.OffensiveSide);
                break;
            default:
                matchInfo.UpdateState(GameState.NormalMatch, (Side)matchInfo.WhosBall);
                return false;
        }
        return true;
    }


    /// <summary>
    /// Change to old system
    /// </summary>
    /// <param name="env"></param>
    private void ChangeSystem(SideInfo env)
    {
        env.currentBall.pos.x += Const.Field.Right;
        env.currentBall.pos.y += Const.Field.Top;
        for (int i = 0; i < 5; i++)
        {
            env.home[i].pos.x += Const.Field.Right;
            env.home[i].pos.y += Const.Field.Top;
            env.opp[i].pos.x += Const.Field.Right;
            env.opp[i].pos.y += Const.Field.Top;
        }
    }


    private void Copy()
    {

        //ballslow;
        //pushball;
        //Foul_pushball;

        ////record
        //recordtime1;
        //recordtime2;
        //recordtime3;
        //recordtime4;
        //recordrobot;
        //recordzong;
        //recordrobot3;
        //recordrobot4;
        //recordzong4;

        ////opp
        //op_recordtime1;
        //op_recordtime2;
        //op_recordtime3;
        //op_recordtime4;
        //op_recordrobot;
        //op_recordzong;
        //op_recordrobot3;
        //op_recordrobot4;
        //op_recordzong4;
    }


    /// <summary>
    /// Calcute length between point a and b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private double GetLength(Vector3D a, Vector3D b)
    {
        double disx = a.x - b.x;
        double disy = a.y - b.y;
        double num = (disx * disx) + (disy * disy);
        return Math.Sqrt(num);
    }


    /// <summary>
    /// whether Object1 collide Object2
    /// </summary>
    /// <param name="Object1"></param>
    /// <param name="Object2"></param>
    /// <returns></returns>
    private bool JudgeCollision(GameObject Object1, GameObject Object2)
    {
        ArrayList TouchObject = Object1.GetComponent<BoxColliderEvent>().TouchObject;
        if (TouchObject.IndexOf(Object2) == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    /// <summary>
    /// Judge PenaltyKick
    /// </summary>
    /// <returns></returns>
    private int PenaltyKick()
    {
        int num1 = 0;
        int num2 = 0;
        int zong = 0;
        int nCycles = 10;
        if (pEnv.currentBall.pos.x >= 0 && pEnv.currentBall.pos.x <= 80 && pEnv.currentBall.pos.y >= 30 && pEnv.currentBall.pos.y <= 150)   // 黄方的部分区域
        {
            for (int i = 1; i < 5; i++)
            {
                if ((pEnv.opp[i].pos.x >= -15 && pEnv.opp[i].pos.x <= 0 && pEnv.opp[i].pos.y >= 70 && pEnv.opp[i].pos.y <= 110) || (pEnv.opp[i].pos.x >= 0 && pEnv.opp[i].pos.x <= 15 && pEnv.opp[i].pos.y >= 65 && pEnv.opp[i].pos.y <= 115))
                {
                    num1 = num1 + 1;
                    if (num1 == 1)
                    {
                        if (op_recordrobot == 0 || op_recordrobot == i)
                        {
                            op_recordrobot = i;
                            op_recordtime1 = op_recordtime1 + 1;
                        }
                        else
                        {
                            op_recordrobot = i;
                            op_recordtime1 = 1;
                        }
                    }
                    else
                    {
                        op_recordrobot = 0;
                        op_recordtime1 = 0;
                        op_recordtime2 = 0;
                        statespace.gameState = PlayMode.PM_PenaltyKick_Blue;
                        return 8;
                    }
                }
                if (((pEnv.opp[i].pos.x >= 0 && pEnv.opp[i].pos.x <= 15) && ((pEnv.opp[i].pos.y >= 50 && pEnv.opp[i].pos.y <= 65) || (pEnv.opp[i].pos.y >= 115 && pEnv.opp[i].pos.y <= 130))) || (pEnv.opp[i].pos.x >= 15 && pEnv.opp[i].pos.x <= 35 && pEnv.opp[i].pos.y >= 50 && pEnv.opp[i].pos.y <= 130))
                {
                    num2 = num2 + 1;
                    zong = zong + i;
                }
            }
            if (op_recordtime1 > nCycles)
            {
                op_recordrobot = 0;
                op_recordtime1 = 0;
                op_recordtime2 = 0;
                statespace.gameState = PlayMode.PM_PenaltyKick_Blue;
                return 8;
            }
            zong = zong + op_recordrobot;
            int num = num1 + num2;
            if (num > 3)
            {
                op_recordzong = 0;
                op_recordtime1 = 0;
                op_recordtime2 = 0;
                statespace.gameState = PlayMode.PM_PenaltyKick_Blue;
                return 8;
            }
            else if (num == 3)
            {
                if (op_recordzong == 0 || op_recordzong == zong)
                {
                    op_recordzong = zong;
                    op_recordtime2 = op_recordtime2 + 1;
                }
                else
                {
                    op_recordzong = zong;
                    op_recordtime2 = 1;
                    statespace.gameState = PlayMode.PM_PlayOn;
                    return 0;
                }
            }
            else
            {
                op_recordzong = 0;
                op_recordtime2 = 0;
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
            if (op_recordtime2 > nCycles)
            {
                op_recordzong = 0;
                op_recordtime1 = 0;
                op_recordtime2 = 0;
                statespace.gameState = PlayMode.PM_PenaltyKick_Blue;
                return 8;
            }
            else
            {
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }

        }
        else if (pEnv.currentBall.pos.x >= 140 && pEnv.currentBall.pos.x <= 220 && pEnv.currentBall.pos.y >= 30 && pEnv.currentBall.pos.y <= 150)
        {
            {
                for (int i = 1; i < 5; i++)
                {
                    if ((pEnv.home[i].pos.x >= 220 && pEnv.home[i].pos.x <= 235 && pEnv.home[i].pos.y >= 70 && pEnv.home[i].pos.y <= 110) || (pEnv.home[i].pos.x >= 205 && pEnv.home[i].pos.x <= 220 && pEnv.home[i].pos.y >= 65 && pEnv.home[i].pos.y <= 115))
                    {
                        num1 = num1 + 1;
                        if (num1 == 1)
                        {
                            if (recordrobot == 0 || recordrobot == i)
                            {
                                recordrobot = i;
                                recordtime1 = recordtime1 + 1;
                            }
                            else
                            {
                                recordrobot = i;
                                recordtime1 = 1;
                            }
                        }
                        else
                        {
                            recordrobot = 0;
                            recordtime1 = 0;
                            recordtime2 = 0;
                            statespace.gameState = PlayMode.PM_PenaltyKick_Yellow;
                            return 7;
                        }

                    }
                    if (((pEnv.home[i].pos.x >= 205 && pEnv.home[i].pos.x <= 220) && ((pEnv.home[i].pos.y >= 50 && pEnv.home[i].pos.y <= 65) || (pEnv.home[i].pos.y >= 115 && pEnv.home[i].pos.y <= 130))) || (pEnv.home[i].pos.x >= 185 && pEnv.home[i].pos.x <= 205 && pEnv.home[i].pos.y >= 50 && pEnv.home[i].pos.y <= 130))
                    {
                        num2 = num2 + 1;
                        zong = zong + i;
                    }
                }
                if (recordtime1 > nCycles)
                {
                    recordrobot = 0;
                    recordtime1 = 0;
                    recordtime2 = 0;
                    statespace.gameState = PlayMode.PM_PenaltyKick_Yellow;
                    return 7;
                }
                zong = zong + recordrobot;
                int num = num1 + num2;
                if (num > 3)
                {
                    recordzong = 0;
                    recordtime1 = 0;
                    recordtime2 = 0;
                    statespace.gameState = PlayMode.PM_PenaltyKick_Yellow;
                    return 7;
                }
                else if (num == 3)
                {
                    if (recordzong == 0 || recordzong == zong)
                    {
                        recordzong = zong;
                        recordtime2 = recordtime2 + 1;
                    }
                    else
                    {
                        recordzong = zong;
                        recordtime2 = 1;
                        statespace.gameState = PlayMode.PM_PlayOn;
                        return 0;
                    }
                }
                else
                {
                    recordzong = 0;
                    recordtime2 = 0;
                    statespace.gameState = PlayMode.PM_PlayOn;
                    return 0;
                }
                if (recordtime2 > nCycles)
                {
                    recordzong = 0;
                    recordtime1 = 0;
                    recordtime2 = 0;
                    statespace.gameState = PlayMode.PM_PenaltyKick_Yellow;
                    return 7;
                }
                else
                {
                    statespace.gameState = PlayMode.PM_PlayOn;
                    return 0;
                }
            }
        }
        else
        {
            recordzong = 0;
            recordtime1 = 0;
            recordtime2 = 0;
            op_recordzong = 0;
            op_recordtime1 = 0;
            op_recordtime2 = 0;
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
    }


    /// <summary>
    /// Judge GoalKick case 1
    /// </summary>
    /// <returns></returns>
    private int GoalKick1()
    {
        if (pEnv.currentBall.pos.x >= 140 && pEnv.currentBall.pos.x <= 220 && pEnv.currentBall.pos.y >= 30 && pEnv.currentBall.pos.y <= 150)
        {
            if (statespace.home[0].pos.x < 205 || statespace.home[0].pos.y < 65 || statespace.home[0].pos.y > 115)
            {
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
            for (int i = 0; i < 5; i++)
            {
                if (JudgeCollision(BlueObject[0], YellowObject[i]))
                {
                    statespace.gameState = PlayMode.PM_GoalKick_Blue;
                    return 12;
                }
            }
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
        else if (pEnv.currentBall.pos.x >= 0 && pEnv.currentBall.pos.x <= 80 && pEnv.currentBall.pos.y >= 30 && pEnv.currentBall.pos.y <= 150)
        {
            if (statespace.opp[0].pos.x > 15 || statespace.opp[0].pos.y < 65 || statespace.opp[0].pos.y > 115)
            {
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
            for (int i = 0; i < 5; i++)
            {
                if (JudgeCollision(BlueObject[i], YellowObject[0]))
                {
                    statespace.gameState = PlayMode.PM_GoalKick_Yellow;
                    return 11;
                }
            }
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
        else
        {
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
    }


    /// <summary>
    /// Judge GoalKick case 2
    /// </summary>
    /// <returns></returns>
    private int GoalKick2()
    {
        int num3 = 0;
        int num4 = 0;
        int zong4 = 0;
        int robot3 = 0;
        int nCycles = 10;

        if (pEnv.currentBall.pos.x >= 140 && pEnv.currentBall.pos.x <= 220 && pEnv.currentBall.pos.y >= 30 && pEnv.currentBall.pos.y <= 150)
        {
            for (int i = 0; i < 5; i++)
            {
                if ((pEnv.opp[i].pos.x >= 220 && pEnv.opp[i].pos.x <= 235 && pEnv.opp[i].pos.y >= 70 && pEnv.opp[i].pos.y <= 110) || (pEnv.opp[i].pos.x >= 205 && pEnv.opp[i].pos.x <= 220 && pEnv.opp[i].pos.y >= 65 && pEnv.opp[i].pos.y <= 115))
                {
                    num3 = num3 + 1;
                    if (num3 == 1)
                    {
                        robot3 = i;
                    }
                    else if (num3 == 2)
                    {
                        if (op_recordrobot3 == 0)
                        {
                            op_recordrobot3 = robot3;
                            op_recordrobot4 = i;
                            op_recordtime3 = 1;
                        }
                        else if (op_recordrobot3 == robot3)
                        {
                            if (op_recordrobot4 == i)
                            {
                                op_recordtime3 = op_recordtime3 + 1;
                            }
                            else
                            {
                                op_recordrobot4 = i;
                                op_recordtime3 = 1;
                            }
                        }
                        else
                        {
                            op_recordrobot3 = robot3;
                            op_recordrobot4 = i;
                            op_recordtime3 = 1;
                        }
                    }
                    else
                    {
                        op_recordrobot3 = 0;
                        op_recordrobot4 = 0;
                        op_recordtime3 = 0;
                        op_recordtime4 = 0;
                        op_recordzong4 = 0;
                        statespace.gameState = PlayMode.PM_GoalKick_Blue;
                        return 12;
                    }
                }

                if (((pEnv.opp[i].pos.x >= 205 && pEnv.opp[i].pos.x <= 220) && ((pEnv.opp[i].pos.y >= 50 && pEnv.opp[i].pos.y <= 65) || (pEnv.opp[i].pos.y >= 115 && pEnv.opp[i].pos.y <= 130))) || (pEnv.opp[i].pos.x >= 185 && pEnv.opp[i].pos.x <= 205 && pEnv.opp[i].pos.y >= 50 && pEnv.opp[i].pos.y <= 130))
                {
                    num4 = num4 + 1;
                    zong4 = zong4 + i;
                }
            }
            if (op_recordtime3 > nCycles)
            {
                op_recordrobot3 = 0;
                op_recordrobot4 = 0;
                op_recordzong4 = 0;
                op_recordtime3 = 0;
                op_recordtime4 = 0;
                statespace.gameState = PlayMode.PM_GoalKick_Blue;
                return 12;
            }
            zong4 = zong4 + robot3 + op_recordrobot4;
            int num = num3 + num4;
            if (num > 4)
            {
                op_recordrobot3 = 0;
                op_recordrobot4 = 0;
                op_recordzong4 = 0;
                op_recordtime3 = 0;
                op_recordtime4 = 0;
                statespace.gameState = PlayMode.PM_GoalKick_Blue;
                return 12;
            }
            else if (num == 4)
            {
                if (op_recordzong4 == 0 || op_recordzong4 == zong4)
                {
                    op_recordzong4 = zong4;
                    op_recordtime4 = op_recordtime4 + 1;
                }
                else
                {
                    op_recordzong4 = zong4;
                    op_recordtime4 = 1;
                    statespace.gameState = PlayMode.PM_PlayOn;
                    return 0;
                }
            }
            else
            {
                op_recordzong4 = 0;
                op_recordtime4 = 0;
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
            if (op_recordtime4 > nCycles)
            {
                op_recordrobot3 = 0;
                op_recordrobot4 = 0;
                op_recordzong4 = 0;
                op_recordtime3 = 0;
                op_recordtime4 = 0;
                statespace.gameState = PlayMode.PM_GoalKick_Blue;
                return 12;
            }
            else
            {
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
        }
        else if (pEnv.currentBall.pos.x >= 0 && pEnv.currentBall.pos.x <= 80 && pEnv.currentBall.pos.y >= 30 && pEnv.currentBall.pos.y <= 150)
        {
            for (int i = 0; i < 5; i++)
            {
                if ((pEnv.home[i].pos.x >= -15 && pEnv.home[i].pos.x <= 0 && pEnv.home[i].pos.y >= 70 && pEnv.home[i].pos.y <= 110) || (pEnv.home[i].pos.x >= 0 && pEnv.home[i].pos.x <= 15 && pEnv.home[i].pos.y >= 65 && pEnv.home[i].pos.y <= 115))
                {
                    num3 = num3 + 1;
                    if (num3 == 1)
                    {
                        robot3 = i;
                    }
                    else if (num3 == 2)
                    {
                        if (recordrobot3 == 0)
                        {
                            recordrobot3 = robot3;
                            recordrobot4 = i;
                            recordtime3 = 1;
                        }
                        else if (recordrobot3 == robot3)
                        {

                            if (recordrobot4 == i)
                            {
                                recordtime3 = recordtime3 + 1;
                            }
                            else
                            {
                                recordrobot4 = i;
                                recordtime3 = 1;
                            }
                        }
                        else
                        {
                            recordrobot3 = robot3;
                            recordrobot4 = i;
                            recordtime3 = 1;
                        }
                    }
                    else
                    {
                        recordrobot3 = 0;
                        recordrobot4 = 0;
                        recordtime3 = 0;
                        recordtime4 = 0;
                        recordzong4 = 0;
                        statespace.gameState = PlayMode.PM_GoalKick_Yellow;
                        return 11;
                    }
                }

                if (((pEnv.home[i].pos.x >= 0 && pEnv.home[i].pos.x <= 15) && ((pEnv.home[i].pos.y >= 50 && pEnv.home[i].pos.y <= 65) || (pEnv.home[i].pos.y >= 115 && pEnv.home[i].pos.y <= 130))) || (pEnv.home[i].pos.x >= 15 && pEnv.home[i].pos.x <= 35 && pEnv.home[i].pos.y >= 50 && pEnv.home[i].pos.y <= 130))
                {
                    num4 = num4 + 1;
                    zong4 = zong4 + i;
                }
            }
            if (recordtime3 > nCycles)
            {
                recordrobot3 = 0;
                recordrobot4 = 0;
                recordzong4 = 0;
                recordtime3 = 0;
                recordtime4 = 0;
                statespace.gameState = PlayMode.PM_GoalKick_Yellow;
                return 11;
            }
            zong4 = zong4 + robot3 + recordrobot4;
            int num = num3 + num4;
            if (num > 4)
            {
                recordrobot3 = 0;
                recordrobot4 = 0;
                recordzong4 = 0;
                recordtime3 = 0;
                recordtime4 = 0;
                statespace.gameState = PlayMode.PM_GoalKick_Yellow;
                return 11;
            }
            else if (num == 4)
            {
                if (recordzong4 == 0 || recordzong4 == zong4)
                {
                    recordzong4 = zong4;
                    recordtime4 = recordtime4 + 1;
                }
                else
                {
                    recordzong4 = zong4;
                    recordtime4 = 1;
                    statespace.gameState = PlayMode.PM_PlayOn;
                    return 0;
                }
            }
            else
            {
                recordzong4 = 0;
                recordtime4 = 0;
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
            if (recordtime4 > nCycles)
            {
                recordrobot3 = 0;
                recordrobot4 = 0;
                recordzong4 = 0;
                recordtime3 = 0;
                recordtime4 = 0;
                statespace.gameState = PlayMode.PM_GoalKick_Yellow;
                return 11;
            }
            else
            {
                statespace.gameState = PlayMode.PM_PlayOn;
                return 0;
            }
            
        }
        else
        {
            recordrobot3 = 0;
            recordrobot4 = 0;
            recordzong4 = 0;
            recordtime3 = 0;
            recordtime4 = 0;
            op_recordrobot3 = 0;
            op_recordrobot4 = 0;
            op_recordzong4 = 0;
            op_recordtime3 = 0;
            op_recordtime4 = 0;
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
    }


    /// <summary>
    /// Judge PlcaeKick
    /// </summary>
    /// <returns></returns>
    private int PlcaeKick()
    {
        if (pEnv.currentBall.pos.x <= -2.5)
        {
            statespace.gameState = PlayMode.PM_PlaceKick_Yellow;
            return 5;
        }
        else if (pEnv.currentBall.pos.x >= 222.5)
        {
            statespace.gameState = PlayMode.PM_PlaceKick_Blue;
            return 6;
        }
        else
        {
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
    }


    /// <summary>
    /// Judge FreeBall
    /// </summary>
    /// <returns></returns>
    private int FreeBall()
    {
        int robot = 0;
        bool tuiqiu1 = false;
        bool tuiqiu2 = false;
        bool tuiqiu3 = false;
        bool facetoface = false;
        int nSlowCycles = 400;
        int nFaceCycles = 50;
        if (pEnv.currentBall.pos.x <= 8 && pEnv.currentBall.pos.y <= 70)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pEnv.home[i].pos.y > pEnv.currentBall.pos.y && pEnv.home[i].pos.x <= 8 && pEnv.home[i].pos.y <= 70)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        tuiqiu3 = true;
                    }
                }
                if (pEnv.home[i].pos.y < pEnv.currentBall.pos.y && pEnv.home[i].pos.x <= 8 && pEnv.home[i].pos.y <= 70)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu1 = true;
                        break;
                    }
                }
            }
            if (tuiqiu1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.home[i].pos.x <= 8 && pEnv.home[i].pos.y <= 70)
                        {
                            if (JudgeCollision(BlueObject[i], BlueObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightBot;
                                Foul_pushball += 1;
                                //char strMsg[100] = { 0 };
                                //sprintf(strMsg, "The Blue team has violated the nopushing rule for %d time(s)!", Foul_pushball);
                                //MessageBox(NULL, strMsg, "Hint", MB_OK);
                                //Console.WriteLine("The Blue team has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                Event.Send(Event.EventType1.LogUpdate, "The " + (Side)pEnv.whosBall + " has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                return 4;
                            }
                        }
                    }
                }
            }
            if (tuiqiu1 || tuiqiu3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (pEnv.opp[i].pos.x <= 8 && pEnv.opp[i].pos.y <= 70)
                    {
                        if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                        {
                            facetoface = true;
                            pushball += 1;
                            if (pushball >= nFaceCycles)
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftBot;
                                pushball = 0;
                                return 2;
                            }
                        }
                    }
                }
                if (facetoface == false)
                {
                    pushball = 0;
                }
            }
            else
            {
                pushball = 0;
            }


            for (int i = 0; i < 5; i++)
            {
                if (pEnv.opp[i].pos.y > pEnv.currentBall.pos.y && pEnv.opp[i].pos.x <= 8 && pEnv.opp[i].pos.y <= 70)
                {
                    if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu2 = true;
                        break;
                    }
                }
            }
            if (tuiqiu2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.opp[i].pos.x <= 8 && pEnv.opp[i].pos.y <= 70)
                        {
                            if (JudgeCollision(YellowObject[i], YellowObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftBot;
                                return 2;
                            }
                        }
                    }
                }
            }
        }
        if (pEnv.currentBall.pos.x <= 8 && pEnv.currentBall.pos.y >= 110)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pEnv.home[i].pos.y < pEnv.currentBall.pos.y && pEnv.home[i].pos.x <= 8 && pEnv.home[i].pos.y <= 110)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        tuiqiu3 = true;
                    }
                }
                if (pEnv.home[i].pos.y > pEnv.currentBall.pos.y && pEnv.home[i].pos.x <= 8 && pEnv.home[i].pos.y <= 110)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu1 = true;
                        break;
                    }
                }
            }
            if (tuiqiu1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.home[i].pos.x <= 8 && pEnv.home[i].pos.y >= 110)
                        {
                            if (JudgeCollision(BlueObject[i], BlueObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightTop;
                                Foul_pushball += 1;
                                //char strMsg[100] = { 0 };
                                //sprintf(strMsg, "The Blue team has violated the nopushing rule for %d time(s)!", Foul_pushball);
                                //MessageBox(NULL, strMsg, "Hint", MB_OK);
                                //Console.WriteLine("The Blue team has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                Event.Send(Event.EventType1.LogUpdate, "The " + (Side)pEnv.whosBall + " has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                return 3;
                            }
                        }
                    }
                }
            }
            if (tuiqiu1 || tuiqiu3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (pEnv.opp[i].pos.x <= 8 && pEnv.opp[i].pos.y >= 110)
                    {
                        if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                        {
                            facetoface = true;
                            pushball += 1;
                            if (pushball >= nFaceCycles)
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftTop;
                                pushball = 0;
                                return 1;
                            }
                        }
                    }
                }
                if (facetoface == false)
                {
                    pushball = 0;
                }
            }
            else
            {
                pushball = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                if (pEnv.opp[i].pos.y < pEnv.currentBall.pos.y && pEnv.opp[i].pos.x <= 8 && pEnv.opp[i].pos.y >= 110)
                {
                    if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu2 = true;
                        break;
                    }
                }
            }
            if (tuiqiu2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.opp[i].pos.x <= 8 && pEnv.opp[i].pos.y >= 110)
                        {
                            if (JudgeCollision(YellowObject[i], YellowObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftTop;
                                return 1;
                            }
                        }
                    }
                }
            }
        }
        if (pEnv.currentBall.pos.y >= 172)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pEnv.home[i].pos.x < pEnv.currentBall.pos.x && pEnv.home[i].pos.y >= 172)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        tuiqiu3 = true;
                    }
                }
                if (pEnv.home[i].pos.x > pEnv.currentBall.pos.x && pEnv.home[i].pos.y >= 172)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu1 = true;
                        break;
                    }
                }
            }
            if (tuiqiu1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.home[i].pos.y >= 172)
                        {
                            if (JudgeCollision(BlueObject[i], BlueObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightTop;
                                Foul_pushball += 1;
                                //char strMsg[100] = { 0 };
                                //sprintf(strMsg, "The Blue team has violated the nopushing rule for %d time(s)!", Foul_pushball);
                                //MessageBox(NULL, strMsg, "Hint", MB_OK);
                                //Console.WriteLine("The Blue team has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                Event.Send(Event.EventType1.LogUpdate, "The " + (Side)pEnv.whosBall + " has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                return 3;
                            }
                        }
                    }
                }
            }
            if (tuiqiu1 || tuiqiu3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (pEnv.opp[i].pos.y >= 172)
                    {
                        if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                        {
                            facetoface = true;
                            pushball += 1;
                            if (pushball >= nFaceCycles)
                            {
                                if (pEnv.currentBall.pos.x <= 110)
                                {
                                    statespace.gameState = PlayMode.PM_FreeBall_LeftTop;
                                    pushball = 0;
                                    return 1;
                                }
                                else
                                {
                                    statespace.gameState = PlayMode.PM_FreeBall_RightTop;
                                    pushball = 0;
                                    return 3;
                                }
                            }
                        }
                    }
                }
                if (facetoface == false)
                {
                    pushball = 0;
                }
            }
            else
            {
                pushball = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                if (pEnv.opp[i].pos.x < pEnv.currentBall.pos.x && pEnv.opp[i].pos.y >= 172)
                {
                    if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu2 = true;
                        break;
                    }
                }
            }
            if (tuiqiu2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.opp[i].pos.y >= 172)
                        {
                            if (JudgeCollision(YellowObject[i], YellowObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftTop;
                                return 1;
                            }
                        }
                    }
                }
            }
        }
        if (pEnv.currentBall.pos.y <= 8)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pEnv.home[i].pos.x < pEnv.currentBall.pos.x && pEnv.home[i].pos.y <= 8)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        tuiqiu3 = true;
                    }
                }
                if (pEnv.home[i].pos.x > pEnv.currentBall.pos.x && pEnv.home[i].pos.y <= 8)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu1 = true;
                        break;
                    }
                }
            }
            if (tuiqiu1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.home[i].pos.y <= 8)
                        {
                            if (JudgeCollision(BlueObject[i], BlueObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightBot; // 2018-8-8 changed from PlayMode.PM_FreeBall_LeftBot
                                Foul_pushball += 1;
                                //char strMsg[100] = { 0 };
                                //sprintf(strMsg, "The Blue team has violated the nopushing rule for %d time(s)!", Foul_pushball);
                                //MessageBox(NULL, strMsg, "Hint", MB_OK);
                                //Console.WriteLine("The Blue team has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                Event.Send(Event.EventType1.LogUpdate, "The " + (Side)pEnv.whosBall + " has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                return 4;  // 2018-8-8 changed from 2
                            }
                        }
                    }
                }
            }
            if (tuiqiu1 || tuiqiu3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (pEnv.opp[i].pos.y <= 8)
                    {
                        if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                        {
                            facetoface = true;
                            pushball += 1;
                            if (pushball >= nFaceCycles)
                            {
                                if (pEnv.currentBall.pos.x <= 110)
                                {
                                    statespace.gameState = PlayMode.PM_FreeBall_LeftBot;
                                    pushball = 0;
                                    return 2;
                                }
                                else
                                {
                                    statespace.gameState = PlayMode.PM_FreeBall_RightBot;
                                    pushball = 0;
                                    return 4;
                                }
                            }
                        }
                    }
                }
                if (facetoface == false)
                {
                    pushball = 0;
                }
            }
            else
            {
                pushball = 0;
            }


            for (int i = 0; i < 5; i++)
            {
                if (pEnv.opp[i].pos.x < pEnv.currentBall.pos.x && pEnv.opp[i].pos.y <= 8)
                {
                    if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu2 = true;
                        break;
                    }
                }
            }
            if (tuiqiu2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.opp[i].pos.y <= 8)
                        {
                            if (JudgeCollision(YellowObject[i], YellowObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftBot;
                                return 2;
                            }
                        }
                    }
                }
            }
        }
        if (pEnv.currentBall.pos.x >= 212 && pEnv.currentBall.pos.y <= 70)
        {
            for (int i = 0; i < 5; i++)
            {
                if (pEnv.home[i].pos.y < pEnv.currentBall.pos.y && pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y <= 70)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        tuiqiu3 = true;
                    }
                }
                if (pEnv.home[i].pos.y > pEnv.currentBall.pos.y && pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y <= 70)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu1 = true;
                        break;
                    }
                }
            }
            if (tuiqiu1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y <= 70)
                        {
                            if (JudgeCollision(BlueObject[i], BlueObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightBot;
                                Foul_pushball += 1;
                                //char strMsg[100] = { 0 };
                                //sprintf(strMsg, "The Blue team has violated the nopushing rule for %d time(s)!", Foul_pushball);
                                //MessageBox(NULL, strMsg, "Hint", MB_OK);
                                //Console.WriteLine("The Blue team has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                Event.Send(Event.EventType1.LogUpdate, "The " + (Side)pEnv.whosBall + " has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                return 4;
                            }
                        }
                    }
                }
            }
            if (tuiqiu1 || tuiqiu3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (pEnv.opp[i].pos.x >= 212 && pEnv.opp[i].pos.y <= 70)
                    {
                        facetoface = true;
                        if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                        {
                            pushball += 1;
                            if (pushball >= nFaceCycles)
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightBot;
                                pushball = 0;
                                return 4;
                            }
                        }
                    }
                }
                if (facetoface == false)
                {
                    pushball = 0;
                }
            }
            else
            {
                pushball = 0;
            }


            for (int i = 0; i < 5; i++)
            {
                if (pEnv.opp[i].pos.y < pEnv.currentBall.pos.y && pEnv.opp[i].pos.x >= 212 && pEnv.opp[i].pos.y <= 70)
                {
                    if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu2 = true;
                        break;
                    }
                }
            }
            if (tuiqiu2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.opp[i].pos.x >= 212 && pEnv.opp[i].pos.y <= 70)
                        {
                            if (JudgeCollision(YellowObject[i], YellowObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftBot;
                                return 2;
                            }
                        }
                    }
                }
            }
        }
        if (pEnv.currentBall.pos.x >= 212 && pEnv.currentBall.pos.y >= 110)
        {
            for (int i = 0; i < 5; i++)
            {
                // 2018-8-8 change from if (pEnv.home[i].pos.y > pEnv.currentBall.pos.y && pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y <= 110)
                if (pEnv.home[i].pos.y > pEnv.currentBall.pos.y && pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y >= 110)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        tuiqiu3 = true;
                    }
                }
                // 2018-8-8 change from if (pEnv.home[i].pos.y < pEnv.currentBall.pos.y && pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y <= 110)
                if (pEnv.home[i].pos.y < pEnv.currentBall.pos.y && pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y >= 110)
                {
                    if (GetLength(pEnv.home[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu1 = true;
                        break;
                    }
                }
            }
            if (tuiqiu1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.home[i].pos.x >= 212 && pEnv.home[i].pos.y >= 110)
                        {
                            if (JudgeCollision(BlueObject[i], BlueObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightTop;
                                Foul_pushball += 1;
                                //char strMsg[100] = { 0 };
                                //sprintf(strMsg, "The Blue team has violated the nopushing rule for %d time(s)!", Foul_pushball);
                                //MessageBox(NULL, strMsg, "Hint", MB_OK);
                                //Console.WriteLine("The Blue team has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                Event.Send(Event.EventType1.LogUpdate, "The " + (Side)pEnv.whosBall + " has violated the nopushing rule for " + Foul_pushball.ToString() + " time(s)!");
                                return 3;
                            }
                        }
                    }
                }
            }
            if (tuiqiu1 || tuiqiu3)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (pEnv.opp[i].pos.x >= 212 && pEnv.opp[i].pos.y >= 110)
                    {
                        if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                        {
                            facetoface = true;
                            pushball += 1;
                            if (pushball >= nFaceCycles)
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_RightTop;
                                pushball = 0;
                                return 3;
                            }
                        }
                    }
                }
                if (facetoface == false)
                {
                    pushball = 0;
                }
            }
            else
            {
                pushball = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                if (pEnv.opp[i].pos.y > pEnv.currentBall.pos.y && pEnv.opp[i].pos.x >= 212 && pEnv.opp[i].pos.y >= 110)
                {
                    if (GetLength(pEnv.opp[i].pos, pEnv.currentBall.pos) < 6.69)
                    {
                        robot = i;
                        tuiqiu2 = true;
                        break;
                    }
                }
            }
            if (tuiqiu2)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i == robot)
                    {
                        continue;
                    }
                    else
                    {
                        if (pEnv.opp[i].pos.x >= 212 && pEnv.opp[i].pos.y >= 110)
                        {
                            if (JudgeCollision(YellowObject[i], YellowObject[robot]))
                            {
                                statespace.gameState = PlayMode.PM_FreeBall_LeftTop;
                                return 1;
                            }
                        }
                    }
                }
            }
        }
        if (System.Math.Sqrt(System.Math.Pow(pEnv.currentBall.linearVelocity.x, 2) + System.Math.Pow(pEnv.currentBall.linearVelocity.y, 2)) > 0.5)
        {
            ballslow = 0;
            statespace.gameState = PlayMode.PM_PlayOn;
            return 0;
        }
        else
        {
            ballslow = ballslow + 1;
            if (ballslow >= nSlowCycles)
            {
                if (pEnv.currentBall.pos.x <= 15 && (pEnv.currentBall.pos.y >= 65 && pEnv.currentBall.pos.y <= 115))
                {
                    ballslow = 0;
                    statespace.gameState = PlayMode.PM_GoalKick_Yellow;
                    return 11;
                }
                else if (pEnv.currentBall.pos.x >= 205 && (pEnv.currentBall.pos.y >= 65 && pEnv.currentBall.pos.y <= 115))
                {
                    ballslow = 0;
                    statespace.gameState = PlayMode.PM_GoalKick_Blue;
                    return 12;
                }
                else
                {
                    if (pEnv.currentBall.pos.x <= 110 && pEnv.currentBall.pos.y >= 90)
                    {
                        ballslow = 0;
                        statespace.gameState = PlayMode.PM_FreeBall_LeftTop;
                        return 1;
                    }
                    if (pEnv.currentBall.pos.x <= 110 && pEnv.currentBall.pos.y < 90)
                    {
                        ballslow = 0;
                        statespace.gameState = PlayMode.PM_FreeBall_LeftBot;
                        return 2;
                    }
                    if (pEnv.currentBall.pos.x > 110 && pEnv.currentBall.pos.y >= 90)
                    {
                        ballslow = 0;
                        statespace.gameState = PlayMode.PM_FreeBall_RightTop;
                        return 3;
                    }
                    if (pEnv.currentBall.pos.x > 110 && pEnv.currentBall.pos.y < 90)
                    {
                        ballslow = 0;
                        statespace.gameState = PlayMode.PM_FreeBall_RightBot;
                        return 4;
                    }
                }
            }
        }
        return 0;
    }

}

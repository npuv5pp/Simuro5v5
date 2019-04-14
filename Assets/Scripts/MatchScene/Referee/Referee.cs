using System;
using System.Collections;
using System.Text;
using Assets.Scripts.MatchScene.Referee;
using Simuro5v5;
using UnityEngine;
using Event = Simuro5v5.EventSystem.Event;

public class Referee
{
    //Class
    private GameObject[] BlueObject { get { return PlayMain.ObjectManager.blueObject; } }
    private GameObject[] YellowObject { get { return PlayMain.ObjectManager.yellowObject; } }
    private MatchInfo matchInfo;
    private Robot[] BlueRobots;
    private Robot[] YellowRobots;

    private int GoalieBlueID;
    private int GoalieYellowID;
    private int StandoffTime;

    private Square YellowGoalState;
    private Square YellowBigState;
    private Square YellowSmallState;
    private Square BlueGoalState;
    private Square BlueBigState;
    private Square BlueSmallState;

    public Referee()
    {
        YellowGoalState = new Square(-125, -110, 20, -20);
        YellowBigState = new Square(-125, -75, 40, -40);
        YellowSmallState = new Square(-125, -95, 25, -25);

        BlueGoalState = new Square(110, 125, 20, -20);
        BlueBigState = new Square(75, 125, 40, -40);
        BlueSmallState = new Square(95, 125, 25, -25);

    }

    ///<summary>
    ///判断比赛状态，返回JudgeResult类
    /// </summary>
    public JudgeResult Judge(MatchInfo matchInfo)
    {
        this.matchInfo = matchInfo;
        this.BlueRobots = matchInfo.BlueRobots;
        this.YellowRobots = matchInfo.YellowRobots;
        this.GoalieBlueID = FindBlueGoalie();
        this.GoalieYellowID = FindYellowGoalie();
        this.StandoffTime = 0;

        JudgeResult judgeResult = new JudgeResult();

        if (JudgePlace(judgeResult) == false)
        {
            if (JudgePenalty(judgeResult) == false)
            {
                if (JudgeGoalie(judgeResult) == false)
                {
                    if (JudgeFree(judgeResult) == false)
                    {
                        judgeResult.ResultType = ResultType.NormalMatch;
                        judgeResult.Actor = Side.None;
                        judgeResult.Reason = "Normal competition";
                    }
                }
            }
        }

        return judgeResult;
    }

    private int FindBlueGoalie()
    {
        int ID = -1;
        for (int i = 0; i <= 4; i++)
        {
            if (BlueSmallState.InSquare(BlueRobots[i].pos))
            {
                ID = i;
            }
        }
        return ID;
    }

    private int FindYellowGoalie()
    {
        int ID = -1;
        for (int i = 0; i <= 4; i++)
        {
            if (YellowSmallState.InSquare(YellowRobots[i].pos))
            {
                ID = i;
            }
        }
        return ID;
    }

    private bool JudgePlace(JudgeResult judgeResult)
    {
        if (BlueGoalState.InSquare(matchInfo.Ball.pos))
        {
            judgeResult.Actor = Side.Yellow;
            judgeResult.Reason = "Be scored and PlaceKick again";
            judgeResult.ResultType = ResultType.PlaceKick;
            Event.Send(Event.EventType1.Goal, true); //黄方被进球
            return true;
        }
        else if (YellowGoalState.InSquare(matchInfo.Ball.pos))
        {
            judgeResult.Actor = Side.Blue;
            judgeResult.Reason = "Be scored and PlaceKick again";
            judgeResult.ResultType = ResultType.PlaceKick;
            Event.Send(Event.EventType1.Goal, false); //蓝方被进球
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool JudgePenalty(JudgeResult judgeResult)
    {
        if (matchInfo.Ball.pos.x > 0)
        {
            int SmallStateNum = 0;
            int BigStateNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (BlueBigState.InSquare(BlueRobots[i].pos))
                {
                    BigStateNum++;
                }
                if (BlueSmallState.InSquare(BlueRobots[i].pos))
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = Side.Yellow;
                judgeResult.Reason = "Defenders have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = Side.Blue;
                judgeResult.Reason = "Defenders have two robots in SmallState";
                return true;
            }
        }
        else
        {
            int SmallStateNum = 0;
            int BigStateNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (YellowBigState.InSquare(YellowRobots[i].pos))
                {
                    BigStateNum++;
                }
                if (YellowSmallState.InSquare(YellowRobots[i].pos))
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = Side.Blue;
                judgeResult.Reason = "Defenders have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = Side.Blue;
                judgeResult.Reason = "Defenders have two robots in SmallState";
                return true;
            }
        }
        return false;
    }

    private bool JudgeGoalie(JudgeResult judgeResult)
    {
        if (BlueBigState.InSquare(matchInfo.Ball.pos))
        {
            int SmallStateNum = 0;
            int BigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (GoalieBlueID != -1 && JudgeCollision(BlueObject[GoalieBlueID], YellowObject[i]))
                {
                    judgeResult.ResultType = ResultType.GoalKick;
                    judgeResult.Actor = Side.Blue;
                    judgeResult.Reason = "Attacker hit the Goalie";
                    return true;
                }
                if (BlueBigState.InSquare(YellowRobots[i].pos))
                {
                    BigStateNum++;
                }
                if (BlueSmallState.InSquare(YellowRobots[i].pos))
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = Side.Blue;
                judgeResult.Reason = "Attacker have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = Side.Blue;
                judgeResult.Reason = "Attacker have two robots in SmallState";
                return true;
            }
        }
        else if (YellowBigState.InSquare(matchInfo.Ball.pos))
        {
            int SmallStateNum = 0;
            int BigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (GoalieYellowID != -1 && JudgeCollision(YellowObject[GoalieYellowID], BlueObject[i]))
                {
                    judgeResult.ResultType = ResultType.GoalKick;
                    judgeResult.Actor = Side.Yellow;
                    judgeResult.Reason = "Attacker hit the Goalie";
                    return true;
                }
                if (YellowBigState.InSquare(BlueRobots[i].pos))
                {
                    BigStateNum++;
                }
                if (YellowSmallState.InSquare(BlueRobots[i].pos))
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = Side.Yellow;
                judgeResult.Reason = "Attacker have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = Side.Yellow;
                judgeResult.Reason = "Attacker have two robots in SmallState";
                return true;
            }
        }

        return false;

    }
    private bool JudgeFree(JudgeResult judgeResult)
    {
        if (matchInfo.Ball.linearVelocity.GetUnityVector2().magnitude < 5)
        {
            StandoffTime++;
            if (StandoffTime > 500)
            {
                if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = Side.Blue;
                    judgeResult.Reason = "RightTop Standoff time longer than 10 seconds in game";
                    return true;
                }
                else if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y < 0)
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = Side.Blue;
                    judgeResult.Reason = "RightBot Standoff time longer than 10 seconds in game";
                    return true;
                }
                else if (matchInfo.Ball.pos.x < 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = Side.Yellow;
                    judgeResult.Reason = "LeftTop Standoff time longer than 10 seconds in game";
                    return true;
                }
                else
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = Side.Yellow;
                    judgeResult.Reason = "LeftBot Standoff time longer than 10 seconds in game";
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
            StandoffTime = 0;
            return false;
        }

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


}

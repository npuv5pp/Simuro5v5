using System;
using System.Collections;
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

    public Referee()
    {

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
                        judgeResult.Actor = "None";
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
            if (BlueRobots[i].pos.x > 94.4 && BlueRobots[i].pos.y > -25 && BlueRobots[i].pos.y < 25)
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
            if (YellowRobots[i].pos.x < -94.4 && YellowRobots[i].pos.y > -25 && YellowRobots[i].pos.y < 25)
            {
                ID = i;
            }
        }
        return ID;
    }

    private bool JudgePlace(JudgeResult judgeResult)
    {
        if (matchInfo.Ball.pos.x <= -112 && matchInfo.Ball.pos.y >= -15 && matchInfo.Ball.pos.y <= 15)
        {
            judgeResult.Actor = "Yellow";
            judgeResult.Reason = "Be scored and PlaceKick again";
            judgeResult.ResultType = ResultType.PlaceKick;
            Event.Send(Event.EventType1.Goal, true); //黄方被进球
            return true;
        }
        else if (matchInfo.Ball.pos.x >= 112 && matchInfo.Ball.pos.y >= -15 && matchInfo.Ball.pos.y <= 15)
        {
            judgeResult.Actor = "Blue";
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
                if (BlueRobots[i].pos.x > 74.4 && BlueRobots[i].pos.y > -40 && BlueRobots[i].pos.y < 40)
                {
                    BigStateNum++;
                }
                if (BlueRobots[i].pos.x > 94.4 && BlueRobots[i].pos.y > -25 && BlueRobots[i].pos.y < 25)
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = "Yellow";
                judgeResult.Reason = "Defenders have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = "Yellow";
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
                if (YellowRobots[i].pos.x < -74.4 && YellowRobots[i].pos.y > -40 && YellowRobots[i].pos.y < 40)
                {
                    BigStateNum++;
                }
                if (YellowRobots[i].pos.x < -94.4 && YellowRobots[i].pos.y > -25 && YellowRobots[i].pos.y < 25)
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = "Blue";
                judgeResult.Reason = "Defenders have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.PenaltyKick;
                judgeResult.Actor = "Blue";
                judgeResult.Reason = "Defenders have two robots in SmallState";
                return true;
            }
        }
        return false;
    }

    private bool JudgeGoalie(JudgeResult judgeResult)
    {
        if (matchInfo.Ball.pos.x >= 74.4 && matchInfo.Ball.pos.y >= -40 && matchInfo.Ball.pos.y <= 40)
        {
            int SmallStateNum = 0;
            int BigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (GoalieBlueID != -1 && JudgeCollision(BlueObject[GoalieBlueID], YellowObject[i]))
                {
                    judgeResult.ResultType = ResultType.GoalKick;
                    judgeResult.Actor = "Blue";
                    judgeResult.Reason = "Attacker hit the Goalie";
                    return true;
                }
                if (YellowRobots[i].pos.x < -74.4 && YellowRobots[i].pos.y > -40 && YellowRobots[i].pos.y < 40)
                {
                    BigStateNum++;
                }
                if (YellowRobots[i].pos.x < -94.4 && YellowRobots[i].pos.y > -25 && YellowRobots[i].pos.y < 25)
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = "Blue";
                judgeResult.Reason = "Attacker have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = "Blue";
                judgeResult.Reason = "Attacker have two robots in SmallState";
                return true;
            }
        }
        else if (matchInfo.Ball.pos.x <= -74.4 && matchInfo.Ball.pos.y >= -40 && matchInfo.Ball.pos.y <= 40)
        {
            int SmallStateNum = 0;
            int BigStateNum = 0;
            for (int i = 0; i <= 4; i++)
            {
                if (GoalieYellowID != -1 && JudgeCollision(YellowObject[GoalieYellowID], BlueObject[i]))
                {
                    judgeResult.ResultType = ResultType.GoalKick;
                    judgeResult.Actor = "Yellow";
                    judgeResult.Reason = "Attacker hit the Goalie";
                    return true;
                }
                if (BlueRobots[i].pos.x < -74.4 && BlueRobots[i].pos.y > -40 && YellowRobots[i].pos.y < 40)
                {
                    BigStateNum++;
                }
                if (BlueRobots[i].pos.x < -94.4 && BlueRobots[i].pos.y > -25 && YellowRobots[i].pos.y < 25)
                {
                    SmallStateNum++;
                }
            }
            if (BigStateNum >= 4)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = "Yellow";
                judgeResult.Reason = "Attacker have four robots in BigState";
                return true;
            }
            if (SmallStateNum >= 2)
            {
                judgeResult.ResultType = ResultType.GoalKick;
                judgeResult.Actor = "Yellow";
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
                    judgeResult.Actor = "Blue";
                    judgeResult.Reason = "RightTop Standoff time longer than 10 seconds in game";
                    return true;
                }
                else if (matchInfo.Ball.pos.x > 0 && matchInfo.Ball.pos.y < 0)
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = "Blue";
                    judgeResult.Reason = "RightBot Standoff time longer than 10 seconds in game";
                    return true;
                }
                else if (matchInfo.Ball.pos.x < 0 && matchInfo.Ball.pos.y > 0)
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = "Yellow";
                    judgeResult.Reason = "LeftTop Standoff time longer than 10 seconds in game";
                    return true;
                }
                else
                {
                    judgeResult.ResultType = ResultType.FreeKick;
                    judgeResult.Actor = "Yellow";
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

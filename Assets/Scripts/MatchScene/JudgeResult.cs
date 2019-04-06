using UnityEngine;
using UnityEditor;

public enum ResultType
{
    PlaceKick,
    GoalKick,
    PenaltyKick,
    FreeKick,
    NormalMatch
}

public class JudgeResult
{
    public ResultType ResultType;
    public string Actor;
    public string Reason;

    public JudgeResult(ResultType result , string actor , string reason)
    {
        this.ResultType = result;
        this.Actor = actor;
        this.Reason = reason;
    }

    public JudgeResult(){ }
}
using UnityEngine;
using UnityEditor;
using Simuro5v5;

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
    public Side Actor;
    public string Reason;

    public JudgeResult(ResultType result, Side actor, string reason)
    {
        ResultType = result;
        Actor = actor;
        Reason = reason;
    }

    public JudgeResult() { }

    public string ToRichText()

    {

        var rv = $"Foul: {ResultType}\t";
        if (Actor == Side.Blue)
        {
            rv += $"<color=#{Const.Style.SideBlue.ToHex()}>{Actor}<color=#F20C00> team is actor.\nReason: <color=\"green\">{Reason}";
        }
        else
        {
            rv += $"<color=#{Const.Style.SideYellow.ToHex()}>{Actor}<color=#F20C00> team is actor.\nReason: <color=\"green\">{Reason}";
        }
        return rv;
    }
}

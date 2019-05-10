using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEditor;
using Simuro5v5;

public enum ResultType
{
    NormalMatch,
    EndHalf,        //半场结束 上半场下半场加时赛结束、接口使拍数变0
    GameOver,        //游戏结束，用来判断胜负
    PlaceKick,
    GoalKick,
    PenaltyKick,
    FreeKickRightTop,
    FreeKickRightBot,
    FreeKickLeftTop,
    FreeKickLeftBot
}

public struct JudgeResult
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ResultType ResultType { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public Side Actor { get; set; }
    public string Reason { get; set; }

    public string ToRichText()
    {

        var rv = $"Foul: {ResultType}, ";
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

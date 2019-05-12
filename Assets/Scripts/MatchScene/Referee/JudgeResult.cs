using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEditor;
using Simuro5v5;
using System;

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
    /// <summary>
    /// 本次摆位的类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ResultType ResultType { get; set; }
    
    /// <summary>
    /// 本次摆位的进攻方
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public Side Actor { get; set; }

    /// <summary>
    /// 本次摆位的原因细节
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// 获取哪方先摆位
    /// 门球（GoalKick）和开球（PlaceKick）由进攻方（Actor）先摆位，其他情况由防守方先摆位
    /// </summary>
    /// <returns></returns>
    public Side WhoisFirst
    {
        get
        {
            switch (ResultType)
            {
                case ResultType.GoalKick:
                case ResultType.PlaceKick:
                    return Actor;

                case ResultType.FreeKickLeftBot:
                case ResultType.FreeKickLeftTop:
                case ResultType.FreeKickRightBot:
                case ResultType.FreeKickRightTop:
                case ResultType.PenaltyKick:
                    return Actor.ToAnother();

                default:
                    throw new ArgumentException($"error ResultType {ResultType}");
            }
        }
    }

    /// <summary>
    /// 获取哪方需要摆球
    /// 门球由进攻方（Actor）摆球，其他情况球固定
    /// </summary>
    /// <returns></returns>
    public Side WhosBall
    {
        get
        {
            switch (ResultType)
            {
                case ResultType.GoalKick:
                    return Actor;

                case ResultType.PlaceKick:
                case ResultType.FreeKickLeftBot:
                case ResultType.FreeKickLeftTop:
                case ResultType.FreeKickRightBot:
                case ResultType.FreeKickRightTop:
                case ResultType.PenaltyKick:
                    return Side.Nobody;

                default:
                    throw new ArgumentException($"error ResultType {ResultType}");
            }
        }
    }

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

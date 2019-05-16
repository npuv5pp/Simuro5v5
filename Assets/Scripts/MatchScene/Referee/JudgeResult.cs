using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEditor;
using Simuro5v5;
using System;

/// <summary>
/// 裁判的判定结果类型。用于指定下一拍的动作
/// </summary>
public enum ResultType
{
    NormalMatch,
    NextPhase,        //半场结束 上半场下半场加时赛结束、接口使拍数变0
    GameOver,        //游戏结束，用来判断胜负
    PlaceKick,
    GoalKick,
    PenaltyKick,
    FreeKickRightTop,
    FreeKickRightBot,
    FreeKickLeftTop,
    FreeKickLeftBot
}

[JsonObject(MemberSerialization.OptIn)]
public struct JudgeResult
{
    /// <summary>
    /// 下一拍的动作类型
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public ResultType ResultType { get; set; }
    
    /// <summary>
    /// 如果ResultType是摆位，则表示摆位的进攻方
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public Side Actor { get; set; }

    /// <summary>
    /// 如果ResultType是摆位，则表示摆位的具体原因
    /// </summary>
    [JsonProperty]
    public string Reason { get; set; }

    /// <summary>
    /// 如果不为Nobody，则表明当前拍有队伍进球
    /// </summary>
    [JsonProperty]
    [JsonConverter(typeof(StringEnumConverter))]
    public Side WhoGoal { get; set; }

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

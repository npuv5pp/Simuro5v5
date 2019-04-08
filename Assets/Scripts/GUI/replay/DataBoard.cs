using Simuro5v5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理数据板
/// </summary>
public class DataBoard : MonoBehaviour
{
    public DataBoardContent content;

    public void Render(MatchInfo matchinfo)
    {
        content.Render(matchinfo);
    }
}

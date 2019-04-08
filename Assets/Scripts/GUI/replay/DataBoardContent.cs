using UnityEngine.UI;
using UnityEngine;
using Simuro5v5;

/// <summary>
/// 管理数据板的内容
/// </summary>
public class DataBoardContent : MonoBehaviour
{
    public GameObject template;

    RobotDataModel[] blues;
    RobotDataModel[] yellows;
    BallDataModel ball;

    void Start()
    {
        ball = transform.Find("ball").gameObject.GetComponent<BallDataModel>();
        blues = new RobotDataModel[5];
        yellows = new RobotDataModel[5];

        for (int i = 0; i < 5; i++)
        {
            var go = Instantiate(template, transform);
            go.name = $"blue{i}";
            blues[i] = go.GetComponent<RobotDataModel>();
            blues[i].SetNameColor(Const.Style.SideBlue);
            blues[i].SetName($"B{i}");
        }
        for (int i = 0; i < 5; i++)
        {
            var go = Instantiate(template, transform);
            go.name = $"yellow{i}";
            yellows[i] = go.GetComponent<RobotDataModel>();
            yellows[i].SetNameColor(Const.Style.SideYellow);
            yellows[i].SetName($"Y{i}");
        }
        Destroy(template);

        GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
    }

    public void Render(MatchInfo matchinfo)
    {
        for (int i = 0; i < 5; i++)
        {
            blues[i].RenderData(matchinfo.BlueRobots[i]);
            yellows[i].RenderData(matchinfo.YellowRobots[i]);
        }
        ball.RenderData(matchinfo.Ball);
    }
}

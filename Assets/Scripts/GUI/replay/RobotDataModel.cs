using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Simuro5v5;

public class RobotDataModel : MonoBehaviour
{
    public Text rowname;
    public InputField x, y, rotation;

    string PlayerName { get; set; }
    Robot Robot { get; set; }

    private void Start()
    {
        if (name.ToLower().StartsWith("blue"))
        {
            rowname.color = Color.blue;
        }
        else if (name.ToLower().StartsWith("yellow"))
        {
            rowname.color = Color.yellow;
        }

    }

    private void Update()
    {
        rowname.text = PlayerName;
        x.text = Robot.pos.x.ToString();
        y.text = Robot.pos.y.ToString();
        rotation.text = Robot.rotation.ToString();
    }

    /// <summary>
    /// 渲染数据
    /// </summary>
    /// <param name="data"></param>
    public void RenderData(Robot robot)
    {
        Robot = robot;
    }

    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="name"></param>
    public void SetName(string name)
    {
        PlayerName = name;
    }
}

using Newtonsoft.Json;
using Simuro5v5;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.IO;
using Random = System.Random;
using System;
using SFB;

public class RefereeTestMain : MonoBehaviour
{
    public GameObject entity;
    public MouseDrag mouseDrag;

    ObjectManager objectManager;
    MatchInfo matchInfo;
    MatchInfo preMatchInfo;

    public RefereeTestMain()
    {

    }

    void Start()
    {
        matchInfo = new MatchInfo();
        objectManager = new ObjectManager();
        objectManager.RebindObject(entity);
        objectManager.RebindMatchInfo(matchInfo);
        objectManager.DisablePhysics();
        mouseDrag.dragEnabled = true;
    }

    void Update()
    {
        
    }

    public MatchInfo RandomMatchInfo()
    {
        Random rd = new Random();
        var matchInfo = new MatchInfo()
        {
            Ball = new Ball() { pos = new Vector2D(rd.Next(-100, 100), rd.Next(-80, 80)) },
            BlueRobots = new Robot[]
                {
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                },
            YellowRobots = new Robot[]
                {
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                },
        };
        return matchInfo;
    }

    //自测时发现错误，导出到文件中
    public void ExportFail()
    {
        objectManager.UpdateFromScene();
        string path = StandaloneFileBrowser.SaveFilePanel(
            "Export Fail Record",
            "H:\\V5++\\UnityProject\\Bug\\",
            $"{DateTime.Now:yyyy-MM-dd_hhmmss}" + "--" ,
            "json");

        // if save file panel cancelled
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        string failInfo = JsonConvert.SerializeObject(matchInfo);
        File.WriteAllText(path, failInfo);

    }

    //将文件导入场景中
    public void ImportFail()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel(
            "Import Fail Dacord",
            "H:\\V5++\\UnityProject\\Bug\\",
            "json",
            false);

        // if open file panel cancelled
        if (path.Length == 0)
        {
            return;
        }

        string json = File.ReadAllText(path[0]);
        matchInfo = JsonConvert.DeserializeObject<MatchInfo>(json);
        objectManager.RevertScene(matchInfo);

    }

    public void RandomPlace()
    {
        Random rd = new Random();
        var matchInfo = new MatchInfo()
        {
            Ball = new Ball() { pos = new Vector2D(rd.Next(-100, 100), rd.Next(-80, 80)) },
            BlueRobots = new Robot[]
                {
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                },
            YellowRobots = new Robot[]
                {
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                new Robot() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
                },
        };
        preMatchInfo = matchInfo;
        objectManager.RevertScene(matchInfo);
    }

    public void RecoverPlace()
    {
        objectManager.RevertScene(preMatchInfo);
    }

    public void PenaltyBluePlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Blue,
            ResultType = ResultType.PenaltyKick
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void PenaltyYellowPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Yellow,
            ResultType = ResultType.PenaltyKick
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void PlaceBluePlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Blue,
            ResultType = ResultType.PlaceKick
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void PlaceYellowPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Yellow,
            ResultType = ResultType.PlaceKick
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void GoalieBluePlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Blue,
            ResultType = ResultType.GoalKick
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void GoalieYellowPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Yellow,
            ResultType = ResultType.GoalKick
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void FreeLeftBotPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Yellow,
            ResultType = ResultType.FreeKickLeftBot
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void FreeLeftTopPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Yellow,
            ResultType = ResultType.FreeKickLeftTop
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }
    public void FreeRiBotPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Blue,
            ResultType = ResultType.FreeKickRightBot
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }

    public void FreeRiTopPlace()
    {
        objectManager.UpdateFromScene();
        JudgeResult judgeResult = new JudgeResult
        {
            Actor = Side.Blue,
            ResultType = ResultType.FreeKickRightTop
        };
        matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult);
        objectManager.RevertScene(matchInfo);
    }
}

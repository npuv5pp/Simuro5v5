using Simuro5v5;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = System.Random;

public class RefereeTestMain : MonoBehaviour
{
    public GameObject entity;
    public MouseDrag mouseDrag;

    ObjectManager objectManager;
    MatchInfo matchInfo;
    MatchInfo preMatchInfo;

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

    private MatchInfo RandomMatchInfo()
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

    public void TestAuto()
    {
        int testtimes = 10;
        int NeedChange = 0;
        for(int i = 0;i<testtimes;i++)
        {
            bool IsNeedChange = false;
            objectManager.RevertScene(RandomMatchInfo());
            PenaltyBluePlace();
            objectManager.UpdateFromScene();
            JudgeResult judgeResult = new JudgeResult
            {
                Actor = Side.Blue,
                ResultType = ResultType.PenaltyKick
            };
            matchInfo.Referee.JudgeAutoPlacement(matchInfo, judgeResult,IsNeedChange);
            if(IsNeedChange)
            {
                NeedChange++;
                Debug.Log(i + "次测试错误");
            }
            else
            {
                Debug.Log(i+ "次测试正确");
            }
            
        }
        Debug.Log("错误率为：" + NeedChange / testtimes);
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
}

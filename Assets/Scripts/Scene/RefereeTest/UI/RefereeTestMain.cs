using Simuro5v5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class RefereeTestMain : MonoBehaviour
{
    public GameObject entity;
    public MouseDrag mouseDrag;

    ObjectManager objectManager;
    MatchInfo matchInfo;

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

    public void RandomPlace()
    {
        Random rd = new Random();
        var matchInfo = new MatchInfo()
        {
            Ball = new Ball() { pos = new Vector2D(rd.Next(-100,100), rd.Next(-80, 80)) },
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
        objectManager.RevertScene(matchInfo);
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
}

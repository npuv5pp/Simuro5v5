using Simuro5v5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

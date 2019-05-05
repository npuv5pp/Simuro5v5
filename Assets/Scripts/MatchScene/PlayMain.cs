using System;
using System.Collections;
using UnityEngine;
using Simuro5v5;
using Simuro5v5.Config;
using Simuro5v5.Strategy;
using Event = Simuro5v5.EventSystem.Event;

public class PlayMain : MonoBehaviour
{
    /// <summary>
    /// 在比赛运行的整个时间内为真
    /// </summary>
    public bool Started { get; private set; }
    
    /// <summary>
    /// 在比赛暂停时为真
    /// </summary>
    public bool Paused { get; private set; }

    // 策略已经加载成功
    public bool LoadSucceed => StrategyManager.IsBlueReady && StrategyManager.IsYellowReady;

    int timeTick = 0;       // 时间计时器。每次物理拍加一。FixedUpdate奇数拍运行，偶数拍跳过.
    public static GameObject Singleton;
    public StrategyManagerRPC StrategyManager { get; private set; }
    public MatchInfo GlobalMatchInfo { get; private set; }
    public static ObjectManager ObjectManager { get; private set; }

    public delegate void TimedPauseCallback();
    bool TimedPausing { get; set; }
    readonly object timedPausingLock = new object();

    // 进入场景之后
    void OnEnable()
    {
        if (Singleton != null)
        {
            if (gameObject != Singleton)
            {
                // 此时新的gameObject已经创建，调用DestroyImmediate而不是Destroy以确保新的go不会与已存在的go碰撞
                DestroyImmediate(gameObject);
            }
            else
            {
                ObjectManager.Pause();
            }
            // 激活单例
            Singleton.SetActive(true);
        }
    }

    IEnumerator Start()
    {
        ConfigManager.ReadConfigFile("config.json");
        Singleton = gameObject;
        DontDestroyOnLoad(GameObject.Find("/Entity"));

        StrategyManager = new StrategyManagerRPC();
        GlobalMatchInfo = MatchInfo.NewDefaultPreset();
        // 绑定物体
        ObjectManager = new ObjectManager();
        ObjectManager.RebindObject();
        ObjectManager.RebindMatchInfo(GlobalMatchInfo);
        Event.Register(Event.EventType0.PlaySceneExited, SceneExited);
        Event.Register(Event.EventType1.GetGoal, OnGetGoal);

        // 等待当前帧渲染完毕后暂停，确保还原后的场景显示到屏幕上
        yield return new WaitForEndOfFrame();
        ObjectManager.Pause();
    }

    void FixedUpdate()
    {
        timeTick++;
        
        if (timeTick % 2 == 0) // 偶数拍
        {
            return;
        }

        ObjectManager.UpdateFromScene();

        
        if (LoadSucceed && Started)
        {
            InMatchLoop();
        }

        //if (LoadSucceed && StartedMatch)
        //{
        //    if (InRound)
        //    {
        //        // 回合进行中
        //        InRoundLoop();
        //    }
        //    else if (InPlacement)
        //    {
        //        // 回合结束
        //        // 自动摆位
        //        if (!autoPlaced)
        //        {
        //            // 摆位时状态机不会停止运行，在这里确保不会运行两次摆位函数
        //            autoPlaced = true;
        //            InRound = false;
        //            PauseForTime(3, delegate ()
        //            {
        //                AutoPlacement();
        //                GlobalMatchInfo.TickMatch++;
        //                InPlacement = false;
        //                autoPlaced = false;
        //                StartRound();
        //            });
        //        }
        //    }
        //}
    }

    void OnGetGoal(object obj)
    {
        Side who = (Side)obj;
        switch (who)
        {
            case Side.Blue:
                GlobalMatchInfo.Score.BlueScore++;
                break;
            case Side.Yellow:
                GlobalMatchInfo.Score.YellowScore++;
                break;
        }
    }

    public void InMatchLoop()
    {
        if (Paused) return;

        try
        {
            JudgeResult judgeResult = GlobalMatchInfo.Referee.Judge(GlobalMatchInfo);

            if (judgeResult.ResultType == ResultType.EndGame)
            {
                // 时间到，比赛结束
                Debug.Log("Game Over");
                StopMatch();
            }
            else if (judgeResult.ResultType != ResultType.NormalMatch)
            {
                // 需要摆位
                Debug.Log("placing...");

                PauseForTime(3, () =>
                {
                    UpdatePlacementToScene(judgeResult.Actor.ToAnother());
                    GlobalMatchInfo.TickMatch++;

                    Event.Send(Event.EventType1.AutoPlacement, GlobalMatchInfo);
                    Event.Send(Event.EventType1.RefereeLogUpdate, judgeResult.ToRichText());
                });
            }
            else
            {
                // 正常比赛
                UpdateWheelsToScene();
                GlobalMatchInfo.TickMatch++;

                Event.Send(Event.EventType1.MatchInfoUpdate, GlobalMatchInfo);
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            StopMatch();
        }
    }

    /// <summary>
    /// 新比赛开始
    /// 重设场景，时间清空，裁判历史数据清空
    /// </summary>
    public void StartMatch()
    {
        Started = false;

        ObjectManager.SetToDefault();
        ObjectManager.SetStill();
        GlobalMatchInfo.Score = new MatchScore();
        GlobalMatchInfo.TickMatch = 0;
        GlobalMatchInfo.Referee = new Referee();

        StrategyManager.BlueStrategy.OnMatchStart();
        StrategyManager.YellowStrategy.OnMatchStart();

        Started = true;
        Paused = true;
        Event.Send(Event.EventType1.MatchStart, GlobalMatchInfo);
    }

    public void StopMatch()
    {
        GlobalMatchInfo.TickMatch = 0;
        Started = false;
        Paused = true;
        ObjectManager.Pause();

        StrategyManager.BlueStrategy.OnMatchStop();
        StrategyManager.YellowStrategy.OnMatchStop();
        Event.Send(Event.EventType1.MatchStop, GlobalMatchInfo);
    }

    public void PauseMatch()
    {
        if (Started)
        {
            Paused = true;
            ObjectManager.Pause();
        }
    }

    public void ResumeMatch()
    {
        if (Started)
        {
            Paused = false;
            ObjectManager.Resume();
        }
    }

    void UpdateWheelsToScene()
    {
        WheelInfo wheelsBlue = StrategyManager.BlueStrategy.GetInstruction(GlobalMatchInfo.GetSide(Side.Blue));
        WheelInfo wheelsYellow = StrategyManager.YellowStrategy.GetInstruction(GlobalMatchInfo.GetSide(Side.Yellow));
        wheelsBlue.Normalize();     //轮速规整化
        wheelsYellow.Normalize();   //轮速规整化

        ObjectManager.SetBlueWheels(wheelsBlue);
        ObjectManager.SetYellowWheels(wheelsYellow);
    }

    void UpdatePlacementToScene(Side whosball)
    {
        PlacementInfo blueInfo = StrategyManager.BlueStrategy.GetPlacement(GlobalMatchInfo.GetSide(Side.Blue));
        PlacementInfo yellowInfo = StrategyManager.YellowStrategy.GetPlacement(GlobalMatchInfo.GetSide(Side.Yellow));
        blueInfo.Normalize();
        yellowInfo.Normalize();

        if (GeneralConfig.EnableConvertYellowData)
            yellowInfo.ConvertToOtherSide();

        ObjectManager.SetBluePlacement(blueInfo);
        ObjectManager.SetYellowPlacement(yellowInfo);
        ObjectManager.SetStill();

        if (whosball == Side.Blue)                        // 先摆后摆另考虑
        {
            ObjectManager.SetBallPlacement(blueInfo);
        }
        else
        {
            ObjectManager.SetBallPlacement(yellowInfo);
        }
    }

    public void LoadStrategy(Side side, string endpoint)
    {
        switch (side)
        {
            case Side.Blue:
                StrategyManager.ConnectBlue(endpoint);
                break;
            case Side.Yellow:
                StrategyManager.ConnectYellow(endpoint);
                break;
        }
    }

    public void LoadStrategy(string blueEndpoint, string yellowEndpoint)
    {
        StrategyManager.ConnectBlue(blueEndpoint);
        StrategyManager.ConnectYellow(yellowEndpoint);
    }

    public void RemoveStrategy(Side side)
    {
        switch (side)
        {
            case Side.Blue:
                StrategyManager.CloseBlue();
                break;
            case Side.Yellow:
                StrategyManager.CloseYellow();
                break;
        }
    }

    public void RemoveStrategy()
    {
        StrategyManager.CloseBlue();
        StrategyManager.CloseYellow();
    }

    private void PauseForTime(int sec, TimedPauseCallback callback)
    {
        if (sec > 0)
        {
            PauseMatch();
            StartCoroutine(_PauseCoroutine(sec, callback));
        }
    }

    IEnumerator _PauseCoroutine(float sec, TimedPauseCallback callback)
    {
        yield return new WaitUntil(delegate ()
        {
            lock (timedPausingLock)
            {
                return TimedPausing == false;
            }
        });
        TimedPausing = true;
        yield return new WaitForSecondsRealtime(sec);
        callback();
        ResumeMatch();
        TimedPausing = false;
    }

    void SceneExited()
    {
        gameObject.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        Event.Send(Event.EventType0.PlatformExiting);
    }
}

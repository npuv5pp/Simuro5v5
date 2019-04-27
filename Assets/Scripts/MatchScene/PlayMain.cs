using System;
using System.Collections;
using UnityEngine;
using Simuro5v5;
using Simuro5v5.Config;
using Simuro5v5.Strategy;
using Event = Simuro5v5.EventSystem.Event;

public class PlayMain : MonoBehaviour
{
    public bool debugging;

    // 比赛已经开始
    public bool StartedMatch
    {
        get => GlobalMatchInfo.ControlState.StartedMatch;
        private set => GlobalMatchInfo.ControlState.StartedMatch = value;
    }
    // 策略已经加载成功
    public bool LoadSucceed => StrategyManager.IsBlueReady && StrategyManager.IsYellowReady;

    // 回合已经开始
    public bool InRound
    {
        get => GlobalMatchInfo.ControlState.InRound;
        private set => GlobalMatchInfo.ControlState.InRound = value;
    }
    // 回合已经暂停
    public bool PausedRound
    {
        get => GlobalMatchInfo.ControlState.PausedRound;
        private set => GlobalMatchInfo.ControlState.PausedRound = value;
    }
    // 在摆位中
    public bool InPlacement
    {
        get => GlobalMatchInfo.ControlState.InPlacement;
        private set => GlobalMatchInfo.ControlState.InPlacement = value;
    }

    int timeTick = 0;       // 时间计时器。每次物理拍加一。FixedUpdate奇数拍运行，偶数拍跳过.
    public static GameObject Singleton;
    public StrategyManagerRPC StrategyManager { get; private set; }
    public MatchInfo GlobalMatchInfo { get; private set; }
    public static ObjectManager ObjectManager { get; private set; }

    public delegate void TimedPauseCallback();
    bool TimedPausing { get; set; }
    readonly object timedPausingLock = new object();

    bool autoPlaced = false;

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
        Singleton = gameObject;
        DontDestroyOnLoad(GameObject.Find("/Entity"));
        ConfigManager.ReadConfigFile("config.json");

        // 绑定物体
        ObjectManager = new ObjectManager();
        NewMatch();
        ObjectManager.RebindObject();
        ObjectManager.RebindMatchInfo(GlobalMatchInfo);
        Event.Register(Event.EventType0.PlaySceneExited, SceneExited);

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
        if (LoadSucceed && StartedMatch)
        {
            if (InRound)
            {
                // 回合进行中
                InRoundLoop();
            }
            else if (InPlacement)
            {
                // 回合结束
                // 自动摆位
                if (!autoPlaced)
                {
                    // 摆位时状态机不会停止运行，在这里确保不会运行两次摆位函数
                    autoPlaced = true;
                    InRound = false;
                    PauseForTime(3, delegate ()
                    {
                        AutoPlacement();
                        GlobalMatchInfo.TickMatch++;
                        InPlacement = false;
                        autoPlaced = false;
                        StartRound();
                    });
                }
            }
        }
    }

    void NewMatch()
    {
        StrategyManager?.CloseBlue();
        StrategyManager?.CloseYellow();

        StrategyManager = new StrategyManagerRPC();
        GlobalMatchInfo = MatchInfo.NewDefaultPreset();

        Event.Register(Event.EventType1.GetGoal, delegate (object obj)
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
        });
    }

    public void InRoundLoop()
    {
        if (PausedRound)
        {
            return;
        }

        try
        {
            // 广播信息
            Event.Send(Event.EventType1.MatchInfoUpdate, GlobalMatchInfo);

            // 裁判判断
            JudgeResult judgeResult = GlobalMatchInfo.Referee.Judge(GlobalMatchInfo);
            if (judgeResult.ResultType != ResultType.NormalMatch)
            {
                InRound = false;
                InPlacement = true;
                GlobalMatchInfo.Referee = new Referee();

                Event.Send(Event.EventType1.RefereeLogUpdate, judgeResult.ToRichText());
                Event.Send(Event.EventType1.AutoPlacement, GlobalMatchInfo);
            }
            else
            {
                UpdateWheelsToScene();
                GlobalMatchInfo.TickMatch++;
                GlobalMatchInfo.TickRound++;
            }
        }
        catch (Exception)
        {
            StopMatch();
            throw;
        }
    }

    /// <summary>
    /// 新比赛开始
    /// 重设场景，比分清空，裁判历史数据清空
    /// </summary>
    public void StartMatch()
    {
        GlobalMatchInfo.ControlState.Reset();
        StartedMatch = false;

        ObjectManager.SetToDefault();
        ObjectManager.SetStill();
        GlobalMatchInfo.Score = new MatchScore();
        GlobalMatchInfo.TickMatch = 1;
        GlobalMatchInfo.TickRound = 1;
        GlobalMatchInfo.Referee = new Referee();

        StrategyManager.BlueStrategy.OnMatchStart();
        StrategyManager.YellowStrategy.OnMatchStart();

        StartedMatch = true;
        Event.Send(Event.EventType1.MatchStart, GlobalMatchInfo);
    }

    public void StopMatch()
    {
        GlobalMatchInfo.ControlState.Reset();
        ObjectManager.Pause();

        StrategyManager.BlueStrategy.OnMatchStop();
        StrategyManager.YellowStrategy.OnMatchStop();
        Event.Send(Event.EventType1.MatchStop, GlobalMatchInfo);
    }

    public void StartRound()
    {
        if (StartedMatch)
        {
            InRound = true;
            GlobalMatchInfo.TickRound = 1;
            PauseRound();

            StrategyManager.BlueStrategy.OnRoundStart();
            StrategyManager.YellowStrategy.OnRoundStart();
            Event.Send(Event.EventType1.RoundStart, GlobalMatchInfo);
        }
    }

    public void StartRoundAfterFrame()
    {
        StartCoroutine(AwaitStartRound());
    }

    IEnumerator AwaitStartRound()
    {
        yield return new WaitForFixedUpdate();
        StartRound();
    }

    public void PauseRound()
    {
        if (InRound)
        {
            PausedRound = true;
            ObjectManager.Pause();

            Event.Send(Event.EventType1.RoundPause, GlobalMatchInfo);
        }
    }

    private void PauseForTime(int sec, TimedPauseCallback callback)
    {
        if (sec > 0)
        {
            ObjectManager.Pause();
            StartCoroutine(PauseCoroutine(sec, callback));
        }
    }

    IEnumerator PauseCoroutine(float sec, TimedPauseCallback callback)
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
        TimedPausing = false;
    }

    public void ResumeRound()
    {
        if (InRound && PausedRound)
        {
            PausedRound = false;
            ObjectManager.Resume();
            Event.Send(Event.EventType1.RoundResume, GlobalMatchInfo);
        }
    }

    public void StopRound()
    {
        PauseRound();
        InRound = false;
        StrategyManager.BlueStrategy.OnRoundStop();
        StrategyManager.YellowStrategy.OnRoundStop();
        ObjectManager.Pause();
        Event.Send(Event.EventType1.RoundStop, GlobalMatchInfo);
    }

    public void AutoPlacement()
    {
        if (StartedMatch)
        {
            Debug.Log("auto placement...");
            UpdatePlacementToScene();
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

    void UpdateWheelsToScene()
    {
        WheelInfo wheelsBlue = StrategyManager.BlueStrategy.GetInstruction(GlobalMatchInfo.GetSide(Side.Blue));
        WheelInfo wheelsYellow = StrategyManager.YellowStrategy.GetInstruction(GlobalMatchInfo.GetSide(Side.Yellow));
        wheelsBlue.Normalize();     //轮速规整化
        wheelsYellow.Normalize();   //轮速规整化

        ObjectManager.SetBlueWheels(wheelsBlue);
        ObjectManager.SetYellowWheels(wheelsYellow);
    }

    void UpdatePlacementToScene()
    {
        ObjectManager.UpdateFromScene();
        PlacementInfo blueInfo = StrategyManager.BlueStrategy.GetPlacement(GlobalMatchInfo.GetSide(Side.Blue));
        PlacementInfo yellowInfo = StrategyManager.YellowStrategy.GetPlacement(GlobalMatchInfo.GetSide(Side.Yellow));
        blueInfo.Normalize();
        yellowInfo.Normalize();
        if (GeneralConfig.EnableConvertYellowData)
        {
            yellowInfo.ConvertToOtherSide();
        }
        ObjectManager.SetBluePlacement(blueInfo);
        ObjectManager.SetYellowPlacement(yellowInfo);
        ObjectManager.SetStill();

        if (GlobalMatchInfo.WhosBall == 0)                        // 先摆后摆另考虑
        {
            ObjectManager.SetBallPlacement(blueInfo);
        }
        else
        {
            ObjectManager.SetBallPlacement(yellowInfo);
        }

        Event.Send(Event.EventType1.MatchInfoUpdate, GlobalMatchInfo);
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

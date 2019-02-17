using System;
using System.Collections;
using UnityEngine;
using Simuro5v5;
using Simuro5v5.Config;
using Simuro5v5.Strategy;
using Logger = Simuro5v5.Logger;
using Event = Simuro5v5.EventSystem.Event;

public class MatchMain : MonoBehaviour
{
    // 比赛已经开始
    public bool StartedMatch
    {
        get { return GlobalMatchInfo.ControlState.StartedMatch; }
        set { GlobalMatchInfo.ControlState.StartedMatch = value; }
    }
    // 策略已经加载成功
    public bool LoadSucceed
    {
        get { return GlobalMatchInfo.ControlState.LoadSucceed; }
        set { GlobalMatchInfo.ControlState.LoadSucceed = value; }
    }
    // 回合已经开始
    public bool InRound
    {
        get { return GlobalMatchInfo.ControlState.InRound; }
        set { GlobalMatchInfo.ControlState.InRound = value; }
    }
    // 回合已经暂停
    public bool PausedRound
    {
        get { return GlobalMatchInfo.ControlState.PausedRound; }
        set { GlobalMatchInfo.ControlState.PausedRound = value; }
    }
    // 在摆位中
    public bool InPlacement
    {
        get { return GlobalMatchInfo.ControlState.InPlacement; }
        set { GlobalMatchInfo.ControlState.InPlacement = value; }
    }

    private int PauseTime { get; set; }

    private bool PauseAfterUpdate { get; set; }
    public static bool CreateNewScene { get; set; }
    public static StrategyManager StrategyManager { get; private set; }
    public static MatchInfo GlobalMatchInfo { get; private set; }
    //public static GameObject Entity { get; private set; }

    static Watcher Watcher { get; set; }
    Logger logger { get; set; }

    IEnumerator Start()
    {
        // TODO 第一时间读取配置文件
        ConfigManager.ReadConfigFile("config.json");

        Time.fixedDeltaTime = Const.Zeit;

        logger = Logger.MainLogger;
        if (Watcher == null)
        {
            Watcher = new Watcher();
            // Watcher.RegisterHook();
        }

        // 绑定物体
        ObjectManager.RebindObject();
        if (GlobalMatchInfo != null && !CreateNewScene)
        {
            // 还原场景
            ObjectManager.RevertScene(GlobalMatchInfo);
            Debug.Log("Reverting");
        }
        else
        {
            // 新建场景
            NewMatch();
        }
        // 绑定MatchInfo
        ObjectManager.RebindMatchInfo(GlobalMatchInfo);

        // 暂停世界
        // 等待当前帧渲染完毕后暂停，确保还原后的场景显示到屏幕上
        yield return _pauseAfterFrame();
    }

    IEnumerator _pauseAfterFrame()
    {
        // 等待当前帧渲染完毕后暂停，确保当前帧已经显示在屏幕上
        yield return new WaitForEndOfFrame();
        ObjectManager.Pause();
    }

    void FixedUpdate()
    {
        ObjectManager.UpdateFromScene();
        if (LoadSucceed && StartedMatch)
        {
            try
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
                    Debug.Log("InPlacement");

                    GlobalMatchInfo.PlayTime++;
                    AutoPlacement();
                    InPlacement = false;
                    StartRound();
                }
            }
            catch (Exception ex)
            {
                StopMatch();
                LoadSucceed = false;
                logger.LogError(ex.Message);
                throw;  // 直接抛出，让Unity3d处理
            }
        }
    }

    void NewMatch()
    {
        Debug.Log("new match");
        if (StrategyManager != null)
        {
            StrategyManager.Dispose();
        }

        StrategyManager = new StrategyManager();
        GlobalMatchInfo = new MatchInfo();

        Event.Register(Event.EventType1.Goal, delegate (object obj)
        {
            bool who = (bool)obj;
            if (who)
            {
                GlobalMatchInfo.Score.BlueScore++;
            }
            else
            {
                GlobalMatchInfo.Score.YellowScore++;
            }
        });
        //LoadInfo.SaveInfo("StrategyServer/dlltest/debug/dlltest.dll", "StrategyServer/dll2.dll");
    }

    public void InRoundLoop()
    {
        if (PausedRound)
        {
            return;
        }

        try
        {
            // 检查策略
            StrategyManager.CheckBlueReady_ex();
            StrategyManager.CheckYellowReady_ex();

            // 广播信息
            Event.Send(Event.EventType1.MatchInfoUpdate, GlobalMatchInfo);

            // 裁判判断
            if (GlobalMatchInfo.Referee.Judge(GlobalMatchInfo))
            {
                InRound = false;
                InPlacement = true;
                GlobalMatchInfo.Referee = new Referee();
                Event.Send(Event.EventType1.LogUpdate, "Foul : " + GlobalMatchInfo.GameState.ToString() + ". Blue team is" + ((Simuro5v5.Side)GlobalMatchInfo.WhosBall).ToString());
            }
            else
            {
                UpdateWheelsToScene();
                GlobalMatchInfo.PlayTime++;
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
        StartedMatch = false;

        ObjectManager.SetDefaultPostion();
        GlobalMatchInfo.Score = new MatchScore();
        GlobalMatchInfo.PlayTime = 0;
        GlobalMatchInfo.Referee = new Referee();

        StrategyManager.CheckReady_ex();
        StrategyManager.BeginBlue(GlobalMatchInfo);
        StrategyManager.BeginYellow(GlobalMatchInfo);

        StartedMatch = true;
        Event.Send(Event.EventType0.MatchStart);
    }

    public void StopMatch()
    {
        StartedMatch = false;
        StopRound();
        InPlacement = false;
        ObjectManager.Pause();
        Event.Send(Event.EventType0.MatchStop);
    }

    public void StartRound()
    {
        if (StartedMatch)
        {
            InRound = true;
            PauseRound();
            Event.Send(Event.EventType0.RoundStart);
        }
    }

    public void PauseRound()
    {
        if (InRound)
        {
            PausedRound = true;
            ObjectManager.Pause();
            Event.Send(Event.EventType0.RoundPause);
        }
    }

    public void PauseForTime(int time)
    {
        if (time > 0)
        {
            ObjectManager.Pause();
            PauseTime = time;
        }
    }

    public void ResumeRound()
    {
        if (InRound && PausedRound)
        {
            PausedRound = false;
            ObjectManager.Resume();
            Event.Send(Event.EventType0.RoundResume);
        }
    }

    public void StopRound()
    {
        PauseRound();
        InRound = false;
        ObjectManager.Pause();
        Event.Send(Event.EventType0.RoundStop);
    }

    public void AutoPlacement()
    {
        if (StartedMatch)
        {
            Debug.Log("auto placementing");
            StopRound();
            UpdatePlacementToScene();
        }
    }

    public void LoadStrategy()
    {
        LoadSucceed = false;
        StrategyManager.LoadLastSaved();
        LoadSucceed = true;
    }

    public void RemoveStrategy()
    {
        StrategyManager.RemoveBlueDll();
        StrategyManager.RemoveYellowDll();
    }

    void UpdateWheelsToScene()
    {
        WheelInfo wheelsblue = StrategyManager.NextBlue(GlobalMatchInfo);
        WheelInfo wheelsyellow = StrategyManager.NextYellow(GlobalMatchInfo);
        wheelsblue.Normalize();     //轮速规整化
        wheelsyellow.Normalize();   //轮速规整化

        ObjectManager.SetBlueWheelInfo(wheelsblue);
        ObjectManager.SetYellowWheelInfo(wheelsyellow);
    }

    void UpdatePlacementToScene()
    {
        PlacementInfo blueInfo = StrategyManager.PlacementBlue(GlobalMatchInfo);
        PlacementInfo yellowInfo = StrategyManager.PlacementYellow(GlobalMatchInfo);
        ObjectManager.SetBluePlacement(blueInfo);
        ObjectManager.SetYellowPlacement(yellowInfo);

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
}

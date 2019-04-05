using System;
using System.Collections;
using UnityEngine;
using Simuro5v5;
using Simuro5v5.Config;
using Simuro5v5.Strategy;
using Logger = Simuro5v5.Logger;
using Event = Simuro5v5.EventSystem.Event;

public class PlayMain : MonoBehaviour
{
    public bool Debugging;
    public Wheel[] DebugWheels;

    // 比赛已经开始
    public bool StartedMatch
    {
        get { return GlobalMatchInfo.ControlState.StartedMatch; }
        set { GlobalMatchInfo.ControlState.StartedMatch = value; }
    }
    // 策略已经加载成功
    public bool LoadSucceed
    {
        get { return StrategyManager.IsBlueReady && StrategyManager.IsYellowReady; }
        //get { return GlobalMatchInfo.ControlState.LoadSucceed; }
        //set { GlobalMatchInfo.ControlState.LoadSucceed = value; }
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

    public static GameObject Singleton;
    public StrategyManager StrategyManager { get; private set; }
    public MatchInfo GlobalMatchInfo { get; private set; }
    public static ObjectManager ObjectManager { get; private set; }

    public delegate void TimedPauseCallback();
    bool TimedPausing { get; set; }
    readonly object TimedPausingLock = new object();

    // 进入场景之后
    void OnEnable()
    {
        if (Singleton != null)
        {
            if (gameObject != Singleton)
            {
                // 此时新的gameobject已经创建，调用DestroyImmediate而不是Destroy以确保新的go不会与已存在的go碰撞
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
                PauseForTime(3, delegate ()
                {
                    AutoPlacement();
                    GlobalMatchInfo.PlayTime++;
                    InPlacement = false;
                    //StartRoundAfterFrame();
                    StartRound();
                });
            }
        }
    }

    void NewMatch()
    {
        if (StrategyManager != null)
        {
            StrategyManager.Dispose();
        }

        StrategyManager = new StrategyManager();
        GlobalMatchInfo = MatchInfo.DefaultMatch;

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

                string log;
                if (GlobalMatchInfo.WhosBall == Side.Blue)
                {
                    log = string.Format("Foul: {0}. <b><color=\"#0057FF\">Blue</color></b> team is offensive side",
                         GlobalMatchInfo.GameState);
                }
                else
                {
                    log = string.Format("Foul: {0}. <b><color=\"#F8FF00\">Yellow</color></b> team is offensive side",
                         GlobalMatchInfo.GameState);
                }
                Event.Send(Event.EventType1.LogUpdate, log);
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
        GlobalMatchInfo.ControlState.Reset();
        StartedMatch = false;

        ObjectManager.SetToDefault();
        ObjectManager.SetStill();
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
        GlobalMatchInfo.ControlState.Reset();
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
            Event.Send(Event.EventType0.RoundPause);
        }
    }

    public void PauseForTime(int sec, TimedPauseCallback callback)
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
            lock (TimedPausingLock)
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
            UpdatePlacementToScene();
        }
    }

    public void LoadStrategy(string blue, string yellow)
    {
        if (Debugging)
        {
            if (DebugWheels == null)
            {
                StrategyManager.LoadBlueDebugStrategy();
                StrategyManager.LoadYellowDebugStrategy();
            }
            else
            {
                StrategyManager.LoadBlueDebugStrategy(new WheelInfo { Wheels = DebugWheels });
                StrategyManager.LoadYellowDebugStrategy(new WheelInfo { Wheels = DebugWheels });
            }
        }
        else
        {
            StrategyManager.LoadBlueDll(blue);
            StrategyManager.LoadYellowDll(yellow);
        }
    }

    public void LoadStrategy()
    {
        if (Debugging)
        {
            if (DebugWheels == null)
            {
                StrategyManager.LoadBlueDebugStrategy();
                StrategyManager.LoadYellowDebugStrategy();
            }
            else
            {
                StrategyManager.LoadBlueDebugStrategy(new WheelInfo { Wheels = DebugWheels });
                StrategyManager.LoadYellowDebugStrategy(new WheelInfo { Wheels = DebugWheels });
            }
        }
        else
        {
            StrategyManager.LoadLastSaved();
        }
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

        ObjectManager.SetBlueWheels(wheelsblue);
        ObjectManager.SetYellowWheels(wheelsyellow);
    }

    void UpdatePlacementToScene()
    {
        ObjectManager.UpdateFromScene();
        PlacementInfo blueInfo = StrategyManager.PlacementBlue(GlobalMatchInfo);
        PlacementInfo yellowInfo = StrategyManager.PlacementYellow(GlobalMatchInfo);
        blueInfo.Normalize();
        yellowInfo.Normalize();
        if (GeneralConfig.EnableConvertYellowData)
        {
            yellowInfo.ConvertToOtherSide();
        }
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

    void SceneExited()
    {
        gameObject.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        Event.Send(Event.EventType0.PlatformExiting);
    }
}

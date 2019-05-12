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

    public static GameObject Singleton;
    public StrategyManager StrategyManager { get; private set; }
    public MatchInfo GlobalMatchInfo { get; private set; }
    public ObjectManager ObjectManager { get; private set; }

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

        StrategyManager = new StrategyManager();
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

    // void FixedUpdate()
    // {
    //     timeTick++;
    //     // 偶数拍，跳过
    //     if (timeTick % 2 == 0) return;

    //     ObjectManager.UpdateFromScene();

    //     if (LoadSucceed && Started)
    //     {
    //         InMatchLoop();
    //     }
    // }

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
        if (!LoadSucceed || !Started || Paused) return;

        JudgeResult judgeResult = GlobalMatchInfo.Referee.Judge(GlobalMatchInfo);

        if (judgeResult.ResultType == ResultType.GameOver)
        {
            // 时间到，比赛结束
            Debug.Log("Game Over");
            StopMatch();
        }
        else if (judgeResult.ResultType == ResultType.EndHalf)
        {
            // 半场结束
            Debug.Log("End half");
            GlobalMatchInfo.TickMatch = 0;
        }
        else if (judgeResult.ResultType == ResultType.NormalMatch)
        {
            // 正常比赛
            UpdateWheelsToScene();
            GlobalMatchInfo.TickMatch++;
            Event.Send(Event.EventType1.MatchInfoUpdate, GlobalMatchInfo);
        }
        else
        {
            // 需要摆位
            Debug.Log("placing...");

            void Callback()
            {
                UpdatePlacementToScene(judgeResult);
                GlobalMatchInfo.TickMatch++;

                PauseForTime(2, () => { });
            }

            Event.Send(Event.EventType1.AutoPlacement, GlobalMatchInfo);

            if (GlobalMatchInfo.TickMatch > 0)
            {
                PauseForTime(2, Callback);
            }
            else
            {
                Callback();
            }
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
        GlobalMatchInfo.MatchState = MatchState.FirstHalf;
        GlobalMatchInfo.Referee = new Referee();

        StrategyManager.Blue.OnMatchStart();
        StrategyManager.Yellow.OnMatchStart();

        Started = true;
        Paused = true;
        Event.Send(Event.EventType1.MatchStart, GlobalMatchInfo);
    }

    /// <summary>
    /// 停止比赛
    /// </summary>
    /// <param name="willNotifyStrategies">是否向策略发送通知，如果是由于策略出现错误需要停止比赛，可以指定为false。默认为true</param>
    public void StopMatch(bool willNotifyStrategies=true)
    {
        GlobalMatchInfo.TickMatch = 0;
        Started = false;
        Paused = true;
        ObjectManager.Pause();

        Event.Send(Event.EventType1.MatchStop, GlobalMatchInfo);
        if (willNotifyStrategies)
        {
            StrategyManager.Blue.OnMatchStop();
            StrategyManager.Yellow.OnMatchStop();
        }
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
        WheelInfo wheelsBlue = StrategyManager.Blue.GetInstruction(GlobalMatchInfo.GetSide(Side.Blue));

        SideInfo yellow = GlobalMatchInfo.GetSide(Side.Yellow);
        if (GeneralConfig.EnableConvertYellowData)
        {
            yellow.ConvertToOtherSide();
        }
        WheelInfo wheelsYellow = StrategyManager.Yellow.GetInstruction(yellow);

        wheelsBlue.Normalize();     //轮速规整化
        wheelsYellow.Normalize();   //轮速规整化

        ObjectManager.SetBlueWheels(wheelsBlue);
        ObjectManager.SetYellowWheels(wheelsYellow);
    }

    void UpdatePlacementToScene(JudgeResult judgeResult)
    {
        var currMi = (MatchInfo)GlobalMatchInfo.Clone();
        PlacementInfo blueInfo;
        PlacementInfo yellowInfo;

        switch (judgeResult.WhoisFirst)
        {
            case Side.Blue:
                // 蓝方先摆位
                blueInfo = StrategyManager.Blue.GetPlacement(currMi.GetSide(Side.Blue));
                // 将蓝方返回的数据同步到currMi
                currMi.UpdateFrom(blueInfo.Robots, Side.Blue);
                // 黄方后摆位
                yellowInfo = StrategyManager.Yellow.GetPlacement(currMi.GetSide(Side.Yellow));

                // 转换数据
                if (GeneralConfig.EnableConvertYellowData)
                    yellowInfo.ConvertToOtherSide();

                break;
            case Side.Yellow:
                // 黄方先摆位
                yellowInfo = StrategyManager.Yellow.GetPlacement(currMi.GetSide(Side.Yellow));
                // 由于右攻假设，需要先将黄方数据转换
                if (GeneralConfig.EnableConvertYellowData)
                    yellowInfo.ConvertToOtherSide();

                // 将黄方返回的数据同步到currMi
                currMi.UpdateFrom(yellowInfo.Robots, Side.Yellow);
                // 蓝方后摆位
                blueInfo = StrategyManager.Blue.GetPlacement(currMi.GetSide(Side.Blue));
                break;
            default:
                throw new ArgumentException("Side cannot be Nobody");
        }

        // 从两方数据拼接MatchInfo，球的数据取决于judgeResult
        var mi = new MatchInfo(blueInfo, yellowInfo, judgeResult.Actor);
        GlobalMatchInfo.Referee.JudgeAutoPlacement(mi, judgeResult);

        // 设置场地
        ObjectManager.SetBluePlacement(mi.BlueRobots);
        ObjectManager.SetYellowPlacement(mi.YellowRobots);
        ObjectManager.SetBallPlacement(mi.Ball);

        ObjectManager.SetStill();
    }

    public bool LoadStrategy(string blue_ep, string yellow_ep)
    {
        var factory = new StrategyFactory
        {
            BlueEP = blue_ep,
            YellowEP = yellow_ep
        };
        StrategyManager.StrategyFactory = factory;

        try
        {
            StrategyManager.ConnectBlue();
        }
        catch (Exception e)
        {
            throw new StrategyException(Side.Blue, e);
        }

        try
        {
            StrategyManager.ConnectYellow();
        }
        catch (Exception e)
        {
            throw new StrategyException(Side.Yellow, e);
        }

        return true;
    }

    public void RemoveStrategy(Side side)
    {
        switch (side)
        {
            case Side.Blue:
                StrategyManager.Blue.Close();
                break;
            case Side.Yellow:
                StrategyManager.Yellow.Close();
                break;
        }
    }

    public void RemoveStrategy()
    {
        RemoveStrategy(Side.Blue);
        RemoveStrategy(Side.Yellow);
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
        ResumeMatch();
        callback();
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

    class StrategyFactory : IStrategyFactory
    {
        public string BlueEP { get; set; }
        public string YellowEP { get; set; }
        public IStrategy CreateBlue()
        {
            try
            {
                return new RPCStrategy(BlueEP);
            }
            catch (Exception e)
            {
                throw new StrategyException(Side.Blue, e);
            }
        }

        public IStrategy CreateYellow()
        {
            try
            {
                return new RPCStrategy(YellowEP);
            }
            catch (Exception e)
            {
                throw new StrategyException(Side.Yellow, e);
            }
        }
    }
}
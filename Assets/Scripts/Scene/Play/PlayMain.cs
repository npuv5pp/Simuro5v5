using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simuro5v5;
using Simuro5v5.Strategy;
using Event = Simuro5v5.EventSystem.Event;

/// <summary>
/// 这个类绑定在Entity物体上，用来维护比赛的状态，以及负责协调策略与场地。<br/>
/// 
/// 裁判的判定：
/// 裁判共有3类判定结果：结束一个阶段（NextPhase和GameOver）、正常比赛（NormalMatch）以及摆位（xxxKick）；
/// 另外要注意的是，裁判不会主动修改MatchInfo中的任何信息，所有信息由JudgeResult返回。<br/>
///
/// 判定结果的执行：
/// 结束一个阶段则结束一个阶段，并将时间设置为0；
/// 正常比赛则从策略中获取轮速，并将时间加一，触发事件；
/// 摆位则首先判断是否为进球引起的摆位（JudgeResult.WhoGoal），然后从策略中获取摆位信息，并将时间加一，触发事件。<br/>
/// 
/// 进球：
/// 进球会发生在摆位或者GameOver状态；
/// 正常情况下进球会引起摆位，但是如果进入点球大战的“突然死亡”模式，有可能进球伴随着GameOver状态。<br/>
///
/// 比赛阶段：
/// 一场比赛最多分为4个阶段：上半场、下半场、加时、点球大战；<br/>
/// 每个阶段都由一个摆位开始，由 <see cref="ResultType.NextPhase"/> 或者 <see cref="ResultType.GameOver"/> 结束；
/// 每个阶段结束后，不会清空比分，但会清空比赛时间，将比赛时间设置为0以使得下一拍的裁判得知这是一个新的阶段；
/// 每个阶段运行期间可能产生正常比赛和摆位两种状态。<br/>
/// 
/// 比赛时间：
/// 整场比赛时间为TickMatch，阶段时间为TickPhase。
/// 两者在正常比赛和摆位时递增，TickPhase在阶段结束后清空，TickPhase在比赛结束后清空。
/// 实际的比赛时间从1开始；指定为0表示切换到了新的阶段，不算做实际比赛时间；<br/>
/// 
/// 事件的触发：
/// 场地进行有效更新（摆位或物理引擎带轮速运行一拍）时，触发MatchInfoUpdate事件；摆位后会触发AutoPlacement事件。
/// 对事件的处理以“使得外部可以以其记录整个比赛进程”为准<br/>
/// </summary>
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

    public Action OnNextPhase;
    public Action OnMatchStart;

    #region 对暂停的补丁

    // 由于有时暂停会无效，所以需要补丁

    private int placementCount = 0;
    private int callbackCount = 0;
    private int lastPlacementId = 0;
    private Queue<int> placementToIgnore = new Queue<int>();

    #endregion

    /// <summary>
    /// 进入场景之后。如果已经有实例在运行，立即销毁所绑定的Entity；否则激活已存在的单例
    /// </summary>
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
        Time.fixedDeltaTime = Const.FixedDeltaTime;
        Configuration.ReadFromFileOrCreate("config.json");

        if (gameObject.name != "Entity") throw new ArgumentException("PlayMain is not binding in an Entity");
        Singleton = gameObject;
        DontDestroyOnLoad(gameObject);

        StrategyManager = new StrategyManager();
        GlobalMatchInfo = MatchInfo.NewDefaultPreset();
        // 绑定物体
        ObjectManager = new ObjectManager();
        ObjectManager.RebindObject();
        ObjectManager.RebindMatchInfo(GlobalMatchInfo);
        Event.Register(Event.EventType0.PlaySceneExited, SceneExited);

        // 等待当前帧渲染完毕后暂停，确保还原后的场景显示到屏幕上
        yield return new WaitForEndOfFrame();
        ObjectManager.Pause();
    }

    int timeTick = 0;
    public Exception FatalException { get; set; }

    private void FixedUpdate()
    {
        timeTick++;
        // 偶数拍，跳过
        if (timeTick % 2 == 0) return;

        ObjectManager.UpdateFromScene();

        try
        {
            InMatchLoop();
        }
        catch (TimeoutException e)
        {
            // StopMatch(false);
            FatalException = e;
        }
    }

    /// <summary>
    /// 维护比赛状态，根据裁判的判定结果执行不同的动作。<br/>
    /// 完整的一拍有三个步骤：
    /// 时间递增、更新输入（轮速或摆位）---> 物理引擎运行 ---> 触发事件。<br/>
    /// 这个函数会在每次“物理引擎运行”结束被调用，因此，一是需要处理上一拍的运行结果，二是作下一拍（本拍）的准备。<br/>
    /// </summary>
    public void InMatchLoop()
    {
        if (ManualPlacing) throw new InvalidOperationException("manual placing");
        if (!LoadSucceed || !Started || Paused) return;

        // 触发事件，作为上一拍的结束
        // 如果现在是阶段的第一拍，上一拍无意义，不用触发事件
        if (GlobalMatchInfo.TickPhase != 0)
            Event.Send(Event.EventType1.MatchInfoUpdate, GlobalMatchInfo);
        else
        {
            // 在第一拍做初始化
            placementCount = 0;
            callbackCount = 0;
            lastPlacementId = 0;
            placementToIgnore = new Queue<int>();
        }

        /* 之前处理上一拍 */
        /* 之后为本拍做准备 */

        // 从裁判中获取下一拍的动作。
        JudgeResult judgeResult = GlobalMatchInfo.Referee.Judge(GlobalMatchInfo);

        // 时间加一
        GlobalMatchInfo.TickMatch++;
        GlobalMatchInfo.TickPhase++;

        switch (judgeResult.ResultType)
        {
            // 执行动作，更新输入
            case ResultType.GameOver:
                // 整场比赛结束

                // 可能最后一拍正好进球，需要判断
                switch (judgeResult.WhoGoal)
                {
                    case Side.Blue:
                        GlobalMatchInfo.Score.BlueScore++;
                        break;
                    case Side.Yellow:
                        GlobalMatchInfo.Score.YellowScore++;
                        break;
                }

                StopMatch();
                break;

            case ResultType.NextPhase:
                // 阶段结束
                // 这之后的物理引擎运行的一拍无意义，不用触发MatchInfoUpdate事件
                if (GlobalMatchInfo.MatchPhase == MatchPhase.FirstHalf)
                {
                    SwitchRole();
                }

                GlobalMatchInfo.MatchPhase = GlobalMatchInfo.MatchPhase.NextPhase();
                GlobalMatchInfo.TickPhase = 0; // 时间指定为0，使得下一拍的裁判得知新阶段的开始
                OnNextPhase();
                // 通知策略
                switch (GlobalMatchInfo.MatchPhase)
                {
                    case MatchPhase.FirstHalf:
                        break;
                    case MatchPhase.SecondHalf:
                        StrategyManager.Blue.OnSecondHalfStart();
                        StrategyManager.Yellow.OnSecondHalfStart();
                        break;
                    case MatchPhase.OverTime:
                        StrategyManager.Blue.OnOvertimeStart();
                        StrategyManager.Yellow.OnOvertimeStart();
                        break;
                    case MatchPhase.Penalty:
                        StrategyManager.Blue.OnPenaltyShootoutHalfStart();
                        StrategyManager.Yellow.OnPenaltyShootoutHalfStart();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;

            case ResultType.NormalMatch:
                // 正常比赛，输入轮速
                UpdateWheelsToScene();
                break;

            default:
                // 判断是否进球
                switch (judgeResult.WhoGoal)
                {
                    case Side.Blue:
                        GlobalMatchInfo.Score.BlueScore++;
                        break;
                    case Side.Yellow:
                        GlobalMatchInfo.Score.YellowScore++;
                        break;
                }


                // 手动摆位
                if (manualPlaceEnabled)
                {
                    BoardcastJudgeResult(judgeResult);
                    BeginManualPlace();
                }
                else
                {
                    void Callback()
                    {
                        #region Patch to placement

                        int callbackId = callbackCount;
                        callbackCount++;
                        if (callbackId == lastPlacementId)
                        {
                            lastPlacementId = 0;
                        }
                        else
                        {
                            Debug.Assert(placementToIgnore.Peek() == callbackId);
                            placementToIgnore.Dequeue();
                            return;
                        }

                        #endregion

                        BoardcastJudgeResult(judgeResult);
                        //Debug.Log($"callback placement {callbackId}");
                        UpdatePlacementToScene(judgeResult);
                        ObjectManager.SetStill();
                        Event.Send(Event.EventType1.AutoPlacement, GlobalMatchInfo);
                    }

                    #region Patch to placement

                    int placementId = placementCount;
                    placementCount++;
                    if (lastPlacementId == 0)
                    {
                        lastPlacementId = placementId;
                    }
                    else
                    {
                        placementToIgnore.Enqueue(placementId);
                    }

                    #endregion

                    if (GlobalMatchInfo.TickMatch > 1)
                    {
                        //Debug.Log($"Will stop at {GlobalMatchInfo.TickMatch} {placementId}");
                        PauseForSeconds(2, Callback);
                    }
                    else
                    {
                        Callback();
                    }
                }

                break;
        }
    }

    private void BoardcastJudgeResult(JudgeResult judgeResult)
    {
        StrategyManager.Blue.OnJudgeResult(judgeResult);
        StrategyManager.Yellow.OnJudgeResult(new JudgeResult
        {
            Actor = judgeResult.Actor.ToAnother(),
            ResultType = judgeResult.ResultType,
            Reason = judgeResult.Reason
        });
    }

    /// <summary>
    /// 是否启用手动摆位的功能
    /// </summary>
    public bool manualPlaceEnabled = false;

    bool _manualPlacing;

    /// <summary>
    /// 是否正在手动摆位。
    /// 手动摆位已启用且正在处于手动摆位状态时为真；仅当手动摆位已经启动时可以设置。
    /// </summary>
    /// <value></value>
    public bool ManualPlacing
    {
        get => manualPlaceEnabled && _manualPlacing;
        private set
        {
            if (!manualPlaceEnabled) throw new InvalidOperationException("manual place disabled");
            _manualPlacing = value;
        }
    }

    /// <summary>
    /// 开始手动摆位。<br/>
    /// 会暂停运行，直到EndManualPlace被调用。<br/>
    /// </summary>
    public void BeginManualPlace()
    {
        if (!manualPlaceEnabled) throw new InvalidOperationException("manual place disabled");
        PauseMatchClearly();
        ManualPlacing = true;

        ObjectManager.SetToDefault();
        ObjectManager.SetStill();
    }

    /// <summary>
    /// 结束手动摆位。<br/>
    /// 裁判合法化摆位，继续比赛<br/>
    /// </summary>
    public void EndManualPlace()
    {
        if (!manualPlaceEnabled) throw new InvalidOperationException("manual place disabled");

        // 此时手动摆位已经完成，从场地中拉取信息
        ObjectManager.UpdateFromScene();
        //        GlobalMatchInfo.Referee.JudgeAutoPlacement(GlobalMatchInfo, GlobalMatchInfo.Referee.savedJudge);
        ObjectManager.SetBluePlacement(GlobalMatchInfo.BlueRobots);
        ObjectManager.SetYellowPlacement(GlobalMatchInfo.YellowRobots);
        ObjectManager.SetBallPlacement(GlobalMatchInfo.Ball);
        ObjectManager.SetStill();

        Event.Send(Event.EventType1.AutoPlacement, GlobalMatchInfo);
        ManualPlacing = false;
        ResumeMatchClearly();
    }

    /// <summary>
    /// 新比赛开始<br/>
    /// 比分、阶段信息清空，时间置为零，裁判信息清空；还原默认场地；通知策略
    /// </summary>
    public void StartMatch()
    {
        Started = false;

        GlobalMatchInfo.Score = new MatchScore();
        GlobalMatchInfo.TickMatch = 0;
        GlobalMatchInfo.TickPhase = 0;
        GlobalMatchInfo.MatchPhase = MatchPhase.FirstHalf;
        GlobalMatchInfo.Referee = new Referee();

        ObjectManager.SetToDefault();
        ObjectManager.SetStill();

        StrategyManager.Blue.OnMatchStart();
        StrategyManager.Yellow.OnMatchStart();
        StrategyManager.Blue.OnFirstHalfStart();
        StrategyManager.Yellow.OnFirstHalfStart();

        Started = true;
        Paused = true;
        Event.Send(Event.EventType1.MatchStart, GlobalMatchInfo);
        OnMatchStart();
    }

    /// <summary>
    /// 停止比赛<br/>
    /// 会根据<parmref name="willNotifyStrategies">参数决定是否通知策略；
    /// 暂时保留赛场信息，等到下次StartMatch会重置赛场。
    /// </summary>
    /// <param name="willNotifyStrategies">是否向策略发送通知，如果是由于策略出现错误需要停止比赛，可以指定为false。默认为true</param>
    public void StopMatch(bool willNotifyStrategies = true)
    {
        Started = false;
        Paused = true;

        ObjectManager.Pause();

        if (willNotifyStrategies)
        {
            StrategyManager.Blue.OnMatchStop();
            StrategyManager.Yellow.OnMatchStop();
        }

        Event.Send(Event.EventType1.MatchStop, GlobalMatchInfo);
    }

    /// <summary>
    /// 是否正在被外部暂停。
    /// </summary>
    bool externalPausing;

    /// <summary>
    /// 是否正在定时暂停。
    /// </summary>
    public bool TimedPausing { get; private set; }

    /// <summary>
    /// 如果比赛开始则暂停比赛。
    /// </summary>
    /// <remarks>
    /// 暂停和继续的讨论：
    /// 暂停和继续由Paused变量控制，有两种方式改变该变量：
    /// 一种是配套使用的PauseMatch和ResumeMatch，另一种是PauseForSeconds的开始和结束。<br/>
    /// 其中，前一种方式提供给外部使用，调用时机不确定，不应对其作任何预测；后一种方式PlayMain自行使用。<br/>
    /// 要明确的几点：<br/>
    /// <list type="number">
    /// <item>
    /// PauseMatch会被用户使用，所以应该具有最高的权限，在任何情况下都应该成功暂停。
    ///     即如果在已经启动的PauseForSeconds即将结束而准备继续比赛前，用户点了暂停（调用了PauseMatch），
    ///     那么最终不应该继续比赛；
    /// </item>
    /// <item>
    /// 为了保证PauseForSeconds暂停的效果，在等待时间内不应允许用户调用ResumeMatch（TODO: 后续可以考虑保存用户的继续请求）。
    /// </item>
    /// </list>
    /// </remarks>
    public void PauseMatch()
    {
        externalPausing = true;
        PauseMatchClearly();
    }

    /// <summary>
    /// 继续比赛。<br/>
    /// 如果正在定时暂停，则禁止。
    /// </summary>
    public void ResumeMatch()
    {
        if (ManualPlacing) throw new InvalidOperationException("manual placing");

        if (!TimedPausing)
        {
            ResumeMatchClearly();
            externalPausing = false;
        }
    }

    /// <summary>
    /// 暂停 <paramref name="sec" /> 秒，然后执行 <paramref name="callback" />。
    /// </summary>
    private void PauseForSeconds(int sec, Action callback)
    {
        IEnumerator _PauseCoroutine()
        {
            TimedPausing = true;
            yield return new WaitForSecondsRealtime(sec);
            try
            {
                callback();
            }
            finally
            {
                TimedPausing = false;
            }

            yield return new WaitForSecondsRealtime(sec);

            // 外部没有主动暂停，则可以继续比赛
            if (!externalPausing) ResumeMatchClearly();
        }

        if (sec > 0)
        {
            PauseMatchClearly();
            StartCoroutine(_PauseCoroutine());
        }
    }

    /// <summary>
    /// 实际执行暂停操作，不维护暂停相关的状态。供内部使用
    /// </summary>
    private void PauseMatchClearly()
    {
        if (Started)
        {
            Paused = true;
            ObjectManager.Pause();
        }
    }

    /// <summary>
    /// 实际执行继续操作，不维护暂停相关的状态。供内部使用<br/>
    /// </summary>
    private void ResumeMatchClearly()
    {
        if (Started) // 比赛已经开始
        {
            Paused = false;
            ObjectManager.Resume();
        }
    }

    /// <summary>
    /// 从策略中获取轮速并输入到场地中。
    /// <remark>满足右攻假设。</remark>
    /// </summary>
    void UpdateWheelsToScene()
    {
        WheelInfo wheelsBlue = StrategyManager.Blue.GetInstruction(
            GlobalMatchInfo.GetSide(Side.Blue));

        // GetSide(Yellow)返回的数据是将黄方当作蓝方的数据，已经完成转换
        WheelInfo wheelsYellow = StrategyManager.Yellow.GetInstruction(
            GlobalMatchInfo.GetSide(Side.Yellow));

        wheelsBlue.Normalize(); //轮速规整化
        wheelsYellow.Normalize(); //轮速规整化

        ObjectManager.SetBlueWheels(wheelsBlue);
        ObjectManager.SetYellowWheels(wheelsYellow);
    }

    /// <summary>
    /// 从策略中获取摆位信息并做检查和修正，之后输入到场地中。
    /// <remark>满足右攻假设。</remark>
    /// </summary>
    /// <param name="judgeResult">摆位的原因信息</param>
    void UpdatePlacementToScene(JudgeResult judgeResult)
    {
        var currentMatchInfo = (MatchInfo)GlobalMatchInfo.Clone();
        PlacementInfo blueInfo;
        PlacementInfo yellowInfo;

        switch (judgeResult.WhoisFirst)
        {
            case Side.Blue:
                {
                    // 蓝方先摆位
                    blueInfo = StrategyManager.Blue.GetPlacement(currentMatchInfo.GetSide(Side.Blue));
                    // 将蓝方返回的数据同步到currMi
                    currentMatchInfo.UpdateFrom(blueInfo.Robots, Side.Blue);
                    // 将蓝方的球的坐标同步到currMi
                    currentMatchInfo.UpdateFrom(blueInfo.Ball);
                    //先进行第一次摆位判罚
                    GlobalMatchInfo.Referee.JudgeAutoPlacement(currentMatchInfo, judgeResult, Side.Blue);
                    //判罚后将蓝方信息更新
                    blueInfo.PlacementInfoFromMatchInfo(currentMatchInfo, Side.Blue);
                    // 黄方后摆位
                    yellowInfo = StrategyManager.Yellow.GetPlacement(currentMatchInfo.GetSide(Side.Yellow));
                    // 黄方策略认为自己是蓝方，所以返回数据之后需要转换
                    yellowInfo.ConvertToAnotherSide();

                    // 从两方数据拼接MatchInfo，球的数据取决于judgeResult
                    currentMatchInfo = new MatchInfo(blueInfo, yellowInfo, judgeResult.WhosBall);

                    //进行第二次裁判判罚
                    GlobalMatchInfo.Referee.JudgeAutoPlacement(currentMatchInfo, judgeResult, Side.Yellow);
                    break;
                }
            case Side.Yellow:
                {
                    // 黄方先摆位
                    yellowInfo = StrategyManager.Yellow.GetPlacement(currentMatchInfo.GetSide(Side.Yellow));
                    // 由于右攻假设，黄方认为自己是蓝方，返回的数据是其作为蓝方的数据，需要转换
                    yellowInfo.ConvertToAnotherSide();
                    // 将黄方返回的数据同步到currMi
                    currentMatchInfo.UpdateFrom(yellowInfo.Robots, Side.Yellow);
                    //再将球进行更新
                    currentMatchInfo.UpdateFrom(yellowInfo.Ball);
                    //进行第一次摆位判罚
                    GlobalMatchInfo.Referee.JudgeAutoPlacement(currentMatchInfo, judgeResult, Side.Yellow);
                    //判罚之后将黄方信息更新
                    yellowInfo.PlacementInfoFromMatchInfo(currentMatchInfo,Side.Yellow);
                    // 蓝方后摆位
                    blueInfo = StrategyManager.Blue.GetPlacement(currentMatchInfo.GetSide(Side.Blue));

                    // 从两方数据拼接MatchInfo，球的数据取决于judgeResult
                    currentMatchInfo = new MatchInfo(blueInfo, yellowInfo, judgeResult.WhosBall);
                    //进行第二次裁判判罚
                    GlobalMatchInfo.Referee.JudgeAutoPlacement(currentMatchInfo, judgeResult, Side.Blue);
                }

                break;
            default:
                throw new ArgumentException("Side cannot be Nobody");
        }


        // 设置场地
        ObjectManager.SetBluePlacement(currentMatchInfo.BlueRobots);
        ObjectManager.SetYellowPlacement(currentMatchInfo.YellowRobots);
        ObjectManager.SetBallPlacement(currentMatchInfo.Ball);
    }

    /// <summary>
    /// 切换双方角色
    /// 将策略双方调换，GlobalMatchInfo中的比分调换
    /// </summary>
    public void SwitchRole()
    {
        StrategyManager.SwitchRole();
        GlobalMatchInfo.Score.Swap();
    }

    /// <summary>
    /// 从指定的endpoint字符串中加载双方策略
    /// </summary>
    /// <param name="blue_ep"></param>
    /// <param name="yellow_ep"></param>
    /// <returns></returns>
    /// <exception cref="StrategyException">
    /// 加载失败则抛出该错误
    /// </exception>
    public bool LoadStrategy(string blue_ep, string yellow_ep)
    {
        var factory = new StrategyFactory
        {
            BlueEP = blue_ep,
            YellowEP = yellow_ep
        };
        StrategyManager.StrategyFactory = factory;

        if (!StrategyManager.ConnectBlue())
            throw new StrategyException(Side.Blue, "Connect Failed");
        if (!StrategyManager.ConnectYellow())
            throw new StrategyException(Side.Yellow, "Connect Failed");

        return true;
    }

    /// <summary>
    /// 移除一方的策略
    /// </summary>
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

    /// <summary>
    /// 移除两方的策略
    /// </summary>
    public void RemoveStrategy()
    {
        RemoveStrategy(Side.Blue);
        RemoveStrategy(Side.Yellow);
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
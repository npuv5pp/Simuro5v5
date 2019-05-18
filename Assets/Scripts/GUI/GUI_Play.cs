/// <summary>
/// Play场景的UI。
/// 主要通过PlayMain脚本操作比赛，同时将其中的信息显示在UI上
/// GUI_Play --> PlayMain --> StrategyManager
///                      \--> ObjectManager
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Simuro5v5;
using Simuro5v5.Config;
using TMPro;
using Event = Simuro5v5.EventSystem.Event;
using System;
using Simuro5v5.Strategy;

public class GUI_Play : MonoBehaviour
{
    bool MenuOpen => MenuStack.Count > 0;

    static DataRecorder recorder;

    PlayMain playMain;
    MatchInfo MatchInfo => playMain.GlobalMatchInfo;

    // background object
    public GameObject menuBackground;

    // sub-menu items
    public GameObject menuMain; // main menu object
    public GameObject menuStrategy; // strategy menu object

    // main menu items
    public Button newMatchButton;
    public Button resumeButton;
    public Button replayButton;
    public Button loadButton;
    public Button unloadButton;

    // strategy menu items
    public TMP_InputField blueInputField;
    public TMP_InputField yellowInputField;

    // animation control items
    public AnimControl refereeAnim;
    public AnimControl cameraAnim;
    public AnimControl topAnim;

    // other ui items
    public TMP_Text blueScoreText;
    public TMP_Text yellowScoreText;
    public TMP_Text timeText;
    public TMP_Text refereeLogText;
    public TMP_Text statusText;

    public Text blueTeamName;
    public Text yellowTeamName;

    Stack<GameObject> MenuStack { get; set; }

    void Start()
    {
        playMain = PlayMain.Singleton.GetComponent<PlayMain>();

        UpdateAnim();
        UpdateTimeText();
        UpdateScoreText();

        menuBackground.SetActive(false);
        menuMain.SetActive(false);
        menuStrategy.SetActive(false);

        MenuStack = new Stack<GameObject>();
        PushMenu(menuMain);

        if (recorder == null)
        {
            recorder = new DataRecorder();
        }

        blueInputField.text = $"127.0.0.1:{StrategyConfig.BlueStrategyPort}";
        yellowInputField.text = $"127.0.0.1:{StrategyConfig.YellowStrategyPort}";
    }

    public void LoadMainScene()
    {
        Event.Send(Event.EventType0.PlaySceneExited);
        SceneManager.LoadScene("MainScene");
    }

    public void LoadReplayScene()
    {
        GUI_Replay.Recorder = recorder;
        Event.Send(Event.EventType0.PlaySceneExited);
        SceneManager.LoadScene("GameScene_Replay");
    }

    /// <summary>
    /// 停止比赛并尝试移除策略
    /// </summary>
    /// <param name="willNotifyStrategies">是否向策略发送通知，如果是由于策略出现错误需要停止比赛，可以指定为false。默认为true</param>
    public void StopMatchAndRemoveStrategy(bool willNotifyStrategies=true)
    {
        AnimOutGame();
        recorder.Stop();
        recorder.Clear();

        try
        {
            playMain.StopMatch(willNotifyStrategies);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Win32Dialog.ShowMessageBox("通讯失败，强制卸载", "Remove Failed");
        }
        finally
        {
            playMain.RemoveStrategy();
        }
    }

    /// <summary>
    /// 尝试加载策略
    /// </summary>
    public void TryLoadStrategy()
    {
        string blue_ep, yellow_ep;
        if (blueInputField.text.Trim() == "")
            blue_ep = "127.0.0.1";
        else
            blue_ep = blueInputField.text;

        if (yellowInputField.text.Trim() == "")
            yellow_ep = "127.0.0.1";
        else
            yellow_ep = yellowInputField.text;

        try
        {
            playMain.LoadStrategy(blue_ep, yellow_ep);
            AnimInGame();
        }
        catch (StrategyException e)
        {
            Debug.LogError(e);
            Win32Dialog.ShowMessageBox($"{(e.side == Side.Blue ? "蓝方" : "黄方")}策略连接失败", "Failed");
            AnimOutGame();
        }
    }

    public void OpenMenuStrategy()
    {
        PushMenu(menuStrategy);
    }

    public void CloseMenuAndResume()
    {
        PopMenu();
        playMain.ResumeMatch();
    }

    public void PlayMainStartMatch()
    {
        if (recorder.IsRecording)
        {
            recorder.Stop();
            recorder.Clear();
        }
        recorder.Start();
        playMain.StartMatch();
    }

    int timeTick = 0;

    void FixedUpdate()
    {
        timeTick++;
        // 偶数拍，跳过
        if (timeTick % 2 == 0) return;

        playMain.ObjectManager.UpdateFromScene();

        try
        {
            playMain.InMatchLoop();
        }
        catch (TimeoutException e)
        {
            Debug.LogError(e);
            Win32Dialog.ShowMessageBox("策略连接异常", "Strategy Connection");
            // 退出比赛
            StopMatchAndRemoveStrategy(false);
            throw;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyUp(KeyCode.Escape))
        {
            // right clicked, pause and toggle menu
            if (MenuOpen)
            {
                PopMenu();
                if (MenuStack.Count > 0)
                {
                    newMatchButton.Select();
                }
            }
            else
            {
                playMain.PauseMatch();
                PushMenu(menuMain);
                resumeButton.Select();
            }
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // left clicked, pause
            if (!MenuOpen)
            {
                if (playMain.Started)
                {
                    if (playMain.Paused)
                    {
                        playMain.ResumeMatch();
                    }
                    else
                    {
                        playMain.PauseMatch();
                    }
                }
            }
        }

        UpdateRefereeText();
        UpdateTimeText();
        UpdateScoreText();
        UpdateButtons();
        UpdateStatusText();
        UpdateTeamname();
        UpdateAnim();
    }

    void UpdateAnim()
    {
        if (playMain.LoadSucceed)
        {
            AnimInGame();
        }
        else
        {
            AnimOutGame();
        }
    }

    void UpdateTeamname()
    {
        if (!playMain.LoadSucceed)
        {
            SetBlueTeamname("Blue team");
            SetYellowTeamname("Yellow team");
        }
        else
        {
            SetBlueTeamname(playMain.StrategyManager.Blue.GetTeamInfo().Name);
            SetYellowTeamname(playMain.StrategyManager.Yellow.GetTeamInfo().Name);
        }
    }

    void UpdateStatusText()
    {
        if (!playMain.LoadSucceed)
        {
            SetStatusInfo("Waiting for strategies");
        }
        else if (!playMain.Started)
        {
            SetStatusInfo("Waiting for new match");
        }
        else if (playMain.Paused)
        {
            SetStatusInfo("Paused round");
        }
        else
        {
            switch (playMain.GlobalMatchInfo.MatchPhase)
            {
                case MatchPhase.FirstHalf:
                    {
                        SetStatusInfo("First Half In Playing");
                        break;
                    }
                case MatchPhase.SecondHalf:
                    {
                        SetStatusInfo("Second Half In Playing");
                        break;
                    }
                case MatchPhase.OverTime:
                    {
                        SetStatusInfo("Over Time In Playing");
                        break;
                    }
                case MatchPhase.Penalty:
                    {
                        SetStatusInfo("Penalty Shootout In Playing");
                        break;
                    }
            }
        }
    }

    void UpdateButtons()
    {
        // update buttons' status
        newMatchButton.interactable = playMain.LoadSucceed;
        resumeButton.interactable = playMain.Started && playMain.Paused;
        unloadButton.interactable = playMain.LoadSucceed;
    }

    void UpdateTimeText()
    {
        SetTimeText(MatchInfo.TickMatch);
    }

    void UpdateScoreText()
    {
        SetBlueScoreText(MatchInfo.Score.BlueScore);
        SetYellowScoreText(MatchInfo.Score.YellowScore);
    }

    void UpdateRefereeText()
    {
        if (MatchInfo.Referee.savedJudge.ResultType != ResultType.NormalMatch)
            SetRefereeInfo(MatchInfo.Referee.savedJudge.ToRichText());
    }

    void PushMenu(GameObject newMenu)
    {
        if (MenuOpen)
        {
            HideMenu();
        }

        MenuStack.Push(newMenu);
        ShowMenu();
    }

    void PopMenu()
    {
        HideMenu();
        if (MenuOpen)
        {
            MenuStack.Pop();
        }
        // 如果栈上还有菜单，就打开它
        if (MenuOpen)
        {
            ShowMenu();
        }
    }

    /// <summary>
    /// 打开背景与 <code>MenuStack</code> 最后一项，并设置 <code>menu_open</code> 为真。
    /// </summary>
    void ShowMenu()
    {
        if (MenuOpen)
        {
            menuBackground.SetActive(true);
            MenuStack.Peek().SetActive(true);
        }
    }

    /// <summary>
    /// 关闭背景与 <code>MenuStack</code> 中最后一项。
    /// </summary>
    void HideMenu()
    {
        if (MenuOpen)
        {
            MenuStack.Peek().SetActive(false);
        }
        menuBackground.SetActive(false);
    }

    void SetBlueScoreText(int i)
    {
        blueScoreText.text = i.ToString();
    }

    void SetYellowScoreText(int i)
    {
        yellowScoreText.text = i.ToString();
    }

    void SetTimeText(int i)
    {
        timeText.text = i.ToString();
    }

    void SetRefereeInfo(string info)
    {
        refereeLogText.text = info;
    }

    void SetRefereeInfo(object obj)
    {
        var info = (string)obj;
        SetRefereeInfo(info);
    }

    void SetBlueTeamname(string info)
    {
        blueTeamName.text = info;
    }

    void SetYellowTeamname(string info)
    {
        yellowTeamName.text = info;
    }

    void SetStatusInfo(string info)
    {
        statusText.text = info;
    }

    void AnimInGame()
    {
        topAnim.InGame();
        refereeAnim.InGame();
        cameraAnim.InGame();
    }

    void AnimOutGame()
    {
        topAnim.OutGame();
        refereeAnim.OutGame();
        cameraAnim.OutGame();
    }
}

/************************************************************************
 * GUI_Replay
 * 1.比赛界面，包括esc进入菜单
 * 2.比赛菜单，包括回到比赛、进入回放、退出
************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Simuro5v5;
using Simuro5v5.Config;
using TMPro;
using Event = Simuro5v5.EventSystem.Event;
using System;

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

    public TMP_Text blueTeamName;
    public TMP_Text yellowTeamName;

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

        Event.Register(Event.EventType1.RefereeLogUpdate, SetRefereeInfo);
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

    public void AnimOutGameAndRemoveStrategy()
    {
        AnimOutGame();
        recorder.Stop();
        recorder.Clear();

        try
        {
            playMain.StopMatch();
            playMain.RemoveStrategy();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Win32Dialog.ShowMessageBox("卸载超时，强制卸载", "Remove Failed");
        }
    }

    public void AnimInGameAndLoadStrategy()
    {
        AnimInGame();
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
            playMain.LoadStrategy(Side.Blue, blue_ep);
        }
        catch (TimeoutException e)
        {
            Debug.LogError(e.Message);
            Win32Dialog.ShowMessageBox("蓝方策略连接超时", "Timeout");
            AnimOutGame();
            return;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Win32Dialog.ShowMessageBox("蓝方策略连接失败", "Failed");
            AnimOutGame();
            return;
        }

        try
        {
            playMain.LoadStrategy(Side.Yellow, yellow_ep);
        }
        catch (TimeoutException e)
        {
            Debug.LogError(e.Message);
            Win32Dialog.ShowMessageBox("黄方策略连接超时", "Timeout");
            AnimOutGame();
            return;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Win32Dialog.ShowMessageBox("黄方策略连接失败", "Failed");
            AnimOutGame();
            return;
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

        UpdateTimeText();
        UpdateScoreText();
        UpdateButtons();
        UpdateStatusText();
        UpdateTeamname();
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
            SetBlueTeamname(playMain.StrategyManager.BlueTeamInfo.Name);
            SetYellowTeamname(playMain.StrategyManager.YellowTeamInfo.Name);
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
            switch (playMain.GlobalMatchInfo.MatchState)
            {
                case MatchState.FirstHalf:
                    {
                        SetStatusInfo("First Half In Playing");
                        break;
                    }
                case MatchState.SecondHalf:
                    {
                        SetStatusInfo("Second Half In Playing");
                        break;
                    }
                case MatchState.OverTime:
                    {
                        SetStatusInfo("Over Time In Playing");
                        break;
                    }
                case MatchState.Penalty:
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
        var info = (string) obj;
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

    private void OnDestroy()
    {
        Event.UnRegister(Event.EventType1.RefereeLogUpdate, SetRefereeInfo);
    }
}

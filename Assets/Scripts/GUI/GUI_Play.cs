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
using TMPro;
using Event = Simuro5v5.EventSystem.Event;
using static Simuro5v5.Strategy.NetStrategy;

public class GUI_Play : MonoBehaviour
{
    bool menuOpen = false;

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
    public Button newRoundButton;
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
        OpenMenu();

        if (recorder == null)
        {
            recorder = new DataRecorder();
        }

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
        playMain.RemoveStrategy();
        playMain.StopMatch();
    }

    public void AnimInGameAndLoadStrategy()
    {
        AnimInGame();
        try
        {
            playMain.LoadStrategy(blueInputField.text.Trim(), yellowInputField.text.Trim());
        }
        catch (LoadDllFailed e)
        {
            Debug.LogError(e.Message);
            AnimOutGame();
        }
    }

    public void OpenMenuStrategy()
    {
        MenuStack.Peek().SetActive(false);
        PushMenu(menuStrategy);
        MenuStack.Peek().SetActive(true);
    }

    public void CloseMenuAndResume()
    {
        CloseMenu();
        playMain.ResumeRound();
    }

    public void PlayMainStartMatch()
    {
        recorder.Begin();
        playMain.StartMatch();
    }

    public void PlayMainStartRound()
    {
        playMain.StartRound();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyUp(KeyCode.Escape))
        {
            // right clicked, pause and toggle menu
            if (menuOpen)
            {
                PopMenu();
                if (MenuStack.Count > 0)
                {
                    newMatchButton.Select();
                }
            }
            else
            {
                if (!playMain.InPlacement)
                {
                    playMain.PauseRound();
                    PushMenu(menuMain);
                    OpenMenu();
                    resumeButton.Select();
                }
            }
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // left clicked, pause
            if (!menuOpen)
            {
                if (playMain.StartedMatch && playMain.InRound)
                {
                    if (playMain.PausedRound)
                    {
                        playMain.ResumeRound();
                    }
                    else
                    {
                        playMain.PauseRound();
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
            SetBlueTeamname(playMain.StrategyManager.GetBlueTeaminfo().Name);
            SetYellowTeamname(playMain.StrategyManager.GetYellowTeaminfo().Name);
        }
    }

    void UpdateStatusText()
    {
        if (!playMain.LoadSucceed)
        {
            SetStatusInfo("Waiting for strategies");
        }
        else if (!playMain.StartedMatch)
        {
            SetStatusInfo("Waiting for new game");
        }
        else
        {
            if (playMain.InPlacement)
            {
                SetStatusInfo("Auto placement");
            }
            else if (playMain.InRound)
            {
                // In round
                SetStatusInfo(playMain.PausedRound ? "Paused round" : "In playing");
            }
            else
            {
                SetStatusInfo("Waiting for new round");
            }
        }
    }

    void UpdateButtons()
    {
        // update buttons' status
        newMatchButton.interactable = playMain.LoadSucceed;
        newRoundButton.interactable = playMain.StartedMatch;
        resumeButton.interactable = playMain.InRound && playMain.PausedRound;
        unloadButton.interactable = playMain.LoadSucceed;
    }

    void UpdateTimeText()
    {
        SetTimeText(MatchInfo.PlayTime);
    }

    void UpdateScoreText()
    {
        SetBlueScoreText(MatchInfo.Score.BlueScore);
        SetYellowScoreText(MatchInfo.Score.YellowScore);
    }

    void PushMenu(GameObject newMenu)
    {
        if (menuOpen)
        {
            CloseMenu();
            MenuStack.Push(newMenu);
            OpenMenu();
        }
        else
        {
            MenuStack.Push(newMenu);
        }
    }

    public void PopMenu()
    {
        bool will_open = menuOpen;
        CloseMenu();
        if (MenuStack.Count >= 1)
        {
            MenuStack.Pop();
        }
        if (will_open)
        {
            OpenMenu();
        }
    }

    /// <summary>
    /// 打开背景与 <code>MenuStack</code> 最后一项，并设置 <code>menu_open</code> 为真。
    /// </summary>
    void OpenMenu()
    {
        if (MenuStack.Count > 0)
        {
            OpenBackground();
            MenuStack.Peek().SetActive(true);
            menuOpen = true;
        }
    }

    /// <summary>
    /// 关闭背景与 <code>MenuStack</code> 中最后一项。
    /// </summary>
    void CloseMenu()
    {
        if (MenuStack.Count > 0)
        {
            MenuStack.Peek().SetActive(false);
        }
        CloseBackground();
        menuOpen = false;
    }

    void OpenBackground()
    {
        menuBackground.SetActive(true);
    }

    void CloseBackground()
    {
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

    void AnimToggleGame()
    {
        topAnim.Toggle();
        refereeAnim.Toggle();
        cameraAnim.Toggle();
    }

    private void OnDestroy()
    {
        Event.UnRegister(Event.EventType1.RefereeLogUpdate, SetRefereeInfo);
    }
}

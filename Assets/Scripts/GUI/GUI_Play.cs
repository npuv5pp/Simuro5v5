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
using Event = Simuro5v5.EventSystem.Event;

public class GUI_Play : MonoBehaviour
{
    bool menu_open = false;

    static DataRecorder Recorder;

    // 以下对象设为静态，防止之后注册事件函数后，闭包造成重载场景后的空引用
    static Popup Popup { get; set; }

    static PlayMain matchMain { get; set; }
    static MatchInfo matchInfo { get { return matchMain?.GlobalMatchInfo; } }

    public PlayMain playMain;
    static MatchInfo MatchInfo => PlayMain.GlobalMatchInfo;

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

    // strategy menu items
    public InputField blueInputField;
    public InputField yellowInputField;

    // animation control items
    public AnimControl refereeAnim;
    public AnimControl cameraAnim;
    public AnimControl topAnim;

    // other ui items
    public Text blueScoreText;
    public Text yellowScoreText;
    public Text timeText;
    public Text refereeLogText;
    public Text statusText;

    public Text blueTeamName;
    public Text yellowTeamName;

    static Stack<GameObject> MenuStack { get; set; }

    void Start()
    {
        InitObjects();

        UpdateTimeText();
        UpdateScoreText();

        CloseBackground();
        menuMain.SetActive(false);
        menuStrategy.SetActive(false);

        MenuStack = new Stack<GameObject>();
        PushMenu(menuMain);
        OpenMenu();

        // >>> SET IN EDITOR <<<
        // NewMatch onClick() => GUI_Play.PlayMainStartMatch()
        // NewRound onClick() => GUI_Play.PlayMainStartRound()
        // Resume onClick() => GUI_Play.CloseMenuAndResume()
        // Replay onClick() => GUI_Play.LoadReplayScene()
        // Menu/Main/Strategy onClick() => GUI_Play.OpenMenuStrategy()
        // BeginBtn onClick() => GUI_Play.AnimInGameAndLoadStrategy()
        // UnloadBtn onClick() => GUI_Play.AnimOutGameAndRemoveStrategy()
        // Replay interactable => false;
        // Exit onClick() => GUI_Play.LoadMainScene()

        Event.Register(Event.EventType1.LogUpdate, SetRefereeInfo);
    }

    public void LoadMainScene()
    {
        Event.Send(Event.EventType0.PlaySceneExited);
        SceneManager.LoadScene("MainScene");
    }

    public void LoadReplayScene()
    {
        GUI_Replay.Recorder = Recorder;
        Event.Send(Event.EventType0.PlaySceneExited);
        SceneManager.LoadScene("GameScene_Replay");
    }

    public void AnimOutGameAndRemoveStrategy()
    {
        AnimOutGame();
        try
        {
            playMain.RemoveStrategy();
            playMain.StopMatch();
        }
        catch
        {
            AnimInGame();
        }
    }

    public void AnimInGameAndLoadStrategy()
    {
        AnimInGame();
        try
        {
            playMain.LoadStrategy(blueInputField.text.Trim(), yellowInputField.text.Trim());
        }
        catch
        {
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
        playMain.StartMatch();
    }

    public void PlayMainStartRound()
    {
        playMain.StartRound();
    }

    void InitObjects()
    {

        UpdateAnim();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // right clicked, pause and toggle menu
            if (menu_open)
            {
                //CloseMenu();
                PopMenu();
            }
            else
            {
                if (!playMain.InPlacement)
                {
                    playMain.PauseRound();
                    PushMenu(menuMain);
                    OpenMenu();
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            // left clicked, pause
            if (!menu_open)
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
            SetBlueTeamname("Blueteam");
            SetYellowTeamname("Yellowteam");
        }
        else
        {
            SetBlueTeamname(matchMain.StrategyManager.GetBlueTeaminfo().Name);
            SetYellowTeamname(matchMain.StrategyManager.GetYellowTeaminfo().Name);
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
                SetStatusInfo("Auto placementing");
            }
            else if (playMain.InRound)
            {
                // In round
                if (playMain.PausedRound)
                {
                    SetStatusInfo("Paused round");
                }
                else
                {
                    SetStatusInfo("In playing");
                }
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
    }

    void UpdateTimeText()
    {
        SetTimeText(MatchInfo.PlayTime);
    }

    void UpdateScoreText()
    {
        SetBlueScoreText(MatchInfo.Score.BlueScore);
        SetBlueScoreText(MatchInfo.Score.YellowScore);
    }

    void PushMenu(GameObject new_menu)
    {
        if (menu_open)
        {
            CloseMenu();
            MenuStack.Push(new_menu);
            OpenMenu();
        }
        else
        {
            MenuStack.Push(new_menu);
        }
    }

    void PopMenu()
    {
        bool will_open = menu_open;
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
            menu_open = true;
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
        menu_open = false;
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
        var info = obj as string;
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
}

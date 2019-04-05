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
using Simuro5v5.Strategy;

public class GUI_Play : MonoBehaviour
{
    bool menu_open = false;
    bool GUI_on_Camera = true;

    // 以下对象设为静态，防止之后注册事件函数后，闭包造成重载场景后的空引用
    // public Popup popup;

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
    static AnimControl AnimReferee { get; set; }
    static AnimControl AnimCamera { get; set; }
    static AnimControl AnimTop { get; set; }

    // other ui items
    static Text BlueScoreText { get; set; }
    static Text YellowScoreText { get; set; }
    static Text TimeText { get; set; }
    static Text RefereeLogText { get; set; }
    static Text StatusText { get; set; }

    static Text BlueTeamname { get; set; }
    static Text YellowTeamname { get; set; }

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
        // NewMatch onClick() => PlayMain.StartMatch()
        // NewRound onClick() => PlayMain.StartRound()
        // Resume onClick() => GUI_Play.CloseMenuAndResume()
        // Replay onClick() => GUI_Play.LoadReplayScene()
        // Menu/Main/Strategy onClick() => GUI_Play.OpenMenuStrategy()
        // BeginBtn onClick() => GUI_Play.AnimInGameAndLoadStrategy()
        // UnloadBtn onClick() => GUI_Play.AnimOutGameAndRemoveStrategy()
        // Replay interactable => false;
        // Exit onClick() => GUI_Play.LoadMainScene()

        Event.Register(Event.EventType0.RoundStart, delegate ()
        {
            SetRefereeInfo("Waiting For <b>Referee</b>");
        });
        //Event.Register(Event.EventType0.RoundResume, delegate ()
        //{
        //    Popup.Show("Round", "Round resume", 1500);
        //});
        //Event.Register(Event.EventType0.RoundPause, delegate ()
        //{
        //    Popup.Show("Round", "Round pause", 1500);
        //});
        //Event.Register(Event.EventType0.RoundStop, delegate ()
        //{
        //    Popup.Show("Round", "Round stop", 1500);
        //});
        Event.Register(Event.EventType1.LogUpdate, SetRefereeInfo);
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
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

    public void LoadReplayScene()
    {
        // 总拍数作为回放结束拍数
        PlayerPrefs.SetInt("step_end", MatchInfo.PlayTime);
        SceneManager.LoadScene("GameScene_Replay");
    }

    public void CloseMenuAndResume()
    {
        CloseMenu();
        playMain.ResumeRound();
    }

    void InitObjects()
    {
        // playMain = GameObject.Find("/Entity").GetComponent<PlayMain>();
        // popup = GameObject.Find("/Canvas/Popup").GetComponent<Popup>();

        // menuBack = GameObject.Find("/Canvas/Menu/Background");

        // menuMain = GameObject.Find("/Canvas/Menu/Main");
        // newMatchButton = GameObject.Find("/Canvas/Menu/Main/NewMatch").GetComponent<Button>();

        // menuStrategy = GameObject.Find("/Canvas/Menu/Strategy");
        // blueInputField = GameObject.Find("/Canvas/Menu/Strategy/BlueInput").GetComponent<InputField>();
        // yellowInputField = GameObject.Find("/Canvas/Menu/Strategy/YellowInput").GetComponent<InputField>();

        BlueScoreText  = GameObject.Find("/Canvas/Top/Score/Blue").GetComponent<Text>();
        YellowScoreText = GameObject.Find("/Canvas/Top/Score/Yellow").GetComponent<Text>();
        TimeText = GameObject.Find("/Canvas/Top/Time").GetComponent<Text>();
        RefereeLogText = GameObject.Find("/Canvas/Log/Referee").GetComponent<Text>();
        StatusText = GameObject.Find("/Canvas/Top/Status").GetComponent<Text>();

        BlueTeamname = GameObject.Find("/Canvas/Top/Teamname/Bname").GetComponent<Text>();
        YellowTeamname = GameObject.Find("/Canvas/Top/Teamname/Yname").GetComponent<Text>();

        AnimTop = GameObject.Find("/Canvas/Top").GetComponent<AnimControl>();
        AnimReferee = GameObject.Find("/Canvas/Log/Referee").GetComponent<AnimControl>();
        AnimCamera = GameObject.Find("/Cameras/MainCamera").GetComponent<AnimControl>();

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
            SetBlueTeamname(PlayMain.StrategyManager.GetBlueTeaminfo().Name);
            SetYellowTeamname(PlayMain.StrategyManager.GetYellowTeaminfo().Name);
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
        replayButton.interactable = false;
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
        BlueScoreText.text = i.ToString();
    }

    void SetYellowScoreText(int i)
    {
        YellowScoreText.text = i.ToString();
    }

    void SetTimeText(int i)
    {
        TimeText.text = i.ToString();
    }

    void SetRefereeInfo(string info)
    {
        RefereeLogText.text = info;
    }

    void SetRefereeInfo(object obj)
    {
        var info = obj as string;
        SetRefereeInfo(info);
    }

    void SetBlueTeamname(string info)
    {
        BlueTeamname.text = info;
    }

    void SetYellowTeamname(string info)
    {
        YellowTeamname.text = info;
    }

    void SetStatusInfo(string info)
    {
        StatusText.text = info;
    }

    void AnimInGame()
    {
        AnimTop.InGame();
        AnimReferee.InGame();
        AnimCamera.InGame();
    }
    
    void AnimOutGame()
    {
        AnimTop.OutGame();
        AnimReferee.OutGame();
        AnimCamera.OutGame();
    }

    void AnimToggleGame()
    {
        AnimTop.Toggle();
        AnimReferee.Toggle();
        AnimCamera.Toggle();
    }
}

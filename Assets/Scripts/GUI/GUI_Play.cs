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
    static Popup Popup { get; set; }

    static PlayMain matchMain { get; set; }
    static MatchInfo matchInfo { get { return PlayMain.GlobalMatchInfo; } }

    static GameObject SceneObj { get; set; }

    static GameObject CanvasObj { get; set; }
    static GameObject MenuObj { get; set; }

    // background object
    static GameObject MenuBack { get; set; }

    // sub-menu items
    static GameObject MenuObj_Main { get; set; }  // main menu object
    static GameObject MenuObj_Strategy { get; set; } // strategy menu object

    // main menu items
    static GameObject StrategyMenuBtnObj { get; set; }   // button on main menu to strategy menu
    static GameObject NewMatchObj { get; set; }
    static GameObject NewRoundObj { get; set; }
    static GameObject ResumeObj { get; set; }
    static GameObject ReplayObj { get; set; }
    static GameObject ExitObj { get; set; }

    // strategy menu items
    static InputField BlueInputField { get; set; }
    static InputField YellowInputField { get; set; }
    static GameObject BeginObj { get; set; }
    static GameObject UnloadObj { get; set; }

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
        MenuObj_Main.SetActive(false);
        MenuObj_Strategy.SetActive(false);

        MenuStack = new Stack<GameObject>();
        PushMenu(MenuObj_Main);
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
            matchMain.RemoveStrategy();
            matchMain.StopMatch();
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
            matchMain.LoadStrategy(BlueInputField.text.Trim(), YellowInputField.text.Trim());
        }
        catch
        {
            AnimOutGame();
        }
    }

    public void OpenMenuStrategy()
    {
        MenuStack.Peek().SetActive(false);
        PushMenu(MenuObj_Strategy);
        MenuStack.Peek().SetActive(true);
    }

    public void LoadReplayScene()
    {
        // 总拍数作为回放结束拍数
        PlayerPrefs.SetInt("step_end", matchInfo.PlayTime);
        SceneManager.LoadScene("GameScene_Replay");
    }

    public void CloseMenuAndResume()
    {
        CloseMenu();
        matchMain.ResumeRound();
    }

    void InitObjects()
    {
        matchMain = GameObject.Find("/Entity").GetComponent<PlayMain>();
        Popup = GameObject.Find("/Canvas/Popup").GetComponent<Popup>();

        SceneObj = GameObject.Find("MatchScene");

        CanvasObj = GameObject.Find("Canvas");
        MenuObj = GameObject.Find("/Canvas/Menu");
        MenuBack = GameObject.Find("/Canvas/Menu/Background");

        MenuObj_Main = GameObject.Find("/Canvas/Menu/Main");
        StrategyMenuBtnObj = GameObject.Find("/Canvas/Menu/Main/Strategy");
        NewMatchObj = GameObject.Find("/Canvas/Menu/Main/NewMatch");
        NewRoundObj = GameObject.Find("/Canvas/Menu/Main/NewRound");
        ResumeObj = GameObject.Find("/Canvas/Menu/Main/Resume");
        ReplayObj = GameObject.Find("/Canvas/Menu/Main/Replay");
        ExitObj = GameObject.Find("/Canvas/Menu/Main/Exit");

        MenuObj_Strategy = GameObject.Find("/Canvas/Menu/Strategy");
        BlueInputField = GameObject.Find("/Canvas/Menu/Strategy/BlueInput").GetComponent<InputField>();
        YellowInputField = GameObject.Find("/Canvas/Menu/Strategy/YellowInput").GetComponent<InputField>();
        BeginObj = GameObject.Find("/Canvas/Menu/Strategy/BeginBtn");
        UnloadObj = GameObject.Find("/Canvas/Menu/Strategy/UnloadBtn");

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
                if (!matchMain.InPlacement)
                {
                    matchMain.PauseRound();
                    PushMenu(MenuObj_Main);
                    OpenMenu();
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            // left clicked, pause
            if (!menu_open)
            {
                if (matchMain.StartedMatch && matchMain.InRound)
                {
                    if (matchMain.PausedRound)
                    {
                        matchMain.ResumeRound();
                    }
                    else
                    {
                        matchMain.PauseRound();
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
        if (matchMain.LoadSucceed)
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
        if (!matchMain.LoadSucceed)
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
        if (!matchMain.LoadSucceed)
        {
            SetStatusInfo("Waiting for strategies");
        }
        else if (!matchMain.StartedMatch)
        {
            SetStatusInfo("Waiting for new game");
        }
        else
        {
            if (matchMain.InPlacement)
            {
                SetStatusInfo("Auto placementing");
            }
            else if (matchMain.InRound)
            {
                // In round
                if (matchMain.PausedRound)
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
        NewMatchObj.GetComponent<Button>().interactable = matchMain.LoadSucceed;
        NewRoundObj.GetComponent<Button>().interactable = matchMain.StartedMatch;
        ResumeObj.GetComponent<Button>().interactable = matchMain.InRound && matchMain.PausedRound;
        ReplayObj.GetComponent<Button>().interactable = false;
    }

    void UpdateTimeText()
    {
        SetTimeText(matchInfo.PlayTime);
    }

    void UpdateScoreText()
    {
        SetBlueScoreText(matchInfo.Score.BlueScore);
        SetBlueScoreText(matchInfo.Score.YellowScore);
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
        MenuBack.SetActive(true);
    }

    void CloseBackground()
    {
        MenuBack.SetActive(false);
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

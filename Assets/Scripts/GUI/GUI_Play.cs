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

    static MatchMain matchMain { get; set; }
    static MatchInfo matchInfo { get { return MatchMain.GlobalMatchInfo; } }

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

    static List<GameObject> MenuStack { get; set; }

    void Start()
    {
        InitObjects();

        UpdateTimeText();
        UpdateScoreText();

        CloseBackground();
        MenuObj_Main.SetActive(false);
        MenuObj_Strategy.SetActive(false);

        MenuStack = new List<GameObject>();
        PushMenu(MenuObj_Main);
        OpenMenu();

        NewMatchObj.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            matchMain.StartMatch();
        });
        NewRoundObj.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            matchMain.StartRound();
        });
        ResumeObj.GetComponent<Button>().onClick.AddListener(delegate
        {
            CloseMenu();
            matchMain.ResumeRound();
        });
        ReplayObj.GetComponent<Button>().onClick.AddListener(delegate
        {
            // 总拍数作为回放结束拍数
            PlayerPrefs.SetInt("step_end", matchInfo.PlayTime);
            SceneManager.LoadScene("GameScene_Replay");
        });
        StrategyMenuBtnObj.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            MenuStack[MenuStack.Count - 1].SetActive(false);
            PushMenu(MenuObj_Strategy);
            MenuStack[MenuStack.Count - 1].SetActive(true);
        });

        BeginObj.GetComponent<Button>().onClick.AddListener(delegate ()
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
        });
        UnloadObj.GetComponent<Button>().onClick.AddListener(delegate ()
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
        });

        ReplayObj.GetComponent<Button>().interactable = false;
        ExitObj.GetComponent<Button>().onClick.AddListener(delegate
        {
            SceneManager.LoadScene("MainScene");
        });

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

    void InitObjects()
    {
        matchMain = GameObject.Find("/Entity").GetComponent<MatchMain>();
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
    }

    void Update()
    {
        var cameras = GameObject.Find("MainCamera");
        if (cameras.GetComponent<Camera>().enabled == true && GUI_on_Camera == false)
        {
            Open_GUI_Camera();
            GUI_on_Camera = true;
        }
        if (cameras.GetComponent<Camera>().enabled == false && GUI_on_Camera == true)
        {
            Close_GUI_Camera();
            GUI_on_Camera = false;
        }
        if (cameras.GetComponent<Camera>().enabled == true) { }

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

    void UpdateTeamname()
    {
        if (!matchMain.LoadSucceed)
        {
            SetBlueTeamname("Blueteam");
            SetYellowTeamname("Yellowteam");
        }
        else
        {
            SetBlueTeamname(MatchMain.StrategyManager.GetBlueTeaminfo().Name);
            SetYellowTeamname(MatchMain.StrategyManager.GetYellowTeaminfo().Name);
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
            MenuStack.Add(new_menu);
            OpenMenu();
        }
        else
        {
            MenuStack.Add(new_menu);
        }
    }

    void PopMenu()
    {
        bool will_open = menu_open;
        CloseMenu();
        if (MenuStack.Count >= 1)
        {
            var curr = MenuStack[MenuStack.Count - 1];
            MenuStack.Remove(curr);
        }
        if (will_open)
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        if (MenuStack.Count > 0)
        {
            OpenBackground();
            MenuStack[MenuStack.Count - 1].SetActive(true);
            menu_open = true;
        }
    }

    void CloseMenu()
    {
        if (MenuStack.Count > 0)
        {
            MenuStack[MenuStack.Count - 1].SetActive(false);
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

    void Open_GUI_Camera()
    {
        GameObject obj0 = GameObject.Find("Canvas");
        if (obj0 == null)
        {
            Debug.Log("No Canvas Object.");
        }
        else
        {
            Debug.Log("Canvas Object.");
        }

        GameObject obj1 = obj0.transform.Find("ys").gameObject;
        if (obj1 == null)
        {
            Debug.Log("No ys Object.");
        }
        else
        {
            Debug.Log("ys Object.");
        }
        obj1.SetActive(true);

        GameObject obj2 = obj0.transform.Find("bs").gameObject;
        if (obj2 == null)
        {
            Debug.Log("No bs Object.");
        }
        else
        {
            Debug.Log("bs Object.");
        }
        obj2.SetActive(true);

        GameObject obj3 = obj0.transform.Find("referee").gameObject;
        if (obj3 == null)
        {
            Debug.Log("No referee Object.");
        }
        else
        {
            Debug.Log("referee Object.");
        }
        obj3.SetActive(true);

        GameObject obj4 = obj0.transform.Find("time").gameObject;
        if (obj4 == null)
        {
            Debug.Log("No time Object.");
        }
        else
        {
            Debug.Log("time Object.");
        }
        obj4.SetActive(true);

        GameObject obj5 = obj0.transform.Find("esc").gameObject;
        if (obj5 == null)
        {
            Debug.Log("No esc Object.");
        }
        else
        {
            Debug.Log("esc Object.");
        }
        obj5.SetActive(true);
    }

    void Close_GUI_Camera()
    {
        GameObject obj1 = GameObject.Find("ys");
        if (obj1 == null)
        {
            Debug.Log("No ys Object.");
        }
        else
        {
            Debug.Log("ys Object.");
        }
        obj1.SetActive(false);

        GameObject obj2 = GameObject.Find("bs");
        if (obj2 == null)
        {
            Debug.Log("No bs Object.");
        }
        else
        {
            Debug.Log("bs Object.");
        }
        obj2.SetActive(false);

        GameObject obj3 = GameObject.Find("referee");
        if (obj3 == null)
        {
            Debug.Log("No referee Object.");
        }
        else
        {
            Debug.Log("referee Object.");
        }
        obj3.SetActive(false);

        GameObject obj4 = GameObject.Find("time");
        if (obj4 == null)
        {
            Debug.Log("No time Object.");
        }
        else
        {
            Debug.Log("time Object.");
        }
        obj4.SetActive(false);

        GameObject obj5 = GameObject.Find("esc");
        if (obj5 == null)
        {
            Debug.Log("No esc Object.");
        }
        else
        {
            Debug.Log("esc Object.");
        }
        obj5.SetActive(false);
    }
}

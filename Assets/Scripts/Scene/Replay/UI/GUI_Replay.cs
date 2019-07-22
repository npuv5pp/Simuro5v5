/********************************************************************************
 * GUI_Replay
 * 1.回放界面：包括进度条、播放、暂停、快退、快进、调整速度、esc进入菜单、显示栏
 * 2.回放菜单：包括回到比赛、返回播放、退出
********************************************************************************/

using System;
using System.CodeDom;
using System.Globalization;
using System.IO;
using System.Linq;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Simuro5v5;
using UnityEditor;
using UnityEngine.EventSystems;
using static Simuro5v5.Configuration;

public class GUI_Replay : MonoBehaviour
{
    public GameObject Entity;
    public Image PauseButtonImage;
    public Sprite PauseButtonPaused;
    public Sprite PauseButtonNonPaused;

    // controls
    public Slider Slider;
    public DataBoard DataBoard;
    public TMP_Dropdown SpeedDropdown;

    // state text
    public TMP_Text DataTag;
    public TMP_Text tickText;
    public TMP_Text PhaseText;
    public TMP_Text JudgeResultText;
    public TMP_Text BlueScore, YellowScore;

    public static DataRecorder Recorder { get; set; }
    private ObjectManager ObjectManager { get; set; }

    bool Paused;

    private int SliderPosition
    {
        get => (int)Slider.value;
        set => Slider.value = value;
    }

    void Start()
    {
        ObjectManager = new ObjectManager();
        ShowRecord();
        //var eventHistory = Recorder.data
        //    .Where(item => item.matchInfo.Referee.savedJudge.ResultType != ResultType.NormalMatch)
        //    .Select(a => new FormGUI.EventLog()
        //    {
        //        Frame = a.matchInfo.TickMatch,
        //        Info = a.matchInfo.Referee.savedJudge.Reason,
        //    })
        //    .ToList();
        //var form = Launcher.Launcher.Launch(eventHistory);
        //form.Show();
        //
        // Launcher.Launcher.Main();
    }

    private void ShowRecord()
    {
        if (Recorder == null || Recorder.DataLength == 0)
        {
            Debug.Log("Recorder contains no entry or is null.");
            Recorder = DataRecorder.PlaceHolder();
        }

        ObjectManager.RebindObject(Entity);
        ObjectManager.DisablePhysics();
        ObjectManager.Resume();
        Slider.minValue = 0;
        Slider.maxValue = Recorder.DataLength == 0 ? 0 : Recorder.DataLength - 1;
        Slider.value = 0;
        Render(Recorder.IndexOf(0));

        Paused = true;
    }

    void Update()
    {
        DataTag.SetText(Recorder.Name);
        PauseButtonImage.sprite = Paused ? PauseButtonPaused : PauseButtonNonPaused;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPauseClicked();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackToGameClicked();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SpeedDropdown.value -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpeedDropdown.value += 1;
        }
    }

    int tick = 0;

    void FixedUpdate()
    {
        tick++;
        if (tick % 2 == 0)
        {
            return;
        }

        if (!Paused)
        {
            if (SliderPosition == Recorder.DataLength - 1)
            {
                Paused = true;
            }
            else
            {
                Next();
            }
        }

        // 先放到FixedUpdate中，避免回放进行太快
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            OnPreviousClicked();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            OnNextClicked();
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            && Input.GetKey(KeyCode.LeftArrow))
        {
            OnPreviousKeyFrame();
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            && Input.GetKey(KeyCode.RightArrow))
        {
            OnNextKeyFrame();
        }

        if ((Input.GetKey(KeyCode.Home)))
        {
            OnHomeClicked();
        }
         
        if ((Input.GetKey(KeyCode.End)))
        {
            OnEndClicked();
        }
    }

    private void OnEndClicked()
    {
        End();
        Paused = true;
    }

    private void End()
    {
        SliderPosition = Recorder.DataLength - 1;
    }

    private void OnHomeClicked()
    {
        Home();
        Paused = true;
    }

    private void Home()
    {
        SliderPosition = 0;
    }

    /// <summary>
    /// 寻找下一个关键帧 
    /// </summary>
    public void OnNextKeyFrame()
    {
        NextKeyFrame();
        Paused = true;
    }

    private void NextKeyFrame()
    {
        if (SliderPosition == Recorder.DataLength - 1 )
        {
            return;
        }
        for (int tmp = SliderPosition + 1; tmp <= Recorder.DataLength - 1; tmp++)
        {
            if (Recorder.IndexOf(tmp).matchInfo.Referee.savedJudge.ResultType != ResultType.NormalMatch)
            {
                SliderPosition = tmp;
                return;
            }
        }
        SliderPosition = Recorder.DataLength - 1;
    }


    /// <summary>
    /// 寻找上一个关键帧。
    /// </summary>
    public void OnPreviousKeyFrame()
    {
        PreviousKeyFrame();
        Paused = true;
    }

    private void PreviousKeyFrame()
    {
        if (SliderPosition == 0)
        {
            return;
        }
        for (int tmp = SliderPosition - 1; tmp >= 1; tmp--)
        {
            if (Recorder.IndexOf(tmp).matchInfo.Referee.savedJudge.ResultType != ResultType.NormalMatch)
            {
                SliderPosition = tmp;
                return;
            }
        }
        SliderPosition = 0;
    }
   
    /// <summary>
    /// 渲染一拍的数据到场景中，包括：机器人和球的坐标，数据板
    /// </summary>
    void Render(DataRecorder.RecordData data)
    {
        ObjectManager.RevertScene(data.matchInfo);
        tickText.text = $"{data.matchInfo.TickMatch}/{Recorder.DataLength}";
        JudgeResultText.text = data.matchInfo.Referee.savedJudge.ToRichText();
        RenderPhaseText(data.matchInfo.MatchPhase);
        BlueScore.text = data.matchInfo.Score.BlueScore.ToString();
        YellowScore.text = data.matchInfo.Score.YellowScore.ToString();
    }

    void RenderPhaseText(MatchPhase mp)
    {
        switch (mp)
        {
            case MatchPhase.FirstHalf:
                {
                    PhaseText.text = "First Half";
                    break;
                }

            case MatchPhase.SecondHalf:
                {
                    PhaseText.text = "Second Half";
                    break;
                }

            case MatchPhase.OverTime:
                {
                    PhaseText.text = "Over Time";
                    break;
                }

            case MatchPhase.Penalty:
                {
                    PhaseText.text = "Penalty Shootout";
                    break;
                }
        }
    }

    /// <summary>
    /// 切换暂停事件
    /// </summary>
    void TogglePause()
    {
        Paused = !Paused;
    }

    /// <summary>
    /// 可能的话，向后推一拍。
    /// 通过将SliderPostion加一，自动调用Slider上绑定的函数。
    /// </summary>
    void Next()
    {
        if (SliderPosition < Recorder.DataLength - 1)
        {
            SliderPosition++;
        }
    }

    /// <summary>
    /// 可能的话，向前推一拍。
    /// 通过将SliderPostion减一，自动调用Slider上绑定的函数。
    /// </summary>
    void Previous()
    {
        if (SliderPosition > 0)
        {
            SliderPosition--;
        }
    }

    /// <summary>
    /// 返回按钮点击
    /// </summary>
    public void OnBackToGameClicked()
    {
        // 先禁用掉replay场景中的物体，防止与激活后的play场景物体发生碰撞
        Entity.SetActive(false);
        SceneManager.LoadScene("GameScene_Play");
    }

    /// <summary>
    /// 滑动条值改变
    /// </summary>
    public void OnSliderValueChanged()
    {
        Render(Recorder.IndexOf(SliderPosition));
    }

    /// <summary>
    /// 用户在进度条上点击，则暂停播放
    /// </summary>
    public void OnSliderClicked()
    {
        Paused = true;
    }

    /// <summary>
    /// 鼠标在进度条上滑动
    /// </summary>
    /// <param name="_data"></param>
    public void OnSliderScrolled(BaseEventData _data)
    {
        var data = _data as PointerEventData;
        SliderPosition += (int)data.scrollDelta.y;
    }

    /// <summary>
    /// 暂停按钮点击
    /// </summary>
    public void OnPauseClicked()
    {
        TogglePause();
    }

    /// <summary>
    /// Next按钮点击。播放下一拍并暂停。
    /// </summary>
    public void OnNextClicked()
    {
        Next();
        Paused = true;
    }

    /// <summary>
    /// Previous按钮点击。播放上一拍并暂停。
    /// </summary>
    public void OnPreviousClicked()
    {
        Previous();
        Paused = true;
    }

    /// <summary>
    /// 用户选择的速度改变，更改FixedUpdate频率。
    /// </summary>
    public void OnSpeedChanged(int value = -1)
    {
        switch (SpeedDropdown.value)
        {
            case 0:
                Time.timeScale = GeneralConfig.TimeScale;
                break;
            case 1:
                Time.timeScale = GeneralConfig.TimeScale / 2;
                break;
            case 2:
                Time.timeScale = GeneralConfig.TimeScale / 5;
                break;
            case 3:
                Time.timeScale = GeneralConfig.TimeScale / 10;
                break;
        }
    }

    /// <summary>
    /// 鼠标在速度滑动条上滑动事件。
    /// </summary>
    /// <param name="data"></param>
    public void OnSpeedScrolled(BaseEventData data)
    {
        var pointerEventData = (PointerEventData)data;
        SpeedDropdown.value -= (int)pointerEventData.scrollDelta.y;
    }

    public void ExportDataRecord()
    {
        string path = StandaloneFileBrowser.SaveFilePanel(
            "Export Data Record",
            "",
            $"match-{DateTime.Now:yyyy-MM-dd_hhmmss}.json",
            "json");

        // if save file panel cancelled
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string json = Recorder.Serialize();
        File.WriteAllText(path, json);
    }

    public void ImportDataRecord()
    {
        string[] path = StandaloneFileBrowser.OpenFilePanel(
            "Import Data Record",
            "",
            "json",
            false);

        // if open file panel cancelled
        if (path.Length == 0)
        {
            return;
        }

        string json = File.ReadAllText(path[0]);
        Recorder = new DataRecorder(json);
        ShowRecord();
    }
}
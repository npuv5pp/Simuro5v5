/********************************************************************************
 * GUI_Replay
 * 1.回放界面：包括进度条、播放、暂停、快退、快进、调整速度、esc进入菜单、显示栏
 * 2.回放菜单：包括回到比赛、返回播放、退出
********************************************************************************/

using System;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Simuro5v5;
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
    public TMP_Dropdown SpeedDropdown;

    // state text
    public TMP_Text DataTag;
    public TMP_Text tickText;
    public TMP_Text PhaseText;
    public TMP_Text JudgeResultText;
    public TMP_Text BlueScore, YellowScore;

    public static DataRecorder Recorder { get; set; }
    private ObjectManager ObjectManager { get; set; }

    private DelayRepeatKey _leftArrow = new DelayRepeatKey(KeyCode.LeftArrow);
    private DelayRepeatKey _rightArrow = new DelayRepeatKey(KeyCode.RightArrow);
    private DelayRepeatKey _pageUp = new DelayRepeatKey(KeyCode.PageUp);
    private DelayRepeatKey _pageDown = new DelayRepeatKey(KeyCode.PageDown);

    private bool _paused;

    private int SliderPosition
    {
        get => (int) Slider.value;
        set => Slider.value = value;
    }

    void Start()
    {
        ObjectManager = new ObjectManager();
        ShowRecord();
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

        _paused = true;

        _leftArrow.AddDownListener(OnPreviousClicked);
        _leftArrow.AddRepeatListener(times => { OnPreviousClicked(); });
        _rightArrow.AddDownListener(OnNextClicked);
        _rightArrow.AddRepeatListener(times => { OnNextClicked(); });
        _pageDown.AddDownListener(OnPreviousKeyFrame);
        _pageDown.AddRepeatListener(times => { OnPreviousKeyFrame(); });
        _pageUp.AddDownListener(OnNextKeyFrame);
        _pageUp.AddRepeatListener(times => { OnNextKeyFrame(); });
    }

    void Update()
    {
        DataTag.SetText(Recorder.Name);
        PauseButtonImage.sprite = _paused ? PauseButtonPaused : PauseButtonNonPaused;

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

        if (Input.GetKeyDown(KeyCode.Home))
        {
            OnHomeClicked();
        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            OnEndClicked();
        }
    }

    private int _tick = 0;

    void FixedUpdate()
    {
        _tick++;
        if (_tick % 2 == 0)
        {
            return;
        }

        if (!_paused)
        {
            if (SliderPosition == Recorder.DataLength - 1)
            {
                _paused = true;
            }
            else
            {
                Next();
            }
        }
        
        _rightArrow.UpdateState();
        _leftArrow.UpdateState();
        _pageUp.UpdateState();
        _pageDown.UpdateState();
    }

    private void OnEndClicked()
    {
        End();
        _paused = true;
    }

    private void End()
    {
        SliderPosition = Recorder.DataLength - 1;
    }

    private void OnHomeClicked()
    {
        Home();
        _paused = true;
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
        _paused = true;
    }

    private void NextKeyFrame()
    {
        if (SliderPosition == Recorder.DataLength - 1)
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
        _paused = true;
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
        _paused = !_paused;
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
        _paused = true;
    }

    /// <summary>
    /// 鼠标在进度条上滑动
    /// </summary>
    /// <param name="_data"></param>
    public void OnSliderScrolled(BaseEventData _data)
    {
        var data = _data as PointerEventData;
        SliderPosition += (int) data.scrollDelta.y;
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
        _paused = true;
    }

    /// <summary>
    /// Previous按钮点击。播放上一拍并暂停。
    /// </summary>
    public void OnPreviousClicked()
    {
        Previous();
        _paused = true;
    }

    /// <summary>
    /// 用户选择的速度改变，更改FixedUpdate频率。
    /// </summary>
    public void OnSpeedChanged(int value = -1)
    {
        switch (SpeedDropdown.value)
        {
            case 0:
                Time.timeScale = TimeConfig.TimeScale;
                break;
            case 1:
                Time.timeScale = TimeConfig.TimeScale / 2;
                break;
            case 2:
                Time.timeScale = TimeConfig.TimeScale / 5;
                break;
            case 3:
                Time.timeScale = TimeConfig.TimeScale / 10;
                break;
        }
    }

    /// <summary>
    /// 鼠标在速度滑动条上滑动事件。
    /// </summary>
    /// <param name="data"></param>
    public void OnSpeedScrolled(BaseEventData data)
    {
        var pointerEventData = (PointerEventData) data;
        SpeedDropdown.value -= (int) pointerEventData.scrollDelta.y;
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

internal class DelayRepeatKey
{
    private KeyCode _bindKey;

    private enum KeyState
    {
        Up, // 抬起
        Wait, // 刚按下，延迟中，等待重复
        Repeat // 重复中
    }

    private KeyState _keyState;
    private Action _downAction;
    private Action<int> _repeatAction;

    /// <summary>
    /// 已按下保持时间
    /// </summary>
    private float _holdTime;

    /// <summary>
    /// 上次按下时间
    /// </summary>
    private float _lastDownTime;

    /// <summary>
    /// 重复次数
    /// </summary>
    private int _repeatTimes;

    /// <summary>
    /// 最小等待时间，按下保持超过该时间则开始触发重复事件
    /// </summary>
    public float MinWaitTime { get; set; }

    public DelayRepeatKey(KeyCode keyCode, float waitTime = 0.5f)
    {
        _bindKey = keyCode;
        MinWaitTime = waitTime;
    }

    public void AddDownListener(Action func)
    {
        _downAction += func;
    }

    public void AddRepeatListener(Action<int> func)
    {
        _repeatAction += func;
    }

    public void UpdateState()
    {
        var down = Input.GetKey(_bindKey);

        if (!down)
        {
            _keyState = KeyState.Up;
            _holdTime = 0;
            _repeatTimes = 0;
            return;
        }

        switch (_keyState)
        {
            case KeyState.Up:
                // 当前是抬起状态，第一次按下
                _keyState = KeyState.Wait;
                _lastDownTime = Time.realtimeSinceStartup;
                _holdTime = 0;
                _repeatTimes = 0;
                _downAction();
                break;
            case KeyState.Wait:
                _holdTime += Time.realtimeSinceStartup - _lastDownTime;
                _lastDownTime = Time.realtimeSinceStartup;
                if (_holdTime >= MinWaitTime)
                    _keyState = KeyState.Repeat;
                break;
            case KeyState.Repeat:
                _holdTime += Time.realtimeSinceStartup - _lastDownTime;
                _lastDownTime = Time.realtimeSinceStartup;
                _repeatTimes++;
                _repeatAction(_repeatTimes);
                break;
        }
    }
}
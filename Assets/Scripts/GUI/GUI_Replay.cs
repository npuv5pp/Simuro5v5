/********************************************************************************
 * GUI_Replay
 * 1.回放界面：包括进度条、播放、暂停、快退、快进、调整速度、esc进入菜单、显示栏
 * 2.回放菜单：包括回到比赛、返回播放、退出
********************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Simuro5v5;
using UnityEngine.EventSystems;

public class GUI_Replay : MonoBehaviour
{
    public Slider Slider;
    public DataBoard DataBoard;
    public GameObject Entity;
    public TMP_Text DataName;
    public TMP_Dropdown SpeedDropdown;
    public StateBoard StateBoard;
    public Image PauseButtonImage;
    public Sprite PauseButtonPaused;
    public Sprite PauseButtonNonPaused;

    public static DataRecorder Recorder { get; set; }
    private ObjectManager ObjectManager { get; set; }

    bool Paused;

    private int SliderPostion
    {
        get
        {
            return (int)Slider.value;
        }
        set
        {
            Slider.value = value;
        }
    }

    void Start()
    {
        if (Recorder == null || Recorder.DataLength == 0)
        {
            Recorder = new DataRecorder();
            for (int i = 0; i < 100; i++)
            {
                Recorder.Add(new DataRecorder.StateRecodeData(DataRecorder.DataType.InPlaying, new MatchInfo()));
                Recorder.Add(new DataRecorder.StateRecodeData(DataRecorder.DataType.InPlaying, MatchInfo.newDefaultPreset()));
            }
        }

        ObjectManager = new ObjectManager();
        ObjectManager.RebindObject(Entity);
        ObjectManager.DisablePhysics();
        ObjectManager.Resume();
        Slider.minValue = 0;
        Slider.maxValue = Recorder.DataLength == 0 ? 0 : Recorder.DataLength - 1;
        Render(Recorder.Get(0));

        Paused = true;
    }

    void Update()
    {
        DataName.SetText(Recorder.Name);
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

    void FixedUpdate()
    {
        if (!Paused)
        {
            if (SliderPostion == Recorder.DataLength - 1)
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
    }

    /// <summary>
    /// 渲染一拍的数据到场景中，包括：机器人和球的坐标，数据板
    /// </summary>
    /// <param name="matchInfo">要渲染的场景信息</param>
    void Render(DataRecorder.BaseRecodeData d)
    {
        if (!(d is DataRecorder.StateRecodeData data))
        {
            return;
        }
        switch (data.type)
        {
            case DataRecorder.DataType.InPlaying:
                {
                    if (data.matchInfo != null)
                    {
                        ObjectManager.RevertScene(data.matchInfo);
                        DataBoard.Render(data.matchInfo);
                    }
                }
                break;
            case DataRecorder.DataType.NewMatch:
            case DataRecorder.DataType.NewRound:
            case DataRecorder.DataType.AutoPlacement:
                break;
        }
        StateBoard.Render(data.type);
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
        if (SliderPostion < Recorder.DataLength - 1)
        {
            SliderPostion++;
        }
    }

    /// <summary>
    /// 可能的话，向前推一拍。
    /// 通过将SliderPostion减一，自动调用Slider上绑定的函数。
    /// </summary>
    void Previous()
    {
        if (SliderPostion > 0)
        {
            SliderPostion--;
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
        Render(Recorder.Get(SliderPostion));
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
        SliderPostion += (int)data.scrollDelta.y;
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
                Time.timeScale = 1;
                break;
            case 1:
                Time.timeScale = 0.5f;
                break;
            case 2:
                Time.timeScale = 0.2f;
                break;
            case 3:
                Time.timeScale = 0.1f;
                break;
        }

    }

    /// <summary>
    /// 鼠标在速度滑动条上滑动事件。
    /// </summary>
    /// <param name="_data"></param>
    public void OnSpeedScrolled(BaseEventData _data)
    {
        var data = _data as PointerEventData;
        SpeedDropdown.value -= (int)data.scrollDelta.y;
    }
}

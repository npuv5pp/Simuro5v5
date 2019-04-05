/********************************************************************************
 * GUI_Replay
 * 1.回放界面：包括进度条、播放、暂停、快退、快进、调整速度、esc进入菜单、显示栏
 * 2.回放菜单：包括回到比赛、返回播放、退出
********************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Simuro5v5;
using Event = Simuro5v5.EventSystem.Event;

public class GUI_Replay : MonoBehaviour
{
    public Slider Slider;
    public DataBoard DataBoard;
    public GameObject Entity;

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
        if (Recorder == null)
        {
            Recorder = new DataRecorder();
            for (int i = 0; i < 100; i++)
            {
                Recorder.Add(new MatchInfo());
                Recorder.Add(MatchInfo.DefaultMatch);
            }
        }

        ObjectManager = new ObjectManager();
        ObjectManager.RebindObject(Entity);
        ObjectManager.DisableRigidBodyAndCollider();
        ObjectManager.Resume();
        Slider.minValue = 0;
        Slider.maxValue = Recorder.DataLength == 0 ? 0 : Recorder.DataLength - 1;
        Render(Recorder.Get(0));

        Paused = true;
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
    }

    /// <summary>
    /// 渲染一拍的数据到场景中，包括：机器人和球的坐标，数据板
    /// </summary>
    /// <param name="matchInfo">要渲染的场景信息</param>
    void Render(MatchInfo matchInfo)
    {
        if (matchInfo != null)
        {
            ObjectManager.RevertScene(matchInfo);
            DataBoard.Render(matchInfo);
        }
        else
        {
            Debug.Log("null matchinfo");
        }
    }

    void TogglePause()
    {
        Paused = !Paused;
    }

    void Next()
    {
        if (SliderPostion < Recorder.DataLength - 1)
        {
            SliderPostion++;
        }
    }

    void Previous()
    {
        if (SliderPostion > 0)
        {
            SliderPostion--;
        }
    }

    public void OnBackToGameClicked()
    {
        // 先禁用掉replay场景中的物体，防止与激活后的play场景物体发生碰撞
        Entity.SetActive(false);
        SceneManager.LoadScene("GameScene_Play");
    }

    public void OnSliderValueChanged()
    {
        Render(Recorder.Get(SliderPostion));
    }

    public void OnPauseClicked()
    {
        TogglePause();
    }

    public void OnNextClicked()
    {
        Next();
    }

    public void OnPreviousClicked()
    {
        Previous();
    }
}

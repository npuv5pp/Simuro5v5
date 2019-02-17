/************************************************************************
 * GUI_Menu1
 * 用于测试
 * 未完成（按优先顺序）：dyk数据流、显示栏、GUISkin、
************************************************************************/

using System.Collections;
using System.Collections.Generic;
using Simuro5v5;
using Simuro5v5.Strategy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class game_slider : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject obj = GameObject.Find("Slider");
        if (obj == null)
        {
            Debug.Log("No Such Object.");
        }
        Slider gslider = (Slider)obj.GetComponent<Slider>();
        gslider.onValueChanged.AddListener(delegate { info_update(); });
	}
	
	// Update is called once per frame
	void Update () {
		
	}

     void info_update()
    { 
        /*
        MatchInfo BlueInfo = obj1 as MatchInfo;
        MatchInfo YellowInfo = obj2 as MatchInfo;

        if (BlueInfo == null || YellowInfo == null) 
        {
            Debug.Log("Slider Cannot Get Info.");
        }

        B0_vl.text = BlueInfo.BlueRobot[0].velocityLeft.ToString();
        */

        GameObject obj = GameObject.Find("Slider");
        Slider x = (Slider)obj.GetComponent<Slider>();

        GameObject obj1 = GameObject.Find("B0_vl");
        Text y = (Text)obj1.GetComponent<Text>();
        y.text = x.value.ToString();
    }
}

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

public class GUI_Replay : MonoBehaviour {

    
    bool Open_Menu = false;
    bool Click_Play = true;
    int Play_Speed = 1;
    bool Left_to_Right = true;
    int i = 0;
    bool Side_Appear = false;
    public Sprite icon_play;
    public Sprite icon_pause;
    public int step = 0;
    public Sprite icon_tri;
    public Sprite icon_triopp;


    DataRecorder dr = new DataRecorder();

	// Use this for initialization
	void Start ()
    {
        Close_ReplayMenu();
        Open_Menu = false;
        Side_Appear = false;
        //Replay();
        Debug.Log("end" + PlayerPrefs.GetInt("step_end").ToString());
        Debug.Log("start" + PlayerPrefs.GetInt("step_start").ToString());
        dr.Initialize();
        

        GameObject obj = GameObject.Find("mainslider");
        if (obj == null)
        {
            Debug.Log("No mSlider Object.");
        }
        else
        {
            Debug.Log("mSlider Object.");
        }
        Slider mainslider = (Slider)obj.GetComponent<Slider>();

        mainslider.maxValue = PlayerPrefs.GetInt("step_end");
        mainslider.minValue = PlayerPrefs.GetInt("step_start");

        GameObject obj1 = GameObject.Find("Image");
        if (obj1 == null)
        {
            //Debug.Log("No B0_vl Object.");
        }
        else
        {
            //Debug.Log("B0_vl Object.");
        }
        obj1.SetActive(false);

        ObjectManager.RegisterReplay();
    }
	
	// Update is called once per frame
	void Update ()
    {
        GameObject obj = GameObject.Find("mainslider");
        if (obj == null)
        {
            Debug.Log("No mSlider Object.");
        }
        else
        {
            Debug.Log("mSlider Object.");
        }
        Slider mainslider = (Slider)obj.GetComponent<Slider>();

        step = (int)mainslider.value;

        PlayerPrefs.SetInt("last_speed", Play_Speed);
        if (Left_to_Right == false)
        {
            PlayerPrefs.SetInt("last_orientation", 0);
        }
        else
        {
            PlayerPrefs.SetInt("last_orientation", 1);
        }

        if (Click_Play == false) 
        {
            //Debug.Log("Clickfalse" + i.ToString());
        }
        if (Click_Play == true)
        {
            //Debug.Log("Clicktrue" + i.ToString());
        }
        if(Left_to_Right==true)
        {
            //Debug.Log("Left_to_Right" + i.ToString());
        }
        if (Left_to_Right == false)
        {
            //Debug.Log("Right_to_Left" + i.ToString());
        }
        //Debug.Log("speed_of_slider" + Play_Speed.ToString());
        Replay();
        i++;
	}

    void Replay()
    {
        Call_text_esc();
        if (Open_Menu == false)
        {
            Open_ReplayView();
        }
        else
        {
            Call_button_resume();
            Call_button_backtogame();
            Call_button_exit();
        }
    }

    void Open_ReplayView()
    {
        GameObject obj = GameObject.Find("mainslider");
        if (obj == null)
        {
            Debug.Log("No mSlider Object.");
        }
        else
        {
            Debug.Log("mSlider Object.");
        }
        Slider mainslider = (Slider)obj.GetComponent<Slider>();

        int i = (int)mainslider.value;
        int maxi = (int)mainslider.maxValue;
        int mini = (int)mainslider.minValue;


        if ((i == mini || Left_to_Right == false || Play_Speed != 1) && Click_Play == false)
        {
            Call_button_play();
        }
        //Call_slider_sideslider();
        
        Call_button_forward();
        Call_button_back();
        if (i == mini || i == maxi)
        {
            Play_Speed = 1;
        }
        if (Click_Play==true)
        {
            //Debug.Log("i" + i.ToString());
            //Debug.Log("maxi" + maxi.ToString());
            int speed = 1;
            while(speed<=Play_Speed)
            {
                i = (int)mainslider.value;
                if(Left_to_Right==true)
                {
                    if (i <= maxi)
                    {
                        mainslider.value = mainslider.value + 1;
                    }
                }
                else
                {
                    if (i >= mini)
                    {
                        mainslider.value = mainslider.value - 1;
                    }
                }
                speed++;
            }
            if (Click_Play == true)
            {
                Call_button_pause();
            }
        }
        Call_slider_mainslider();
        Call_button_appear();
    }

    void Call_text_esc()
    {
        GameObject obj1 = GameObject.Find("esc");
        if (obj1 == null)
        {
            Debug.Log("No Text Object.");
        }
        else
        {
            Debug.Log("Text Object.");
        }
        Text text_esc = (Text)obj1.GetComponent<Text>();

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            bool flag_goto = true;
            if (Open_Menu == false)
            {
                GameObject scenes = GameObject.Find("MatchScene");
                if (scenes == null)
                {
                    Debug.Log("No Scene Object.");
                }
                else
                {
                    Debug.Log("Scene Object.");
                }
                GameObject gf = (GameObject)Instantiate(Resources.Load("GreyField2"));
                Debug.Log("create gf");
                gf.transform.name = "gf";
                gf.transform.parent = scenes.transform;
                Debug.Log("create gf2");
                /*
                var me = GetComponent<Material>();
                Debug.Log("create gf23");
                me.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                me.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                me.SetInt("_ZWrite", 0);
                me.DisableKeyword("_ALPHATEST_ON");
                me.DisableKeyword("_ALPHABLEND_ON");
                me.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                me.renderQueue = 3000;
                */
                Open_ReplayMenu();
                Open_Menu = true;
                flag_goto = false;
            }

            if (Open_Menu == true && flag_goto == true)
            {
                Close_ReplayMenu();
                Open_Menu = false;

                GameObject obj3 = GameObject.Find("Greyfield2");
                if (obj3 == null)
                {
                    Debug.Log("No gf-c Object.");
                }
                else
                {
                    Debug.Log("gf-c Object.");
                }

                //GameObject gf = (GameObject)Instantiate(Resources.Load("greyfield"));
                Destroy(GameObject.Find("gf").gameObject);
            }
        }
    }

    void Call_slider_mainslider()
    {
        GameObject obj = GameObject.Find("mainslider");
        if (obj == null)
        {
            Debug.Log("No mSlider Object.");
        }
        else
        {
            Debug.Log("mSlider Object.");
        }
        Slider mainslider = (Slider)obj.GetComponent<Slider>();

        Debug.Log("Max" + mainslider.maxValue.ToString());

        Call_Keyboard();

        mainslider.onValueChanged.AddListener(delegate
        {
            Info_update();
        });
    }

    void Call_slider_sideslider()
    {
        GameObject obj = GameObject.Find("sideslider");
        if (obj == null)
        {
            Debug.Log("No Slider Object.");
        }
        else
        {
            Debug.Log("Slider Object.");
        }
        Slider sideslider = (Slider)obj.GetComponent<Slider>();
        sideslider.onValueChanged.AddListener(delegate
        {
            GameObject obj1 = GameObject.Find("speed");
            if (obj1 == null)
            {
                Debug.Log("No speed Object.");
            }
            else
            {
                Debug.Log("speed Object.");
            }
            Text speed = (Text)obj1.GetComponent<Text>();
            speed.text = "X" + Mathf.Pow(2, sideslider.value).ToString();
        });
    }

    void Close_ReplayView()
    {
        GameObject obj1 = GameObject.Find("mainslider");
        if (obj1 == null)
        {
            Debug.Log("No Mainslider Object.");
        }
        else
        {
            Debug.Log("Mainslider Object.");
        }
        obj1.GetComponent<Slider>().enabled = false;

        GameObject obj2 = GameObject.Find("sideslider");
        if (obj2 == null)
        {
            Debug.Log("No Sideslider Object.");
        }
        else
        {
            Debug.Log("Sideslider Object.");
        }
        obj2.GetComponent<Slider>().enabled = false;

        GameObject obj3 = GameObject.Find("play");
        if (obj3 == null)
        {
            Debug.Log("No play Object.");
        }
        else
        {
            Debug.Log("play Object.");
        }
        obj3.GetComponent<Image>().enabled = false;

        GameObject obj4 = GameObject.Find("pause");
        if (obj4 == null)
        {
            Debug.Log("No pause Object.");
        }
        else
        {
            Debug.Log("pause Object.");
        }
        obj4.GetComponent<Image>().enabled = false;

        GameObject obj5 = GameObject.Find("back");
        if (obj5 == null)
        {
            Debug.Log("No back Object.");
        }
        else
        {
            Debug.Log("back Object.");
        }
        obj5.GetComponent<Image>().enabled = false;

        GameObject obj6 = GameObject.Find("forward");
        if (obj6 == null)
        {
            Debug.Log("No forward Object.");
        }
        else
        {
            Debug.Log("forward Object.");
        }
        obj6.GetComponent<Image>().enabled = false;

        GameObject obj7 = GameObject.Find("num");
        if (obj7 == null)
        {
            Debug.Log("No num Object.");
        }
        else
        {
            Debug.Log("num Object.");
        }
        obj7.GetComponent<Text>().enabled = false;

        GameObject obj8 = GameObject.Find("speed");
        if (obj8 == null)
        {
            Debug.Log("No speed Object.");
        }
        else
        {
            Debug.Log("speed Object.");
        }
        obj8.GetComponent<Text>().enabled = false;
    }

    void Call_button_play()
    {
        
        GameObject obj = GameObject.Find("mainslider");
        if (obj == null)
        {
            Debug.Log("No Slider Object.");
        }
        else
        {
            Debug.Log("Slider Object.");
        }
        Slider mainslider = (Slider)obj.GetComponent<Slider>();

        GameObject obj1 = GameObject.Find("play");
        if (obj1 == null)
        {
            Debug.Log("No play Object.");
        }
        else
        {
            Debug.Log("play Object.");
        }
        Button play = (Button)obj1.GetComponent<Button>();

        play.onClick.RemoveAllListeners();
        play.onClick.AddListener(delegate
        {
            Left_to_Right = true;
            Click_Play = true;
            Play_Speed = 1;
            play.GetComponent<Image>().sprite = icon_pause;
        });
    }

    void Call_button_pause()
    {
        GameObject obj1 = GameObject.Find("play");
        if (obj1 == null)
        {
            Debug.Log("No pause Object.");
        }
        else
        {
            Debug.Log("pause Object.");
        }
        Button pause = (Button)obj1.GetComponent<Button>();

        pause.onClick.RemoveAllListeners();
        pause.onClick.AddListener(delegate
        {
            Click_Play = false;
            pause.GetComponent<Image>().sprite = icon_play;
        });
    }

    void Call_Keyboard()
    {
        if(Input.GetKeyUp(KeyCode.A))
        {
            GameObject obj = GameObject.Find("mainslider");
            if (obj == null)
            {
                Debug.Log("No mSlider Object.");
            }
            else
            {
                Debug.Log("mSlider Object.");
            }
            Slider mainslider = (Slider)obj.GetComponent<Slider>();
            int i = (int)mainslider.value;
            int maxi = (int)mainslider.maxValue;
            Debug.Log("i" + i.ToString());
            Debug.Log("maxi" + maxi.ToString());
            if (i <= maxi)
            {
                mainslider.value = mainslider.value + 1;
            }
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            GameObject obj = GameObject.Find("mainslider");
            if (obj == null)
            {
                Debug.Log("No mSlider Object.");
            }
            else
            {
                Debug.Log("mSlider Object.");
            }
            Slider mainslider = (Slider)obj.GetComponent<Slider>();
            int i = (int)mainslider.value;
            int maxi = (int)mainslider.maxValue;
            Debug.Log("i" + i.ToString());
            Debug.Log("maxi" + maxi.ToString());
            if (i <= maxi)
            {
                mainslider.value = mainslider.value - 1;
            }
        }
    }

    void Call_button_back()
    {
        GameObject obj = GameObject.Find("back");
        if (obj == null)
        {
            Debug.Log("No back Object.");
        }
        else
        {
            Debug.Log("back Object.");
        }
        Button back = (Button)obj.GetComponent<Button>();
        back.onClick.RemoveAllListeners();
        back.onClick.AddListener(delegate
        {
            if (Click_Play == false)
            {
                Click_Play = true;
                Play_Speed = 1;
                Left_to_Right = false;
            }
            else
            {
                if (Left_to_Right == false)
                {
                    if (Play_Speed <= 4)
                    {
                        Play_Speed = Play_Speed * 2;
                    }
                }
                if (Left_to_Right == true)
                {
                    if (Play_Speed > 1)
                    {
                        Play_Speed = Play_Speed / 2;
                    }
                    if (Play_Speed == 1)
                    {
                        Left_to_Right = false;
                    }
                }
            }
        });
    }

    void Call_button_forward()
    {
        GameObject obj = GameObject.Find("forward");
        if (obj == null)
        {
            Debug.Log("No forward Object.");
        }
        else
        {
            Debug.Log("forward Object.");
        }
        Button forward = (Button)obj.GetComponent<Button>();
        forward.onClick.RemoveAllListeners();
        forward.onClick.AddListener(delegate
        {
            if (Click_Play == false)
            {
                Click_Play = true;
                Play_Speed = 1;
                Left_to_Right = true;
            }
            else
            {
                if (Left_to_Right == true)
                {
                    if (Play_Speed <= 4)
                    {
                        Play_Speed = Play_Speed * 2;
                    }
                }
                if (Left_to_Right == false)
                {
                    if (Play_Speed > 1)
                    {
                        Play_Speed = Play_Speed * 2;
                    }
                    if (Play_Speed == 1)
                    {
                        Left_to_Right = true;
                    }
                }
            }
        });
    }

    void Open_ReplayMenu()
    {
        GameObject obj3 = GameObject.Find("Canvas");
        if (obj3 == null)
        {
            Debug.Log("No Canvas Object.");
        }
        else
        {
            Debug.Log("Canvas Object.");
        }

        GameObject obj4 = obj3.transform.Find("backtogame").gameObject;
        if (obj4 == null)
        {
            Debug.Log("No BtG Object.");
        }
        else
        {
            Debug.Log("BtG Object.");
        }
        obj4.SetActive(true);

        GameObject obj5 = obj3.transform.Find("resume").gameObject;
        if (obj5 == null)
        {
            Debug.Log("No Resume Object.");
        }
        else
        {
            Debug.Log("Resume Object.");
        }
        obj5.SetActive(true);

        GameObject obj6 = obj3.transform.Find("exit").gameObject;
        if (obj6 == null)
        {
            Debug.Log("No Exit Object.");
        }
        else
        {
            Debug.Log("Exit Object.");
        }
        obj6.SetActive(true);
    }

    void Close_ReplayMenu()
    {
        GameObject obj1 = GameObject.Find("backtogame");
        if (obj1 == null)
        {
            Debug.Log("No BtG Object.");
        }
        else
        {
            Debug.Log("BtG Object.");
        }
        obj1.SetActive(false);

        GameObject obj2 = GameObject.Find("resume");
        if (obj2 == null)
        {
            Debug.Log("No Resume Object.");
        }
        else
        {
            Debug.Log("Resume Object.");
        }
        obj2.SetActive(false);

        GameObject obj3 = GameObject.Find("exit");
        if (obj3 == null)
        {
            Debug.Log("No Exit Object.");
        }
        else
        {
            Debug.Log("Exit Object.");
        }
        obj3.SetActive(false);
    }

    void Call_button_resume()
    {
        GameObject obj = GameObject.Find("resume");
        if (obj == null)
        {
            Debug.Log("No Resume Object.");
        }
        else
        {
            Debug.Log("Resume Object.");
        }
        Button resume = (Button)obj.GetComponent<Button>();

        resume.onClick.RemoveAllListeners();
        resume.onClick.AddListener(delegate
        {
            
            if (Open_Menu == true)
            {
                Close_ReplayMenu();
                Open_ReplayView();
                Open_Menu = false;


                GameObject obj3 = GameObject.Find("greyfield");
                if (obj3 == null)
                {
                    Debug.Log("No gf-c Object.");
                }
                else
                {
                    Debug.Log("gf-c Object.");
                }

                //GameObject gf = (GameObject)Instantiate(Resources.Load("greyfield"));
                Destroy(GameObject.Find("gf").gameObject);
            }
        });
    }

    void Call_button_backtogame()
    {
        GameObject obj = GameObject.Find("backtogame");
        if (obj == null)
        {
            Debug.Log("No Replay Object.");
        }
        else
        {
            Debug.Log("Replay Object.");
        }
        Button replay = (Button)obj.GetComponent<Button>();

        replay.onClick.RemoveAllListeners();
        replay.onClick.AddListener(delegate
        {
            PlayerPrefs.SetInt("step_start", PlayerPrefs.GetInt("step_end"));
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene_Play");
        });
    }

    void Call_button_exit()
    {
        GameObject obj = GameObject.Find("exit");
        if (obj == null)
        {
            Debug.Log("No Exit Object.");
        }
        else
        {
            Debug.Log("Exit Object.");
        }
        Button exit = (Button)obj.GetComponent<Button>();

        exit.onClick.RemoveAllListeners();
        exit.onClick.AddListener(delegate
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        });
    }

    void Call_button_appear()
    {
        GameObject obj = GameObject.Find("appear");
        if (obj == null)
        {
            //Debug.Log("No appear Object.");
        }
        else
        {
            //Debug.Log("appear Object.");
        }
        Button appear = (Button)obj.GetComponent<Button>();

        appear.onClick.RemoveAllListeners();
        appear.onClick.AddListener(delegate
        {
            bool flag_goto = true;


            if (Side_Appear == true) 
            {
                appear.GetComponent<Image>().sprite = icon_tri;
                GameObject obj1 = GameObject.Find("Image");
                if (obj1 == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }
                obj1.SetActive(false);
                
                Debug.Log("B0_vlclose");
                //B0_vl.text = "1";
                Side_Appear = false;
                flag_goto = false;
            }
            if (Side_Appear == false && flag_goto == true) 
            {
                appear.GetComponent<Image>().sprite = icon_triopp;
                //---------------------------------------g------------------------------------------
                Resolution[] resolution = Screen.resolutions;
                float standard_width = resolution[resolution.Length - 1].width;
                float standard_height = (standard_width/2);
                Screen.SetResolution(System.Convert.ToInt32(standard_width), System.Convert.ToInt32(standard_height), false);
                
                GameObject obj0 = GameObject.Find("Canvas");
                if (obj0 == null)
                {
                    //Debug.Log("No Canvas Object.");
                }
                else
                {
                    //Debug.Log("Canvas Object.");
                }
                GameObject image = obj0.transform.Find("Image").gameObject;
                if (image == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }
                image.SetActive(true);

                Text B0tag = image.transform.Find("B0tag").GetComponent<Text>(); 
                if (B0tag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                Text B0xtag = B0tag.transform.Find("B0xtag").GetComponent<Text>(); 
                if (B0xtag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField B0x = B0xtag.transform.Find("B0x").GetComponent<TMPro.TMP_InputField>();
                if (B0x == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                B0x.text = ".";//dr.Index(step).BlueRobot[0].pos.x.ToString()

                Text B0ytag = B0tag.transform.Find("B0ytag").GetComponent<Text>(); 
                if (B0ytag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B0ytag Object.");
                }

                TMPro.TMP_InputField B0y = B0ytag.transform.Find("B0y").GetComponent<TMPro.TMP_InputField>();
                if (B0y == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B0y Object.");
                }

                //----------------------------改---------------------------//
                B0y.text = ".";//dr.Index(step).BlueRobot[0].pos.y.ToString()

                Text B1tag = image.transform.Find("B1tag").GetComponent<Text>(); 
                if (B1tag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                Text B1xtag = B1tag.transform.Find("B1xtag").GetComponent<Text>(); 
                if (B1xtag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField B1x = B1xtag.transform.Find("B1x").GetComponent<TMPro.TMP_InputField>();
                if (B1x == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                B1x.text = ".";//dr.Index(step).BlueRobot[1].pos.x.ToString()

                Text B1ytag = B1tag.transform.Find("B1ytag").GetComponent<Text>(); 
                if (B1ytag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B0ytag Object.");
                }

                TMPro.TMP_InputField B1y = B1ytag.transform.Find("B1y").GetComponent<TMPro.TMP_InputField>();
                if (B1y == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B0y Object.");
                }

                //----------------------------改---------------------------//
                B1y.text = ".";//dr.Index(step).BlueRobot[1].pos.y.ToString()

                Text B2tag = image.transform.Find("B2tag").GetComponent<Text>();
                if (B2tag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                Text B2xtag = B2tag.transform.Find("B2xtag").GetComponent<Text>();
                if (B2xtag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField B2x = B2xtag.transform.Find("B2x").GetComponent<TMPro.TMP_InputField>();
                if (B2x == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                B2x.text = ".";//dr.Index(step).BlueRobot[2].pos.x.ToString()

                Text B2ytag = B2tag.transform.Find("B2ytag").GetComponent<Text>(); ;
                if (B2ytag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B2ytag Object.");
                }

                TMPro.TMP_InputField B2y = B2ytag.transform.Find("B2y").GetComponent<TMPro.TMP_InputField>();
                if (B2y == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B2y Object.");
                }

                //----------------------------改---------------------------//
                B2y.text = ".";//dr.Index(step).BlueRobot[2].pos.y.ToString()

                Text B3tag = image.transform.Find("B3tag").GetComponent<Text>();
                if (B3tag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                Text B3xtag = B3tag.transform.Find("B3xtag").GetComponent<Text>();
                if (B3xtag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField B3x = B3xtag.transform.Find("B3x").GetComponent<TMPro.TMP_InputField>();
                if (B3x == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                B3x.text = ".";//dr.Index(step).BlueRobot[3].pos.x.ToString()

                Text B3ytag = B3tag.transform.Find("B3ytag").GetComponent<Text>(); ;
                if (B3ytag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B3ytag Object.");
                }

                TMPro.TMP_InputField B3y = B3ytag.transform.Find("B3y").GetComponent<TMPro.TMP_InputField>();
                if (B3y == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B3y Object.");
                }

                //----------------------------改---------------------------//
                B3y.text = ".";//dr.Index(step).BlueRobot[3].pos.y.ToString()

                Text B4tag = image.transform.Find("B4tag").GetComponent<Text>();
                if (B4tag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                Text B4xtag = B4tag.transform.Find("B4xtag").GetComponent<Text>();
                if (B4xtag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField B4x = B4xtag.transform.Find("B4x").GetComponent<TMPro.TMP_InputField>();
                if (B4x == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                B4x.text = ".";//dr.Index(step).BlueRobot[4].pos.x.ToString()

                Text B4ytag = B4tag.transform.Find("B4ytag").GetComponent<Text>(); ;
                if (B4ytag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B4ytag Object.");
                }

                TMPro.TMP_InputField B4y = B4ytag.transform.Find("B4y").GetComponent<TMPro.TMP_InputField>();
                if (B4y == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    Debug.Log("B4y Object.");
                }

                //----------------------------改---------------------------//
                B4y.text = ".";//dr.Index(step).BlueRobot[4].pos.y.ToString()

                Text Y0tag = image.transform.Find("Y0tag").GetComponent<Text>();
                if (Y0tag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                Text Y0xtag = Y0tag.transform.Find("Y0xtag").GetComponent<Text>();
                if (Y0xtag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                TMPro.TMP_InputField Y0x = Y0xtag.transform.Find("Y0x").GetComponent<TMPro.TMP_InputField>();
                if (Y0x == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                //----------------------------改---------------------------//
                Y0x.text = ".";//dr.Index(step).YellowRobot[0].pos.x.ToString()

                Text Y0ytag = Y0tag.transform.Find("Y0ytag").GetComponent<Text>();
                if (Y0ytag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y0ytag Object.");
                }

                TMPro.TMP_InputField Y0y = Y0ytag.transform.Find("Y0y").GetComponent<TMPro.TMP_InputField>();
                if (Y0y == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y0y Object.");
                }

                //----------------------------改---------------------------//
                Y0y.text = ".";//dr.Index(step).YellowRobot[0].pos.y.ToString()

                Text Y1tag = image.transform.Find("Y1tag").GetComponent<Text>();
                if (Y1tag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                Text Y1xtag = Y1tag.transform.Find("Y1xtag").GetComponent<Text>();
                if (Y1xtag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                TMPro.TMP_InputField Y1x = Y1xtag.transform.Find("Y1x").GetComponent<TMPro.TMP_InputField>();
                if (Y1x == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                //----------------------------改---------------------------//
                Y1x.text = ".";//dr.Index(step).YellowRobot[1].pos.x.ToString()

                Text Y1ytag = Y1tag.transform.Find("Y1ytag").GetComponent<Text>();
                if (Y1ytag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y0ytag Object.");
                }

                TMPro.TMP_InputField Y1y = Y1ytag.transform.Find("Y1y").GetComponent<TMPro.TMP_InputField>();
                if (Y1y == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y0y Object.");
                }

                //----------------------------改---------------------------//
                Y1y.text = ".";//dr.Index(step).YellowRobot[1].pos.y.ToString()

                Text Y2tag = image.transform.Find("Y2tag").GetComponent<Text>();
                if (Y2tag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                Text Y2xtag = Y2tag.transform.Find("Y2xtag").GetComponent<Text>();
                if (Y2xtag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                TMPro.TMP_InputField Y2x = Y2xtag.transform.Find("Y2x").GetComponent<TMPro.TMP_InputField>();
                if (Y2x == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                //----------------------------改---------------------------//
                Y2x.text = ".";//dr.Index(step).YellowRobot[2].pos.x.ToString()

                Text Y2ytag = Y2tag.transform.Find("Y2ytag").GetComponent<Text>(); ;
                if (Y2ytag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y2ytag Object.");
                }

                TMPro.TMP_InputField Y2y = Y2ytag.transform.Find("Y2y").GetComponent<TMPro.TMP_InputField>();
                if (Y2y == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y2y Object.");
                }

                //----------------------------改---------------------------//
                Y2y.text = ".";//dr.Index(step).YellowRobot[2].pos.y.ToString()

                Text Y3tag = image.transform.Find("Y3tag").GetComponent<Text>();
                if (Y3tag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                Text Y3xtag = Y3tag.transform.Find("Y3xtag").GetComponent<Text>();
                if (Y3xtag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                TMPro.TMP_InputField Y3x = Y3xtag.transform.Find("Y3x").GetComponent<TMPro.TMP_InputField>();
                if (Y3x == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                //----------------------------改---------------------------//
                Y3x.text = ".";//dr.Index(step).YellowRobot[3].pos.x.ToString()

                Text Y3ytag = Y3tag.transform.Find("Y3ytag").GetComponent<Text>(); ;
                if (Y3ytag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y3ytag Object.");
                }

                TMPro.TMP_InputField Y3y = Y3ytag.transform.Find("Y3y").GetComponent<TMPro.TMP_InputField>();
                if (Y3y == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y3y Object.");
                }

                //----------------------------改---------------------------//
                Y3y.text = ".";//dr.Index(step).YellowRobot[3].pos.y.ToString()

                Text Y4tag = image.transform.Find("Y4tag").GetComponent<Text>();
                if (Y4tag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                Text Y4xtag = Y4tag.transform.Find("Y4xtag").GetComponent<Text>();
                if (Y4xtag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                TMPro.TMP_InputField Y4x = Y4xtag.transform.Find("Y4x").GetComponent<TMPro.TMP_InputField>();
                if (Y4x == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    //Debug.Log("Y0_vl Object.");
                }

                //----------------------------改---------------------------//
                Y4x.text = ".";//dr.Index(step).YellowRobot[4].pos.x.ToString()

                Text Y4ytag = Y4tag.transform.Find("Y4ytag").GetComponent<Text>(); ;
                if (Y4ytag == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y4ytag Object.");
                }

                TMPro.TMP_InputField Y4y = Y4ytag.transform.Find("Y4y").GetComponent<TMPro.TMP_InputField>();
                if (Y4y == null)
                {
                    //Debug.Log("No Y0_vl Object.");
                }
                else
                {
                    Debug.Log("Y4y Object.");
                }

                //----------------------------改---------------------------//
                Y4y.text = ".";//dr.Index(step).YellowRobot[4].pos.y.ToString()

                Text Ballxtag = image.transform.Find("Ballxtag").GetComponent<Text>();
                if (Ballxtag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField Ballx = Ballxtag.transform.Find("Ballx").GetComponent<TMPro.TMP_InputField>();
                if (Ballx == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                Ballx.text = ".";//dr.Index(step).CurrentBall.pos.x.ToString();

                Text Ballytag = image.transform.Find("Ballytag").GetComponent<Text>();
                if (Ballytag == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                TMPro.TMP_InputField Bally = Ballytag.transform.Find("Bally").GetComponent<TMPro.TMP_InputField>();
                if (Bally == null)
                {
                    //Debug.Log("No B0_vl Object.");
                }
                else
                {
                    //Debug.Log("B0_vl Object.");
                }

                //----------------------------改---------------------------//
                Bally.text = ".";//dr.Index(step).CurrentBall.pos.y.ToString();

                //Debug.Log("B0_vlopen");
                Side_Appear = true;
            }
        });
    }

    void Info_update()
    {
        GameObject obj = GameObject.Find("mainslider");
        if (obj == null)
        {
            Debug.Log("No Slider Object.");
        }
        else
        {
            Debug.Log("Slider Object.");
        }
        Slider mainslider = (Slider)obj.GetComponent<Slider>();

        // 设置进度条开始拍数和结束拍数
        

        GameObject obj1 = GameObject.Find("num");
        if (obj1 == null)
        {
            Debug.Log("No num Object.");
        }
        else
        {
            Debug.Log("num Object.");
        }
        Text num = (Text)obj1.GetComponent<Text>();
        num.text = mainslider.value.ToString() + "/" + mainslider.maxValue.ToString();

        int step = (int)mainslider.value;
        //Debug.Log("step" + step);

        GameObject obj2 = GameObject.Find("B0_vl");
        if (obj1 == null)
        {
            Debug.Log("No B0_vl Object.");
        }
        else
        {
            Debug.Log("B0_vl Object.");
        }
        Text B0_vl = (Text)obj2.GetComponent<Text>();

        Debug.Log("++");

        //B0_vl.text = dr.Index(step).BlueRobot[0].pos.x.ToString();
        B0_vl.text = "0";
        // MatchInfo
        Event.Send(Event.EventType1.ReplayInfoUpdate,dr.Index(step));
    }
}

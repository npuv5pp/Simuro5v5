/************************************************************************
 * GUI_MainMenu
 * 主菜单：包括开始游戏、设置、退出
************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUI_MainMenu : MonoBehaviour {
    Button GameBtn;
    Button SettingsBtn;
    Button ExitBtn;

	void Start ()
    {
        Screen.fullScreen = false;

        Simuro5v5.Config.ConfigManager.ReadConfigFile("config.json");

        GameBtn = GameObject.Find("game").GetComponent<Button>();
        SettingsBtn = GameObject.Find("settings").GetComponent<Button>();
        ExitBtn = GameObject.Find("exit").GetComponent<Button>();

        GameBtn.onClick.AddListener(delegate ()
        {
            PlayerPrefs.SetInt("step_start", 0);
            SceneManager.LoadScene("GameScene_Play");
        });
        SettingsBtn.onClick.AddListener(delegate ()
        {
            SceneManager.LoadScene("SettingsScene");
        });
        ExitBtn.onClick.AddListener(delegate ()
        {
            Application.Quit();
        });
	}
	
	void Update ()
    {
    }

    //void Call_button_settings()
    //{
    //    GameObject obj = GameObject.Find("settings");
    //    if (obj == null)
    //    {
    //        Debug.Log("No Settings Object.");
    //    }
    //    else
    //    {
    //        Debug.Log("Settings Object.");
    //    }
    //    Button settings = (Button)obj.GetComponent<Button>();
    //    settings.onClick.AddListener(delegate
    //    {
    //        SceneManager.LoadScene("GameScene_Settings");
    //    });
    //}

    //void Call_button_game()
    //{
    //    GameObject obj = GameObject.Find("game");
    //    if (obj == null)
    //    {
    //        Debug.Log("No Game Object.");
    //    }
    //    else
    //    {
    //        Debug.Log("Game Object.");
    //    }
    //    Button game = (Button)obj.GetComponent<Button>();
    //    game.onClick.AddListener(delegate
    //    {
    //        // 初始化回放开始拍数
    //        PlayerPrefs.SetInt("step_start", 0);
    //        SceneManager.LoadScene("GameScene_Play");
    //    });
    //}

    //void Call_button_exit()
    //{
    //    GameObject obj = GameObject.Find("exit");
    //    if (obj == null)
    //    {
    //        Debug.Log("No Exit Object.");
    //    }
    //    else
    //    {
    //        Debug.Log("Exit Object.");
    //    }
    //    Button exit = (Button)obj.GetComponent<Button>();
    //    exit.onClick.AddListener(delegate
    //    {
    //        Application.Quit();
    //    });
    //}
}

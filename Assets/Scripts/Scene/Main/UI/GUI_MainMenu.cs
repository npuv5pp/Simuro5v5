using Simuro5v5;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUI_MainMenu : MonoBehaviour {

	void Start ()
    {
        Screen.fullScreen = false;
        Configuration.ReadFromFileOrCreate("config.json");
        //Simuro5v5.Config.ConfigManager.ReadConfigFile("config.json");
	}

    public void OnGameButtonClicked()
    {
        SceneManager.LoadScene("GameScene_Play");
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}

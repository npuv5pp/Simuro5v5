using Simuro5v5;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GUI_MainMenu : MonoBehaviour
{
    public GameObject aboutPage;
    public TMP_Text cornerText;
    public TMP_Text versionNumber;

    private const string Version = "1.1.1";

	void Start()
    {
        Screen.fullScreen = false;
        Configuration.ReadFromFileOrCreate("config.json");
        aboutPage.SetActive(false);
        cornerText.text = $"Simuro5v5 v{Version}";
        versionNumber.text = Version;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            aboutPage.SetActive(false);
        }
    }

    public void OnGameButtonClicked()
    {
        SceneManager.LoadScene("GameScene_Play");
    }

    public void OnAboutButtonClicked()
    {
        aboutPage.SetActive(true);
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}

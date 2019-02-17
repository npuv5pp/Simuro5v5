/************************************************************************
 * GUI_MainMenu
 * 主菜单：包括开始游戏、设置、退出
************************************************************************/

using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using UnityEngine.SceneManagement;
using TMPro;
using Simuro5v5.Strategy;
using System.Windows.Forms;
using System;

public class GUI_Settings : MonoBehaviour
{
    Button LoadBtn { get; set; }
    Button ExitBtn { get; set; }
    Button BlueLoadBtn { get; set; }
    Button YellowLoadBtn { get; set; }

    string BluePathStr { get; set; }
    string YellowPathStr { get; set; }
    TMP_InputField BlueDllPathField { get; set; }
    TMP_InputField YellowDllPathField { get; set; }

    // Use this for initialization
    void Start()
    {
        LoadBtn = GameObject.Find("/Canvas/LoadBtn").GetComponent<Button>();
        ExitBtn = GameObject.Find("/Canvas/ExitBtn").GetComponent<Button>();
        BlueLoadBtn = GameObject.Find("/Canvas/BOpenDialog").GetComponent<Button>();
        YellowLoadBtn = GameObject.Find("/Canvas/YOpenDialog").GetComponent<Button>();
        BlueDllPathField = GameObject.Find("/Canvas/BInput").GetComponent<TMP_InputField>();
        YellowDllPathField = GameObject.Find("/Canvas/YInput").GetComponent<TMP_InputField>();

        //BlueLoadBtn.onClick.AddListener(delegate ()
        //{
        //    var open = new OpenFileDialog
        //    {
        //        Title = "Blue Strategy Dll",
        //        CheckFileExists = true,
        //        CheckPathExists = true,
        //        Multiselect = false,
        //        Filter = "Strategy Dll (*.dll)|*.dll"
        //    };
        //    if (open.ShowDialog() == DialogResult.OK)
        //    {
        //        Debug.Log(open.FileName);
        //        BluePathStr = open.FileName.Trim();
        //    }
        //    open.Dispose();
        //});
        //YellowLoadBtn.onClick.AddListener(delegate ()
        //{
        //    var open = new OpenFileDialog
        //    {
        //        Title = "Yellow Strategy Dll",
        //        CheckFileExists = true,
        //        CheckPathExists = true,
        //        Multiselect = false,
        //        Filter = "Strategy Dll (*.dll)|*.dll"
        //    };
        //    if (open.ShowDialog() == DialogResult.OK)
        //    {
        //        Debug.Log(open.FileName);
        //        YellowPathStr = open.FileName.Trim();
        //    }
        //    open.Dispose();
        //});
        LoadBtn.onClick.AddListener(Load);
        ExitBtn.onClick.AddListener(Exit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Exit();
        }
    }

    void Load()
    {
        Debug.Log("loading " + BlueDllPathField.text + " " + YellowDllPathField.text);
        LoadInfo.SaveInfo(BlueDllPathField.text.Trim(), YellowDllPathField.text.Trim());
    }

    void Exit()
    {
        SceneManager.LoadScene("MainScene");
    }

    void Call_button_loadstrategy()
    {
        GameObject obj = GameObject.Find("bi");
        if (obj == null)
        {
            Debug.Log("No bi Object.");
        }
        else
        {
            Debug.Log("bi Object.");
        }
        TMPro.TMP_InputField bi = (TMPro.TMP_InputField)obj.GetComponent<TMPro.TMP_InputField>();

        GameObject obj1 = GameObject.Find("yi");
        if (obj1 == null)
        {
            Debug.Log("No yi Object.");
        }
        else
        {
            Debug.Log("yi Object.");
        }
        TMPro.TMP_InputField yi = (TMPro.TMP_InputField)obj1.GetComponent<TMPro.TMP_InputField>();

        GameObject obj2 = GameObject.Find("LoadStrategy");
        if (obj2 == null)
        {
            Debug.Log("No LoadStrategy Object.");
        }
        else
        {
            Debug.Log("LoadStrategy Object.");
        }
        Button LoadStrategy = (Button)obj2.GetComponent<Button>();

        LoadStrategy.onClick.RemoveAllListeners();
        LoadStrategy.onClick.AddListener(delegate
        {
            LoadInfo.SaveInfo(bi.text.ToString(), yi.text.ToString());
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
        exit.onClick.AddListener(delegate
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        });
    }
}

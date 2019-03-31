using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Simuro5v5;

public class Model : MonoBehaviour
{
    Text rowname;
    InputField x, y;

    public string PlayerName { get; set; }
    public Vector2 Data { get; set; }

    void OnEnable()
    {
        rowname = transform.Find("name").GetComponent<Text>();
        x = transform.Find("x").GetComponent<InputField>();
        y = transform.Find("y").GetComponent<InputField>();
    }

    private void Start()
    {
        if (name.ToLower().StartsWith("blue"))
        {
            rowname.color = Color.blue;
        }
        else if (name.ToLower().StartsWith("yellow"))
        {
            rowname.color = Color.yellow;
        }

    }

    private void Update()
    {
        rowname.text = PlayerName;
        x.text = Data.x.ToString();
        y.text = Data.y.ToString();
    }

    public void RenderData(Vector2 data)
    {
        Data = data;
    }

    public void SetName(string name)
    {
        PlayerName = name;
    }
}

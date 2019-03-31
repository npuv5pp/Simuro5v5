using UnityEngine.UI;
using UnityEngine;

public class DataBoardContent : MonoBehaviour
{
    public GameObject template;

    Model[] blues;
    Model[] yellows;

    void Start()
    {
        blues = new Model[5];
        yellows = new Model[5];

        for (int i = 0; i < 5; i++)
        {
            var go = Instantiate(template, transform);
            go.name = $"blue{i}";
            blues[i] = go.GetComponent<Model>();
            blues[i].SetName($"Blue{i}");
        }
        for (int i = 0; i < 5; i++)
        {
            var go = Instantiate(template, transform);
            go.name = $"yellow{i}";
            yellows[i] = go.GetComponent<Model>();
            yellows[i].SetName($"Yellow{i}");
        }
        Destroy(template);

        GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
    }

    void Update()
    {
    }
}

using Simuro5v5;
using UnityEngine;
using TMPro;

public class StateBoard : MonoBehaviour
{
    public TextMeshProUGUI stateText;

    public void Render(DataRecorder.DataType type)
    {
        switch (type)
        {
            case DataRecorder.DataType.AutoPlacement:
                stateText.text = "Auto Placement";
                break;
            case DataRecorder.DataType.NewMatch:
                stateText.text = "New Match";
                break;
            case DataRecorder.DataType.NewRound:
                stateText.text = "New Round";
                break;
            case DataRecorder.DataType.InPlaying:
                stateText.text = "In Playing";
                break;
        }
    }
}

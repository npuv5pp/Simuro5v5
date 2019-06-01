using Simuro5v5;
using UnityEngine;
using TMPro;

public class StateBoard : MonoBehaviour
{
    public TextMeshProUGUI stateText;

    public void Render(MatchPhase mp)
    {
        switch (mp)
        {
            case MatchPhase.FirstHalf:
                {
                    stateText.text = "First Half";
                    break;
                }
            case MatchPhase.SecondHalf:
                {
                    stateText.text = "Second Half";
                    break;
                }
            case MatchPhase.OverTime:
                {
                    stateText.text = "Over Time";
                    break;
                }
            case MatchPhase.Penalty:
                {
                    stateText.text = "Penalty Shootout";
                    break;
                }
        }
    }
}

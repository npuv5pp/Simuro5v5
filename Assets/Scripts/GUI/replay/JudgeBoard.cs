using Simuro5v5;
using UnityEngine;
using TMPro;

public class JudgeBoard : MonoBehaviour
{
    public TextMeshProUGUI judgeText;

    public void Render(JudgeResult result)
    {
        judgeText.text = result.ToRichText();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simuro5v5;

public class PhaseSwitchAnimControl : MonoBehaviour
{
    public TMPro.TMP_Text text;

    Animator animator { get; set; }

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    public void Notify(MatchPhase current)
    {
        string str;
        switch (current)
        {
            case MatchPhase.FirstHalf:
                str = "<b>First Half Start</b>";
                break;
            case MatchPhase.SecondHalf:
                str = "<b>Second Half Start</b>\n<size=70%>Switch Role";
                break;
            case MatchPhase.OverTime:
                str = "<b>Over Time Start</b>";
                break;
            // case MatchPhase.Penalty:
            default:
                str = "<b>Penalty War Start</b>";
                break;
        }
        text.text = str;
        animator.SetBool("show", true);

        IEnumerator func()
        {
            yield return new WaitForSecondsRealtime(1);
            animator.SetBool("show", false);
        }

        StartCoroutine(func());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Notify(MatchPhase.OverTime);
        }
    }
}

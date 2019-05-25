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
                str = "First Half Start";
                break;
            case MatchPhase.SecondHalf:
                str = "Second Half Start";
                break;
            case MatchPhase.OverTime:
                str = "Over Time Start";
                break;
            // case MatchPhase.Penalty:
            default:
                str = "Penalty War Start";
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

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.N))
    //     {
    //         Notify(MatchPhase.FirstHalf);
    //     }
    // }
}

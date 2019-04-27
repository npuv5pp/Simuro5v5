using UnityEngine;

public class AnimControl : MonoBehaviour {
    Animator animator { get; set; }

    void Start () {
        animator = GetComponent<Animator>();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
	}

    public void InGame()
    {
        animator.SetBool("ingame", true);
    }

    public void OutGame()
    {
        animator.SetBool("ingame", false);
    }

    public void Toggle()
    {
        animator.SetBool("ingame", !animator.GetBool("ingame"));
    }
}

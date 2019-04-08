using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour {
    Text Title { get; set; }
    Text Content { get; set; }

    Animator Animator { get; set; }
    bool Shown
    {
        get
        {
            return Animator.GetBool("show");
        }
    }

	void Start () {
        Animator = gameObject.GetComponent<Animator>();
        Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        Title = transform.Find("Title").GetComponent<Text>();
        Content = transform.Find("Content").GetComponent<Text>();
	}

    public void Show(string title, string content, int millseconds)
    {
        if (Shown)
        {
            return;
        }
        StartCoroutine(_show(title, content, millseconds));
    }

    IEnumerator _show(string title, string content, int millseconds)
    {
        Title.text = title;
        Content.text = content;
        Animator.SetBool("show", true);
        yield return new WaitForSecondsRealtime((float)millseconds / 1000);
        //yield return new WaitForSeconds(1f);
        Animator.SetBool("show", false);
    }

    public void Show(string title, string content)
    {
        if (Shown)
        {
            return;
        }
        Title.text = title;
        Content.text = content;
        Animator.SetBool("show", true);
    }

    public void Hide()
    {
        Animator.SetBool("show", false);
    }
}

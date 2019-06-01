using Simuro5v5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefereeTestMain : MonoBehaviour
{
    MouseDrag mouseDrag;
    ObjectManager objectManager;

    MatchInfo matchInfo = new MatchInfo();

    void Start()
    {
        mouseDrag = GetComponent<MouseDrag>();
        objectManager = new ObjectManager();
        objectManager.RebindObject();
        objectManager.RebindMatchInfo(matchInfo);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            mouseDrag.dragEnabled = !mouseDrag.dragEnabled;
            Debug.Log(mouseDrag.dragEnabled);
        }
    }

    public void BluePenalty()
    {
        objectManager.UpdateFromScene();
        // judge

        objectManager.RevertScene(matchInfo);
    }
}

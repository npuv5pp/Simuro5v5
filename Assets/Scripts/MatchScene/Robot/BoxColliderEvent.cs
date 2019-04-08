using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderEvent : MonoBehaviour {

    public ArrayList TouchObject = new ArrayList();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // 碰撞开始
    void OnCollisionEnter(Collision collision)
    {
        TouchObject.Add(collision.gameObject);
    }

    // 碰撞结束
    void OnCollisionExit(Collision collision)
    {
        TouchObject.Remove(collision.gameObject);
    }

    // 碰撞持续中
    void OnCollisionStay(Collision collision)
    {

    }

}

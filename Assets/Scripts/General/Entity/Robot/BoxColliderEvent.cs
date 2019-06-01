using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderEvent : MonoBehaviour {

    public List<GameObject> TouchObject = new List<GameObject>();

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
}

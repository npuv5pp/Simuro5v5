using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereColliderEvent : MonoBehaviour {

    public ArrayList TouchObject = new ArrayList();

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

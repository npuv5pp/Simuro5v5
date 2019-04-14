using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    public Transform target;
    private Vector3 offset = new Vector3(0,30,0);

    // Start is called before the first frame update
    void Start()
    {
        target = transform.Find("Blue0");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        //Rotate();
    }

    //缩放
    private void Scale()
    {

    }
}

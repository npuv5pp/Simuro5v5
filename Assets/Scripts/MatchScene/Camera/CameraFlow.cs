using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    public Transform target;
    private float x;
    private float z;
    private float y;
    private float angle;
    private Vector3 offset = new Vector3(0, 30, 30);

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Blue0").transform;
        angle = 0;
        //transform.rotation = Quaternion.Euler(60,transform.rotation.y,transform.rotation.z);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        Scale();
        transform.LookAt(target);
        Rotate();

    }

    //缩放
    private void Scale()
    {
        float dis = offset.magnitude;
        dis += Input.GetAxis("Mouse ScrollWheel") * 5;
        offset = offset.normalized * dis;
    }

    private void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            angle += Input.GetAxis("Mouse Y");
            y = offset.y;
            x = Mathf.Cos(angle) * y;
            z = Mathf.Sin(angle) * y;
            offset.x = x;
            offset.z = z;
            //transform.RotateAround(target.position, Vector3.up, Input.GetAxis("Mouse X") * 10);
            //transform.RotateAround(target.position, Vector3.up, Input.GetAxis("Mouse Y") * 10);

        }
    }
}

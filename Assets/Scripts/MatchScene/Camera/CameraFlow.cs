using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraState
{
    OverLooking,
    LocateObject,
    FirstPersonForward,
    FirstPersonReverse
}

public class CameraFlow : MonoBehaviour
{
    public CameraState CameraState;
    public Transform target;
    private Vector3 InitialPosition;
    private Quaternion InitialRotation;
    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private float angle;
    private Vector3 offset = new Vector3(0, 50, 50);

    // Start is called before the first frame update
    void Start()
    {
        CameraState = CameraState.OverLooking;
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        angle = 0;
        cam = GetComponent<Camera>();
        //transform.rotation = Quaternion.Euler(60,transform.rotation.y,transform.rotation.z);
    }

    // Update is called once per frame
    void Update()
    {
        switch (CameraState)
        {
            case CameraState.OverLooking:
                OverLooking();
                break;

            case CameraState.LocateObject:
                transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, 0.3f);
                Scale();
                Rotate();
                transform.LookAt(target);
                break;

            case CameraState.FirstPersonForward:
                FirstPersonForward();
                break;

            case CameraState.FirstPersonReverse:
                FirstPersonReverse();
                break;


        }

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && FindHostByMouse(hit))
            {
                target = hit.transform.transform;
                CameraState = CameraState.LocateObject;

            }

        }

        if(FindHostByKey())
        {
            if(CameraState == CameraState.OverLooking)
            {
                CameraState = CameraState.LocateObject;
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            CameraState = CameraState.OverLooking;
        }

        if (Input.GetKeyDown(KeyCode.K) && (CameraState == CameraState.FirstPersonReverse || CameraState == CameraState.LocateObject || CameraState == CameraState.FirstPersonForward))
        {
            if (CameraState == CameraState.FirstPersonForward)
            {
                CameraState = CameraState.LocateObject;
            }
            else
            {
                CameraState = CameraState.FirstPersonForward;
            }
        }

        if (Input.GetKeyDown(KeyCode.L) && (CameraState == CameraState.FirstPersonForward || CameraState == CameraState.LocateObject || CameraState == CameraState.FirstPersonReverse))
        {
            if (CameraState == CameraState.FirstPersonReverse)
            {
                CameraState = CameraState.LocateObject;
            }
            else
            {
                CameraState = CameraState.FirstPersonReverse;
            }
        }

    }

    private bool FindHostByKey()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            target = GameObject.Find("Blue0").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            target = GameObject.Find("Blue1").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            target = GameObject.Find("Blue2").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            target = GameObject.Find("Blue3").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            target = GameObject.Find("Blue4").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            target = GameObject.Find("Yellow0").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            target = GameObject.Find("Yellow1").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            target = GameObject.Find("Yellow2").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            target = GameObject.Find("Yellow3").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            target = GameObject.Find("Yellow4").transform;
            return true;
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            target = GameObject.Find("Ball").transform;
            return true;
        }
        return false;

    }

    public void OverLooking()
    {
        transform.position = InitialPosition;
        transform.rotation = InitialRotation;
    }

    public void MousePickTarget()
    {

        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, 0.3f);

    }

    private bool FindHostByMouse(RaycastHit hit)
    {
        if (hit.transform.tag == "Ball" || hit.transform.tag == "Robot")
        {
            return true;
        }
        return false;

    }

    public void FirstPersonForward()
    {
        transform.position = target.position;
        var rot = new Quaternion();
        rot.eulerAngles = new Vector3
        {
            x = target.rotation.eulerAngles.x + 90,
            y = target.rotation.eulerAngles.y,
            z = target.rotation.eulerAngles.z,
        };
        transform.rotation = rot;
    }

    public void FirstPersonReverse()
    {
        transform.position = target.position;
        var rot = new Quaternion();
        rot.eulerAngles = new Vector3
        {
            x = target.rotation.eulerAngles.x - 90,
            y = target.rotation.eulerAngles.y,
            z = target.rotation.eulerAngles.z + 180,
        };
        transform.rotation = rot;
    }

    //缩放
    private void Scale()
    {

        float dis = offset.magnitude;
        dis += Input.GetAxis("Mouse ScrollWheel") * -10;
        offset = offset.normalized * dis;

    }

    private void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            angle += Input.GetAxis("Mouse X") * 0.8f;
            float y = offset.y;
            float x = Mathf.Cos(angle) * y;
            float z = Mathf.Sin(angle) * y;
            offset.x = x;
            offset.z = z;
            transform.position = target.position + offset;

        }
    }
}

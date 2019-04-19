using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraState
{
    OverLooking,
    LocateObject
}

public class CameraFlow : MonoBehaviour
{
    public CameraState CameraState;
    public Transform target;
    private Vector3 InitialPosition;
    private Quaternion InitialRotation;
    private Vector3 velocity = Vector3.zero;
    private Camera camera;
    private float angle;
    private Vector3 offset = new Vector3(0, 50, 50);

    // Start is called before the first frame update
    void Start()
    {
        CameraState = CameraState.OverLooking;
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        angle = 0;
        camera = GetComponent<Camera>();
        //transform.rotation = Quaternion.Euler(60,transform.rotation.y,transform.rotation.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(CameraState == CameraState.OverLooking)
        {
            MousePickTarget();
        }

        if (CameraState == CameraState.LocateObject)
        {
            MousePickTarget();
            Scale();
            Rotate();
            transform.LookAt(target);
            WaitOverLooking();

        }
    }

    private void WaitOverLooking()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
            transform.position = InitialPosition;
            transform.position = Vector3.SmoothDamp(transform.position, InitialPosition, ref velocity, 0.3f);

            transform.rotation = InitialRotation;
            CameraState = CameraState.OverLooking;

        }
    }

    private void MousePickTarget()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && FindHost(hit))
            {
                target = hit.transform.transform;
                CameraState = CameraState.LocateObject;
                transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, 0.3f);
            }

        }
    }

    private bool FindHost(RaycastHit hit)
    {
        if (hit.transform.tag == "Ball" || hit.transform.tag == "Robot")
        {
            return true;
        }
        return false;

    }
    //缩放
    private void Scale()
    {
        float dis = offset.magnitude;
        dis += Input.GetAxis("Mouse ScrollWheel") * 10;
        offset = offset.normalized * dis;
        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, 0.3f);
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

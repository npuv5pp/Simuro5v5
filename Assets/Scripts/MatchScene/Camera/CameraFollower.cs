using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    GameObject obj;
    string str;
    public float distance = 0.0f;
    public float altitude = 0.0f;
    public float horizontal = 0.0f;
    public float vertical = 0.0f;
    Vector3 offset;

    void Start()
    {
        str = "Blue0";
        obj = GameObject.Find(str);
        offset = transform.position - obj.transform.position;
    }

    void Update()
    {
        

        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                if (i >= 1 && i <= 5)
                {
                    str = "Blue" + (i - 1);
                    TrackEntity(str);
                }
                else if (i == 0)
                {
                    str = "Yellow4";
                    TrackEntity(str);
                }
                else
                {
                    str = "Yellow" + (i - 6);
                    TrackEntity(str);
                } 
            }
        }
        if (Input.GetKeyDown("b"))
        {
            str = "Ball";
            TrackEntity(str);
        }

        MoveCamera();
    }

    void TrackEntity(string name)
    {
        if (obj != null)
        {
            obj = GameObject.Find(str);
            print("camera " + name.ToLower());
        }
    }

    void MoveCamera()
    {
        if (obj != null)
        {
            Transform camPos = obj.transform.Find("CamPos");
            
            float fixDis = Input.GetAxis("Distance");
            float fixAlt = Input.GetAxis("Altitude");
            float h = Input.GetAxis("Horizontal2");
            float v = Input.GetAxis("Vertical2");

            distance += fixDis;
            altitude += fixAlt;
            horizontal += h;
            vertical += v;

            offset = camPos.position - obj.transform.position;
            transform.position = obj.transform.position + 
                                 offset +
                                 offset.normalized * distance +
                                 Vector3.up * altitude +
                                 Vector3.right * horizontal +
                                 Vector3.forward * vertical;

            transform.LookAt(obj.transform);
        }
    }
}

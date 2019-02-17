using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public GameObject[] cameras;

    void Start()
    {
        cameras = GameObject.FindGameObjectsWithTag("Cam");
        cameraSwitch(GameObject.Find("MainCamera"));
    }

    void Update()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyUp(i.ToString()))
            {
                //print("camera entity");
                cameraSwitch(GameObject.Find("EntityCamera"));
            }
        }
        if (Input.GetKeyUp("b"))
        {
            //print("camera entity");
            cameraSwitch(GameObject.Find("EntityCamera"));
        }
        if (Input.GetKeyUp("o"))
        {
            print("camera main");
            cameraSwitch(GameObject.Find("MainCamera"));
        }
    }

    // free aspect
    void cameraSwitch(GameObject obj)
    {
        foreach (GameObject cam in cameras)
        {
            Camera theCam = cam.GetComponent<Camera>() as Camera;

            if (cam.name == obj.name)
            {
                theCam.enabled = true;
            }
            else
            {
                theCam.enabled = false;
            }
        }
    }
}

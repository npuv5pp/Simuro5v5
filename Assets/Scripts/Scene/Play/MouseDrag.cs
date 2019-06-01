using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    public Camera MainCamera;
    public bool dragEnabled { get; set; }

    /// <summary>
    /// 鼠标是否正在按下
    /// </summary>

    bool mouseHolding;
    /// <summary>
    /// 所按物体的距离
    /// </summary>
    float distance;

    float scrollFactor = 30;

    /// <summary>
    /// 按下的物体
    /// </summary>
    Transform focusingObj;

    void Update()
    {
        if (!dragEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            mouseHolding = true;

            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) &&
                (hit.transform.tag == "Robot" || hit.transform.tag == "Ball"))
            {
                // Debug.Log($"Hit {hit.transform}");
                distance = hit.distance;
                focusingObj = hit.transform;
            }
        }
        else if (!Input.GetMouseButton(0))
        // 鼠标没有按下
        {
            mouseHolding = false;
            focusingObj = null;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.transform.tag == "Robot")
            {
                Quaternion rot = new Quaternion();
                rot.eulerAngles = new Vector3
                {
                    x = -90,
                    y = hit.transform.rotation.eulerAngles.y + scroll * scrollFactor,
                    z = 0,
                };
                hit.transform.rotation = rot;
            }
        }

        if (mouseHolding && focusingObj != null)
        {
            // focusingObj.position = Input.mousePosition;
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            var p = ray.GetPoint(distance);
            // TODO 修改拖动算法
            focusingObj.position = new Vector3 { x = p.x, y = 0, z = p.z, };
        }
    }
}

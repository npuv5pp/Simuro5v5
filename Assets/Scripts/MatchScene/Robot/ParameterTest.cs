using Simuro5v5;
using UnityEngine;
using System.IO;

public class ParameterTest : MonoBehaviour
{
    public GameObject leftWheel, rightWheel;

    public float ForwardFactor;
    public float TorqueFactor;
    public float Drag;
    public float AngularDrag;
    public float DoubleZeroDrag;

    public float LeftVelocity;
    public float RightVelocity;

    Vector3 forward_force, forward_drag;
    Vector3 forward_left_drag, forward_right_drag;
    Vector3 torque, angular_drag;

    Collider Collider;
    Rigidbody rb;

    FileStream fs;
    StreamWriter writer;

    void Start()
    {
        Collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = float.MaxValue;

        fs = File.Open(@"D:\Data\newplatform\robot.csv", FileMode.OpenOrCreate);
        fs.SetLength(0);
        writer = new StreamWriter(fs);

        writer.WriteLine("v,av,x,y");
    }

    void FixedUpdate()
    {
        // TODO 零减速
        forward_force = -transform.up * (LeftVelocity + RightVelocity) * ForwardFactor;
        if (LeftVelocity == 0 && RightVelocity == 0)
        {
            forward_drag = rb.velocity * -Drag * 5;
        }
        else
        {
            forward_drag = rb.velocity * -Drag;
        }
        rb.AddForce(forward_force + forward_drag);

        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        angular_drag = rb.angularVelocity * -AngularDrag;
        if (LeftVelocity == 0 && RightVelocity == 0)
        {
            angular_drag = rb.angularVelocity * -AngularDrag * 20;
        }
        else
        {
            angular_drag = rb.angularVelocity * -AngularDrag;
        }
        rb.AddTorque(torque + angular_drag);

        OutputData();
    }

    void OutputData()
    {
        Debug.Log($"{rb.angularVelocity.y / 50}");
        writer.WriteLine($"{rb.velocity.z / 50},{rb.angularVelocity.y / 50},{rb.position.x},{rb.position.z}");
    }

    private void OnDestroy()
    {
        fs.Close();
    }
}

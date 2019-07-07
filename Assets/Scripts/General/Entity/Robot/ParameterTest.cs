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

    private int tick;
    private int playTime;

    void FixedUpdate()
    {
        tick++;
        if (tick % 2 == 0)
            return;

//        playTime++;
//        if (playTime < 40)
//        {
//            LeftVelocity = 125;
//            RightVelocity = -125;
//        }
//        else
//        {
//            LeftVelocity = 0;
//            RightVelocity = 0;
//        }

        forward_force = -transform.up * (LeftVelocity + RightVelocity) * ForwardFactor;

        if (LeftVelocity == 0 && RightVelocity == 0)
            forward_drag = rb.velocity * -Drag * DoubleZeroDrag;
        else
            forward_drag = rb.velocity * -Drag;

        rb.AddForce(forward_force + forward_drag);

        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        angular_drag = rb.angularVelocity * -AngularDrag;
        rb.AddTorque(torque + angular_drag);

        OutputData();
    }

    float prevAV = 0;

    void OutputData()
    {
        Debug.Log($"{rb.angularVelocity.y}");
        writer.WriteLine(
            $"{rb.velocity.z / Const.FramePerSecond},{rb.angularVelocity.y / Const.FramePerSecond},{rb.position.x},{rb.position.z}");
    }

    private void OnDestroy()
    {
        fs.Close();
    }

    void InitParameter()
    {
        rb.mass = Const.Robot.Mass;
        rb.drag = rb.angularDrag = 0;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY;
        rb.maxAngularVelocity = Const.Robot.maxAngularVelocity;

        ForwardFactor = Const.Robot.ForwardForceFactor;
        TorqueFactor = Const.Robot.TorqueFactor;
        Drag = Const.Robot.DragFactor;
        AngularDrag = Const.Robot.AngularDragFactor;
        DoubleZeroDrag = Const.Robot.DoubleZeroDragFactor;
    }
}
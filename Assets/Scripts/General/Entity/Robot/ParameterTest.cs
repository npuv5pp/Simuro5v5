using System;
using Simuro5v5;
using UnityEngine;
using System.IO;

public class ParameterTest : MonoBehaviour
{
    public GameObject leftWheel, rightWheel;

    public float ForwardFactor;
    public float TorqueFactor;
    public float ForwardDrag;
    public float SidewayDrag;
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

        OnPauseBtnClick();

        ForwardFactor = 102.89712678726376f;
        ForwardDrag = 79.81736047779975f;

        TorqueFactor = 1156.1817018313f;
        AngularDrag = 3769.775104018879f;
    }

    private int tick;
    private int playTime;

    void FixedUpdate()
    {
        tick++;
        if (tick % 2 == 0)
            return;

        forward_force = -transform.forward * (LeftVelocity + RightVelocity) * ForwardFactor;

//        forward_drag = rb.velocity * -Drag;
        var sidewayV = Vector3.Project(rb.velocity, transform.right);
        if (sidewayV != Vector3.zero)
            rb.AddForce(forward_force + rb.velocity * -ForwardDrag + sidewayV / sidewayV.magnitude * -SidewayDrag);
        else
            rb.AddForce(forward_force + rb.velocity * -ForwardDrag);

//        rb.AddForce(forward_force + forward_drag);

        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        angular_drag = rb.angularVelocity * -AngularDrag;
        rb.AddTorque(torque + angular_drag);

        OutputData();
    }

    private void LateUpdate()
    {
        Debug.DrawRay(transform.position, transform.right * -SidewayDrag);
    }

    float prevA = 0;
    private float prevZ = 0;

    void OutputData()
    {
        var eulerAngles = rb.transform.eulerAngles;
        var av = string.Format("{0:N10}", eulerAngles.y - prevA);
        prevA = eulerAngles.y;

        var z = rb.transform.position.z;
        var v = string.Format("{0:N10}", z - prevZ);
        prevZ = z;

        var str = $"v {v}, av {av}";
        Debug.Log(str);

        writer.WriteLine(str);
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
        ForwardDrag = Const.Robot.DragFactor;
        AngularDrag = Const.Robot.AngularDragFactor;
        DoubleZeroDrag = Const.Robot.DoubleZeroDragFactor;
    }

    public void OnResetBtnClick()
    {
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        rb.transform.position = Vector3.zero;
        rb.transform.rotation = Quaternion.Euler(0, 0, 0);
        prevA = 0;
        prevZ = 0;
    }

    public void OnPauseBtnClick()
    {
        if (Time.timeScale != 0)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }
}
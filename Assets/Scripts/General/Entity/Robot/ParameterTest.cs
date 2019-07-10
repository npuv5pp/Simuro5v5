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
    public float ZeroAngularDrag;
    public float DoubleZeroDrag;

    public float LeftVelocity;
    public float RightVelocity;

    Vector3 forward_force, forward_drag, sideway_drag;
    Vector3 forward_left_drag, forward_right_drag;
    Vector3 torque, angular_drag;

    Collider Collider;
    Rigidbody rb;

    FileStream fs;
    StreamWriter writer;

    private Vector3 prevLeftPosition, prevRightPosition;

    void Start()
    {
        Collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = float.MaxValue;

        fs = File.Open(@"D:\Data\newplatform.csv", FileMode.OpenOrCreate);
        fs.SetLength(0);
        writer = new StreamWriter(fs);
        writer.WriteLine("x,y,r");

        OnPauseBtnClick();

        ForwardFactor = 102.89712678726376f;
        ForwardDrag = 79.81736047779975f;
        SidewayDrag = 950;

        TorqueFactor = 1156.1817018313f;
        AngularDrag = 3769.775104018879f;

        ZeroAngularDrag = 305500;
        DoubleZeroDrag = 760;

        prevLeftPosition = leftWheelPosition;
        prevRightPosition = rightWheelPosition;
        
        maxX = minX = transform.position.x;
        maxY = minY = transform.position.y;
    }

    private int tick;
    private int playTime;

    void FixedUpdate()
    {
        tick++;
        if (tick % 2 == 0)
            return;

        playTime++;
        LeftVelocity = 30;
        RightVelocity = 60;

        var forwardDir = transform.forward;

        // 动力
        forward_force = (LeftVelocity + RightVelocity) * ForwardFactor * forwardDir;
        // 速度方向的空气阻力
        forward_drag = rb.velocity * -ForwardDrag;
        // 切向速度
        var sidewayV = Vector3.Project(rb.velocity, transform.right);
        // 切向阻力
//        sideway_drag = sidewayV.magnitude < 0.1 ? Vector3.zero : sidewayV / sidewayV.magnitude * -SidewayDrag;
        sideway_drag = sidewayV * -SidewayDrag;
        rb.AddForce(forward_force + forward_drag + sideway_drag);

        var leftVelocity = leftWheelPosition - prevLeftPosition;
        var rightVelocity = rightWheelPosition - prevRightPosition;
        prevLeftPosition = leftWheelPosition;
        prevRightPosition = rightWheelPosition;

        if (LeftVelocity == 0 && RightVelocity == 0)
        {
            forward_drag = rb.velocity * -DoubleZeroDrag;
            rb.AddForce(forward_drag);
        }
        else if (LeftVelocity == 0)
        {
            var dot = Vector3.Dot(leftVelocity, forwardDir);
            if (RightVelocity < 0)
            {
                if (dot > 0)
                    rb.AddForceAtPosition(forwardDir * -ZeroAngularDrag, leftWheelPosition);
                else if (dot < 0)
                    rb.AddForceAtPosition(-forwardDir * -ZeroAngularDrag, leftWheelPosition);
            }
        }
        else if (RightVelocity == 0)
        {
            var dot = Vector3.Dot(rightVelocity, forwardDir);
            if (LeftVelocity < 0)
            {
                if (dot > 0)
                    rb.AddForceAtPosition(forwardDir * -ZeroAngularDrag, rightWheelPosition);
                else if (dot < 0)
                    rb.AddForceAtPosition(-forwardDir * -ZeroAngularDrag, rightWheelPosition);
            }
        }

        // 动力扭矩
        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        // 阻力扭矩
        angular_drag = rb.angularVelocity * -AngularDrag;
        rb.AddTorque(torque + angular_drag);
        OutputData();
    }

    private Vector3 rightWheelPosition => transform.position + transform.right * Const.Robot.HRL;
    private Vector3 leftWheelPosition => transform.position - transform.right * Const.Robot.HRL;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            rb.AddForceAtPosition(rb.transform.forward * SidewayDrag, leftWheelPosition);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rb.AddForceAtPosition(rb.transform.forward * SidewayDrag, leftWheelPosition);
        }
    }

    private void LateUpdate()
    {
        Debug.DrawRay(rightWheelPosition, forward_right_drag);
        Debug.DrawRay(leftWheelPosition, forward_left_drag);
    }

    float prevA = 0;
    private float prevZ = 0;
    float maxX, maxY, minX, minY;

    void OutputData()
    {
        var eulerAngles = rb.transform.eulerAngles;
        var av = string.Format("{0:N10}", eulerAngles.y - prevA);
        prevA = eulerAngles.y;

        var z = rb.transform.position.z;
        var v = string.Format("{0:N10}", z - prevZ);
        prevZ = z;

        var str = $"v {v}, av {av}";
//        Debug.Log(str);

        if (rb.transform.position.x > maxX)
            maxX = rb.transform.position.x;
        if (rb.transform.position.z > maxY)
            maxY = rb.transform.position.z;
        if (rb.transform.position.x < minX)
            minX = rb.transform.position.x;
        if (rb.transform.position.z < minY)
            minY = rb.transform.position.z;
        Debug.Log($"{(maxX - minX + maxY - minY) / 2}");

        str = $"{rb.position.x},{rb.position.z},{rb.rotation.eulerAngles.y}";
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
        playTime = 0;
        maxX = minX = transform.position.x;
        maxY = minY = transform.position.z;
        Time.timeScale = 0;
    }

    public void OnPauseBtnClick()
    {
        if (Time.timeScale != 0)
            Time.timeScale = 0;
        else
            Time.timeScale = Const.DefaultTimeScale;
    }
}
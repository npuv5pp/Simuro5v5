using System;
using UnityEngine;
using Simuro5v5;

public class ControlRobot : MonoBehaviour
{
    public float ForwardFactor;
    public float TorqueFactor;
    public float Drag;
    public float SidewayDrag;
    public float AngularDrag;
    public float DoubleZeroDrag;
    public float DoubleZeroAngularDrag;

    public bool Debugging;

    bool _physicsEnabled;

    public bool physicsEnabled
    {
        get { return _physicsEnabled; }
        set
        {
            if (value)
            {
                EnableRigidBodyAndCollider();
            }
            else
            {
                DisableRigidBodyAndCollider();
            }

            _physicsEnabled = value;
        }
    }

    private float LeftVelocity;
    private float RightVelocity;

    Vector3 forward_force, forward_drag;
    Vector3 torque, angular_drag;

    Collider Collider;
    Rigidbody rb;

    void Start()
    {
        _physicsEnabled = true;
        Collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        InitParameter();
    }

    private int tick;

    void FixedUpdate()
    {
        if (!physicsEnabled)
        {
            return;
        }

        tick++;
        if (tick % 2 == 0)
            return;

        forward_force = -transform.up * (LeftVelocity + RightVelocity) * ForwardFactor;

        forward_drag = rb.velocity * -Drag;
        var sidewayV = Vector3.Project(rb.velocity, transform.right);
        if (sidewayV != Vector3.zero)
            rb.AddForce(forward_force + forward_drag + sidewayV / sidewayV.magnitude * -SidewayDrag);
        else
            rb.AddForce(forward_force + forward_drag);

        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        angular_drag = rb.angularVelocity * -AngularDrag;
        rb.AddTorque(torque + angular_drag);
    }

    public void SetWheelVelocity(Wheel ws)
    {
        SetWheelVelocity(ws.left, ws.right);
    }

    public void SetWheelVelocity(float left, float right)
    {
        LeftVelocity = left;
        RightVelocity = right;
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="robot"></param>
    public void SetPlacement(Robot robot)
    {
        // Note: 设置刚体的坐标，会在下一拍才会显示到屏幕上，应该直接设置物体的
        Vector3 pos = new Vector3
        {
            x = robot.pos.x,
            z = robot.pos.y,
            y = 0
        };
        transform.position = pos;
        transform.rotation = Quaternion.Euler(-90, robot.rotation.FormatOld().FormatOld2Unity(), 0);
    }

    /// <summary>
    /// 设置速度和位置
    /// </summary>
    /// <param name="robot"></param>
    public void Revert(Robot robot)
    {
        // 设置位置信息
        SetPlacement(robot);
        SetWheelVelocity(robot.wheel.left, robot.wheel.right);
        if (physicsEnabled)
        {
            // 设置刚体的线速度和角速度
            rb.velocity = robot.GetLinearVelocityVector3();
            rb.angularVelocity = robot.GetAngularVelocityVector3();
        }
    }

    /// <summary>
    /// 使静止
    /// </summary>
    public void SetStill()
    {
        SetWheelVelocity(0, 0);
        if (physicsEnabled)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.Sleep();
            rb.WakeUp();
        }
    }

    /// <summary>
    /// 禁用碰撞体，移除刚体
    /// </summary>
    void DisableRigidBodyAndCollider()
    {
        // Collider.enabled = false;
        DestroyImmediate(rb);
    }

    /// <summary>
    /// 启用碰撞体，添加刚体
    /// </summary>
    void EnableRigidBodyAndCollider()
    {
        // Collider.enabled = true;
        rb = gameObject.AddComponent<Rigidbody>();
        InitParameter();
    }

    /// <summary>
    /// 设置参数
    /// </summary>
    void InitParameter()
    {
        if (!physicsEnabled)
        {
            throw new PhysicsDisabledException();
        }

        rb.mass = Const.Robot.Mass;
        rb.drag = rb.angularDrag = 0;
        rb.useGravity = false;
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
        SidewayDrag = Const.Robot.SidewayDragFactor;
        AngularDrag = Const.Robot.AngularDragFactor;
        DoubleZeroDrag = Const.Robot.DoubleZeroDragFactor;
        DoubleZeroAngularDrag = Const.Robot.DoubleZeroAngularDragFactor;
    }
}
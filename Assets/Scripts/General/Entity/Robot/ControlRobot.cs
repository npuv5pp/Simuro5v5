using System;
using UnityEngine;
using Simuro5v5;

public class ControlRobot : MonoBehaviour
{
    public float ForwardFactor;
    public float TorqueFactor;
    public float ForwardDrag;
    public float SidewayDrag;
    public float AngularDrag;
    public float DoubleZeroDrag;
    public float ZeroAngularDrag;

    public bool Debugging;

    bool _physicsEnabled;

    public bool physicsEnabled
    {
        get => _physicsEnabled;
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

    Vector3 forward_force, forward_drag, sideway_drag;
    Vector3 forward_left_drag, forward_right_drag;
    Vector3 torque, angular_drag;
    Vector3 prevLeftPosition, prevRightPosition;

    Collider Collider;
    Rigidbody rb;

    private Vector3 DefaultRotation;

    void Start()
    {
        _physicsEnabled = true;
        Collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        InitParameter();
        DefaultRotation = transform.eulerAngles;
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

        // 前向
        var forwardDir = -transform.up;
        // 右边
        var rightDir = transform.right;
        // 线速度
        var velocity = rb.velocity;
        // 角速度
        var angularVelocity = new Vector3 {y = rb.angularVelocity.y};
        // 左轮中点速度
        var leftPointVelocity = leftWheelPosition - prevLeftPosition;
        // 右轮中点速度
        var rightPointVelocity = rightWheelPosition - prevRightPosition;

        prevLeftPosition = leftWheelPosition;
        prevRightPosition = rightWheelPosition;

        if (LeftVelocity == 0 && RightVelocity == 0)
        {
            // 双零直线减速
            forward_drag = velocity * -DoubleZeroDrag;
            rb.AddForce(forward_drag);
        }
        else if (LeftVelocity == 0)
        {
            // 左轮为0减速，
            var dot = Vector3.Dot(leftPointVelocity, forwardDir);
            if (Math.Abs(dot) > 0.001 && RightVelocity < 0)
            {
                var leftV = Vector3.Project(leftPointVelocity, forwardDir);
                if (dot > 0)
                {
                    forward_drag = velocity * -DoubleZeroDrag;
                    rb.AddForce(forward_drag);
                    // 左手系
                    rb.AddTorque(leftV.magnitude * ZeroAngularDrag * Vector3.down, ForceMode.Impulse);
                }
                else if (dot < 0)
                {
                    forward_drag = velocity * -DoubleZeroDrag;
                    rb.AddForce(forward_drag);
                    rb.AddTorque(leftV.magnitude * ZeroAngularDrag * Vector3.up, ForceMode.Impulse);
                }
            }
        }
        else if (RightVelocity == 0)
        {
            var dot = Vector3.Dot(rightPointVelocity, forwardDir);
            if (Math.Abs(dot) > 0.001 && LeftVelocity < 0)
            {
                var rightV = Vector3.Project(rightPointVelocity, forwardDir);
                if (dot > 0)
                {
                    forward_drag = velocity * -DoubleZeroDrag;
                    rb.AddForce(forward_drag);
                    rb.AddTorque(rightV.magnitude * ZeroAngularDrag * Vector3.up, ForceMode.Impulse);
                }
                else if (dot < 0)
                {
                    forward_drag = velocity * -DoubleZeroDrag;
                    rb.AddForce(forward_drag);
                    rb.AddTorque(rightV.magnitude * ZeroAngularDrag * Vector3.down, ForceMode.Impulse);
                }
            }
        }
        else
        {
            // 切向速度
            var sidewayV = Vector3.Project(velocity, rightDir);
            // 切向阻力
//        sideway_drag = sidewayV.magnitude < 0.1 ? Vector3.zero : sidewayV / sidewayV.magnitude * -30000;
            sideway_drag = sidewayV * -SidewayDrag;
            rb.AddForce(sideway_drag);
        }

        // 动力
        forward_force = (LeftVelocity + RightVelocity) * ForwardFactor * forwardDir;
        // 速度方向的空气阻力
        forward_drag = velocity * -ForwardDrag;
        rb.AddForce(forward_force + forward_drag);

        // 动力扭矩
        torque = (LeftVelocity - RightVelocity) * TorqueFactor * Vector3.up;
        // 阻力扭矩
        angular_drag = angularVelocity * -AngularDrag;
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

    private void LateUpdate()
    {
//        if (transform.eulerAngles.x != DefaultRotation.x)
//            Debug.LogError($"Error rotation x {transform.eulerAngles.x}, except {DefaultRotation.x}", this.gameObject);
//        if (transform.eulerAngles.z != DefaultRotation.z)
//            Debug.LogError($"Error rotation z {transform.eulerAngles.z}, except {DefaultRotation.z}", this.gameObject);
        var rotation = DefaultRotation;
        rotation.y = transform.eulerAngles.y;
        transform.eulerAngles = rotation;
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

        ForwardFactor = Configuration.ParameterConfig.ForwardForceFactor;
        TorqueFactor = Configuration.ParameterConfig.TorqueFactor;
        ForwardDrag = Configuration.ParameterConfig.DragFactor;
        SidewayDrag = Configuration.ParameterConfig.SidewayDragFactor;

        AngularDrag = Configuration.ParameterConfig.AngularDragFactor;
        DoubleZeroDrag = Configuration.ParameterConfig.DoubleZeroDragFactor;
        ZeroAngularDrag = Configuration.ParameterConfig.ZeroAngularDragFactor;
    }

    private Vector3 rightWheelPosition => transform.position + transform.right * Const.Robot.HRL;
    private Vector3 leftWheelPosition => transform.position - transform.right * Const.Robot.HRL;

    void MyDebug(string str)
    {
#if DEBUG
        if (Debugging)
        {
            Debug.Log($"[{gameObject.name}] {str}");
        }
#endif
    }

    void MyDebugWarning(string str)
    {
#if DEBUG
        if (Debugging)
        {
            Debug.LogWarning($"[{gameObject.name}] {str}");
        }
#endif
    }

    void MyDebugError(string str)
    {
#if DEBUG
        if (Debugging)
        {
            Debug.LogError($"[{gameObject.name}] {str}");
        }
#endif
    }
}
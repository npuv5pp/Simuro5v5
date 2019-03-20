using UnityEngine;
using Simuro5v5;

public class ControlRobot : MonoBehaviour
{
    public float ForwardFactor;
    public float TorqueFactor;
    public float Drag;
    public float AngularDrag;

    public bool Debugging;

    float LastLeftVelocity, LastRightVelocity;
    float LeftVelocity { get; set; }
    float RightVelocity { get; set; }

    Vector3 forward_force, forward_drag;
    Vector3 torque, angular_drag;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = (Const.Robot.maxAngularVelocity * Mathf.Deg2Rad);

        InitRobotParameter();
    }

    void FixedUpdate()
    {
        forward_force = -transform.up * (LeftVelocity + RightVelocity) * ForwardFactor;
        forward_drag = rb.velocity * -Drag;
        rb.AddForce(forward_force + forward_drag);
        //rb.AddRelativeForce(forward_force + forward_drag);

        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        angular_drag = rb.angularVelocity * -AngularDrag;
        rb.AddTorque(torque + angular_drag);
        //rb.AddRelativeTorque(torque + angular_drag);
    }

    private void LateUpdate()
    {
        if (Debugging)
        {
            Debug.Log($"{transform.up}, {transform.forward}");
        }
        Debug.DrawRay(transform.position, transform.forward * 10, Color.red);
        Debug.DrawRay(transform.position, transform.up * 10, Color.black);
    }

    public void SetWheelVelocity(Wheel ws)
    {
        SetWheelVelocity((float)ws.left, (float)ws.right);
    }

    public void SetWheelVelocity(float left, float right)
    {
        LastLeftVelocity = LeftVelocity;
        LastRightVelocity = RightVelocity;
        LeftVelocity = left;
        RightVelocity = right;
    }

    /// <summary>
    /// 设置位置，速度设为0
    /// </summary>
    /// <param name="robot"></param>
    public void SetPlacement(Robot robot)
    {
        // Note: 设置刚体的坐标，会在下一拍才会显示到屏幕上，应该直接设置物体的
        Quaternion rot = new Quaternion();
        Vector3 pos = new Vector3
        {
            x = robot.pos.x,
            z = robot.pos.y,
            y = 0
        };
        transform.position = pos;
        rot.eulerAngles = new Vector3
        {
            x = -90,
            y = ((float)robot.rotation).FormatOld().FormatOld2Unity(),
            z = 0,
        };
        transform.rotation = rot;
        SetStill();
    }

    public void SetStill()
    {
        SetWheelVelocity(0, 0);
        rb.Sleep();
        rb.WakeUp();
    }

    /// <summary>
    /// 设置速度和位置，完全还原状态
    /// </summary>
    /// <param name="robot"></param>
    public void Revert(Robot robot)
    {
        // 设置位置信息，并静止
        SetPlacement(robot);

        // 设置刚体的线速度和角速度
        rb.velocity = robot.GetLinearVelocityVector3();
        rb.angularVelocity = robot.GetAngularVelocityVector3();

        SetWheelVelocity((float)robot.velocityLeft, (float)robot.velocityRight);
    }

    void InitRobotParameter()
    {
        rb.mass = Const.Robot.Mass;
        rb.drag = rb.angularDrag = 0;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY;

        ForwardFactor = Const.Robot.ForwardForceFactor;
        TorqueFactor = Const.Robot.TorqueFactor;
        Drag = Const.Robot.DragFactor;
        AngularDrag = Const.Robot.AngularDragFactor;
        rb.maxAngularVelocity = Const.Robot.maxAngularVelocity;
    }
}

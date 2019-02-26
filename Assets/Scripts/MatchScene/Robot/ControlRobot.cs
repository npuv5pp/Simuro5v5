using UnityEngine;
using Simuro5v5;
using System;

public class ControlRobot : MonoBehaviour
{
    public bool Debugging;
    public string role = "";

    RobotParameter RobotParameter;
    WheelCollider WheelParameter;

    Rigidbody rb;
    WheelCollider leftWheel;
    WheelCollider rightWheel;

    // 动力属性
    float velocityLeft;         // 左轮速，标量，因为方向和rotation一致
    float velocityRight;        // 右轮速，标量，因为方向和rotation一致
    const float wheelMultiplier = 197000 / 125;

    void Start()
    {
        Time.fixedDeltaTime = Const.Zeit;

        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = (Const.Robot.maxAngularVelocity * Mathf.Deg2Rad);

        leftWheel = transform.Find("WheelL").GetComponent<WheelCollider>();
        rightWheel = transform.Find("WheelR").GetComponent<WheelCollider>();

        var p = GameObject.Find("Parameter");
        RobotParameter = p.GetComponent<RobotParameter>();
        WheelParameter = p.GetComponent<WheelCollider>();

        InitRobotParameter();
        InitWheelParameter();
    }

    void FixedUpdate()
    {
        if (Debugging)
        {
            Debug.Log(string.Format(
                "{4} motor: {5} {6}; brake: {7}, {8}; rpm: {0}, {1}; v: {2}, {3}",
                leftWheel.rpm, rightWheel.rpm, rb.velocity, rb.angularVelocity, role,
                leftWheel.motorTorque, rightWheel.motorTorque, leftWheel.brakeTorque, rightWheel.brakeTorque));
        }
    }

    float GetMotor(float power)
    {
        //float motor = 0;
        //float velocity = rb.velocity.magnitude;
        //float k = 1.0f / 600.0f;
        //float offset = 2.0f;

        //motor = (power - velocity) / ((velocity + offset) * k);
        //Debug.Log("PlayTime=" + PlayTime + " power=" + power + " velocity=" + velocity + " motor=" + motor + " name=" + transform.name);

        //Debug.LogWarning(motor);
        //return motor;
        return power * wheelMultiplier;
    }

    public void SetWheelVelocity(Wheel ws)
    {
        leftWheel.motorTorque = GetMotor((float)ws.left);
        rightWheel.motorTorque = GetMotor((float)ws.right);
        leftWheel.brakeTorque = ws.left == 0 ? Const.Wheel.brakeTorque : 0;
        rightWheel.brakeTorque = ws.right == 0 ? Const.Wheel.brakeTorque : 0;
    }

    /// <summary>
    /// 设置位置，不包括速度
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
        };
        transform.position = pos;
        rot.eulerAngles = new Vector3
        {
            x = 0,
            y = ((float)robot.rotation).FormatOld().FormatOld2Unity(),
            z = 0,
        };
        transform.rotation = rot;
    }

    public void SetStill()
    {
        velocityLeft = velocityRight = 0;
        rb.Sleep();
        rb.WakeUp();
    }

    /// <summary>
    /// 设置速度和位置，完全还原状态
    /// </summary>
    /// <param name="robot"></param>
    public void Revert(Robot robot)
    {
        // 设置位置信息
        SetPlacement(robot);

        if (Debugging)
        {
            int _i = 0;
            _i++;
        }

        // 设置刚体的线速度和角速度
        rb.velocity = robot.GetLinearVelocityVector3();
        rb.angularVelocity = robot.GetAngularVelocityVector3();

        leftWheel.motorTorque = GetMotor((float)robot.velocityRight);
        rightWheel.motorTorque = GetMotor((float)robot.velocityRight);

        leftWheel.brakeTorque = robot.velocityLeft == 0 ? Const.Wheel.brakeTorque : 0;
        rightWheel.brakeTorque = robot.velocityRight == 0 ? Const.Wheel.brakeTorque : 0;
    }

    void InitRobotParameter()
    {
        rb.mass = RobotParameter.RobotMass;
        rb.drag = RobotParameter.RobotDrag;
        rb.angularDrag = RobotParameter.RobotAngularDrag;
    }

    void InitWheelParameter()
    {
        leftWheel.mass = WheelParameter.mass;
        leftWheel.radius = WheelParameter.radius;
        leftWheel.wheelDampingRate = WheelParameter.wheelDampingRate;
        leftWheel.suspensionDistance = WheelParameter.suspensionDistance;
        leftWheel.suspensionSpring = WheelParameter.suspensionSpring;
        leftWheel.center = WheelParameter.center;
        leftWheel.forceAppPointDistance = WheelParameter.forceAppPointDistance;
        leftWheel.forwardFriction = WheelParameter.forwardFriction;
        leftWheel.sidewaysFriction = WheelParameter.sidewaysFriction;

        rightWheel.mass = WheelParameter.mass;
        rightWheel.radius = WheelParameter.radius;
        rightWheel.wheelDampingRate = WheelParameter.wheelDampingRate;
        rightWheel.suspensionDistance = WheelParameter.suspensionDistance;
        rightWheel.suspensionSpring = WheelParameter.suspensionSpring;
        rightWheel.center = WheelParameter.center;
        rightWheel.forceAppPointDistance = WheelParameter.forceAppPointDistance;
        rightWheel.forwardFriction = WheelParameter.forwardFriction;
        rightWheel.sidewaysFriction = WheelParameter.sidewaysFriction;
    }

    // 数据记录
    Vector3 pos0 = new Vector3(102.5f, 0.0f, 70.0f);       // 记录初始位置
    int PlayTime = 0;

    bool IsBlue(int i)
    {
        return transform.name == ("Blue" + i);
    }

    float InceraseVmin(float Vmax, float Vmin)
    {
        //Vmin = Vmin + (Vmax - Vmin) * (Vmax - Vmin) * 0.004f;
        return Vmin;
    }

    void PositionController()
    {
        if (IsBlue(0))
        {
            if (Input.GetKeyUp("q"))
            {
                rb.position = pos0;
            }
            if (rb.position.z <= -70)
            {
                rb.position = pos0;
            }
        }
    }
}

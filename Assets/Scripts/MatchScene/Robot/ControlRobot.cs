using UnityEngine;
using Simuro5v5;

public class ControlRobot : MonoBehaviour
{
    public bool Debugging;

    WheelCollider leftWheel;
    WheelCollider rightWheel;

    // 动力属性
    float velocityLeft;         // 左轮速，标量，因为方向和rotation一致
    float velocityRight;        // 右轮速，标量，因为方向和rotation一致

    Wheel wheel;
    Rigidbody rb;

    // 数据记录
    Vector3 pos0 = new Vector3(102.5f, 0.0f, 70.0f);       // 记录初始位置
    int PlayTime;

    bool IsBlue(int i)
    {
        return transform.name == ("Blue" + i);
    }

    void Start()
    {
        Time.fixedDeltaTime = Const.Zeit;

        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = (Const.Robot.maxAngularVelocity * Mathf.Deg2Rad);

        leftWheel = transform.Find("WheelL").GetComponent<WheelCollider>();
        rightWheel = transform.Find("WheelR").GetComponent<WheelCollider>();

        PlayTime = 0;
    }

    void FixedUpdate()
    {
        leftWheel.motorTorque = GetMotor(velocityLeft);
        rightWheel.motorTorque = GetMotor(velocityRight);

        leftWheel.brakeTorque = velocityLeft == 0 ? Const.Wheel.brakeTorque : 0;
        rightWheel.brakeTorque = velocityRight == 0 ? Const.Wheel.brakeTorque : 0;

        PlayTime++;

        if (Debugging)
        {
            Debug.Log(string.Format(
                "rpm: {0}, {1}; v: {2}, {3}",
                leftWheel.rpm, rightWheel.rpm, rb.velocity, rb.angularVelocity));
        }
    }

    float GetMotor(float power)
    {
        float motor = 0;
        float velocity = rb.velocity.magnitude;
        float k = 1.0f / 600.0f;
        float offset = 2.0f;

        motor = (power - velocity) / ((velocity + offset) * k);
        //Debug.Log("PlayTime=" + PlayTime + " power=" + power + " velocity=" + velocity + " motor=" + motor + " name=" + transform.name);

        return motor;
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

    public void SetWheelVelocity(Wheel ws)
    {
        velocityLeft = (float)ws.left;
        velocityRight = (float)ws.right;

        if (velocityLeft * velocityRight >= 0)
        {
            if (velocityLeft < velocityRight)
            {
                velocityLeft = InceraseVmin(velocityRight, velocityLeft);
            }
            else
            {
                velocityRight = InceraseVmin(velocityLeft, velocityRight);
            }
        }
    }

    /// <summary>
    /// 设置位置，不包括速度
    /// </summary>
    /// <param name="robot"></param>
    public void SetPlacement(Robot robot)
    {

        // Note: 设置刚体的坐标，会在下一拍才会显示到屏幕上，应该直接设置物体的
        Vector3 pos;
        Quaternion rot = new Quaternion();
        pos.x = robot.pos.x;
        pos.z = robot.pos.y;
        pos.y = robot.pos.z;
        transform.position = pos;
        rot.eulerAngles = new Vector3
        {
            x = 0,
            y = ((float)robot.rotation).FormatOld().FormatOld2Unity(),
            z = 0,
        };
        transform.rotation = rot;
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
        rb.velocity = robot.linearVelocity.GetUnityVector3();
        rb.angularVelocity = robot.angularVelocity.GetUnityVector3();

        // 设置轮速。会在之后影响到扭矩。
        velocityLeft = (float)robot.velocityLeft;
        velocityRight = (float)robot.velocityRight;
    }

    public void SetStill()
    {
        wheel.left = 0;
        wheel.right = 0;
        rb.Sleep();
        rb.WakeUp();
    }

    float InceraseVmin(float Vmax, float Vmin)
    {
        //Vmin = Vmin + (Vmax - Vmin) * (Vmax - Vmin) * 0.004f;

        return Vmin;
    }
}
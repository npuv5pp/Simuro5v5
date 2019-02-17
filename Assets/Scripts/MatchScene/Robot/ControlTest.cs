using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Simuro5v5;

public class ControlTest : MonoBehaviour
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;

    // 动力属性
    float velocityLeft;         // 左轮速，标量，因为方向和rotation一致
    float velocityRight;        // 右轮速，标量，因为方向和rotation一致
    float targV;                // 轮速均值，用于赋力
    float targW;                // 轮速差值，用于赋扭矩，L减R，分正负

    // 导出属性
    float parallelF;  // 平行于前进方向的力，相对rot，含正负
    float verticalF;   // 垂直于前进方向的力，相对rot，含正负
    float torque;      // 扭矩，顺时针正方向

    Wheel wheel;
    Rigidbody rb;
    PhysicMaterial phyMat;
    Vector2D velocity;

    // 规律属性
    float kwDym { get { return 1 / (Const.Robot.k1Dym * rb.angularVelocity.y * Mathf.Rad2Deg + Const.Robot.k2Dym); } }// 一个修正引擎角速度非线性规律的系数
    /*
    f(x) = k1 * x + k2
    Coefficients (with 95% confidence bounds):
    k1 = -0.0007292  (-0.0007307, -0.0007278)
    k2 = 1  (1, 1)
    */
    bool useManualF;    // 手动设置摩擦力，即直接用老平台规律进行计算而不在Unity中设置摩擦力

    // 数据记录
    StreamWriter sw1;
    StreamWriter sw2;
    int count;          // 计拍数
    public Vector3 pos0 = new Vector3(102.5f, 0.0f, 50.0f);       // 记录初始位置
    float x0;           // 上一拍x坐标
    float v0;           // 上一拍速度
    float w0;           // 上一拍角速度
    float rpm;
    float rpm0;
    bool writeAllowed;  // 能否打印数据
    int PlayTime;
    
    bool IsBlue(int i)
    {
        return transform.name == ("Blue" + i);
    }

    void Start()
    {
        Time.fixedDeltaTime = Const.Zeit;
        useManualF = true;

        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = (Const.Robot.maxAngularVelocity * Mathf.Deg2Rad);

        rb.position = pos0;

        PrintDataPre();
        PlayTime = 0;
    }

    void FixedUpdate()
    {
        if (IsBlue(0))
        {
            PrintData();
            UpdateLocalData();// 更新实例化的类中存储的信息
            //ForceController();
            PositionController();

            float power = 100.0f;

            transform.Find("WheelL").GetComponent<WheelCollider>().motorTorque = GetMotor(power);
            transform.Find("WheelR").GetComponent<WheelCollider>().motorTorque = GetMotor(power);
            //transform.Find("WheelL").GetComponent<WheelCollider>().motorTorque = power;
            //transform.Find("WheelR").GetComponent<WheelCollider>().motorTorque = power;

            //Debug.Log("PlayTime=" + PlayTime + "\trpm=" + leftWheel.rpm);
            rpm = leftWheel.rpm;

            if (PlayTime < 40)
            {
                //rb.AddRelativeForce(Vector3.forward * rb.mass * Const.Robot.kv * 1.0f);
            }
            //rb.AddRelativeForce(Vector3.forward * rb.mass * Const.Robot.kv * 1.0f);
            //rb.AddRelativeForce(Vector3.right * rb.mass * Const.Robot.kv * 1.0f);
            //rb.AddRelativeTorque(Vector3.up * rb.mass * Const.Robot.kw * kwDym * 1.0f);

            DebugInfo();
            PlayTime++;
        }
        //VelocityConstrain();// 将线速度限制到合适范围内// 暂时不做限制
        // 角速度在Start()中直接通过maxAngularVelocity属性限制
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

    void UpdateLocalData()
    {
        velocity.x = rb.velocity.x;
        velocity.y = rb.velocity.z;
    }

    void ForceController()
    {
        Mean();
        Difference();

        if (useManualF)
        {
            parallelF = ParallelForceCalculatorManual();
            verticalF = VerticalForceCalculatorManual();
            torque = TorqueCalculatorManual();
        }
        else
        {
            parallelF = ParallelForceCalculatorAuto();
            verticalF = VerticalForceCalculatorAuto();
            torque = TorqueCalculatorAuto();
        }
    }

    void Mean() { targV = (velocityLeft + velocityRight) / 2; }
    void Difference() { targW = velocityLeft - velocityRight; }

    float ParallelForceCalculatorManual()// 别忘了模拟甩球！！
    {
        float vp0, vp1;
        float rot = rb.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
        float theta = (rot - velocity.rotation).FormatOld();// 方向相对于速度，左正右负
        float k;

        if (velocityLeft == 0 && velocityRight == 0)
        {
            k = Const.Robot.kf2;
        }
        else
        {
            k = Const.Robot.kf1;
        }

        vp0 = rb.velocity.magnitude * Mathf.Cos(theta * Mathf.Deg2Rad);
        vp1 = vp0 + (targV - vp0) * (1 - k);// 含正负

        return (vp1 - vp0);
    }

    float VerticalForceCalculatorManual()
    {
        float vv0, vv1;
        float rot = rb.rotation.eulerAngles.y.FormatUnity2Old().FormatOld();
        float theta = (rot - velocity.rotation).FormatOld();// 方向相对于速度，左正右负
        float k = Const.Robot.kf2;

        vv0 = rb.velocity.magnitude * Mathf.Sin(theta * Mathf.Deg2Rad);
        vv1 = vv0 * k;// 含正负

        return (vv1 - vv0);
    }

    float TorqueCalculatorManual()
    {
        float w0, w1;
        float k = Const.Robot.kt1;

        w0 = rb.angularVelocity.y * Mathf.Rad2Deg;
        w1 = w0 + (targW * Const.Robot.r - w0) * (1 - k);// 含正负

        return (w1 - w0);
    }

    float ParallelForceCalculatorAuto()
    {
        float f = 1.0f;
        return f;
    }

    float VerticalForceCalculatorAuto()
    {
        float f = 1.0f;
        return f;
    }

    float TorqueCalculatorAuto()
    {
        float t = 1.0f;
        return t;
    }

    void PositionController()
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

    void VelocityConstrain()
    {
        // 考虑到更新滞后，此部分代码先不实现
        //if (rb.velocity.magnitude > Const.Robot.maxVelocity)
        //{
        //    rb.velocity = FormatVelocity(rb.velocity, Const.Robot.maxVelocity);
        //}
        //else if (Mathf.Abs(rb.velocity.magnitude - Mathf.Round(rb.velocity.magnitude)) < Const.Robot.range)
        //{
        //    rb.velocity = FormatVelocity(rb.velocity, Mathf.Round(rb.velocity.magnitude));
        //}
    }

    void DebugInfo()
    {
        //if (true)
        if (rb.name == "0")
        {
            //Const.DebugLog("id=" + rb.name + "  vl="+ velocityLeft + "  vr=" + velocityRight);
            //Const.DebugLog("Fp=" + parallelF);
            //Const.DebugLog2("Fv=" + verticalF);
            //Const.DebugLog2("Tor=" + torque);
            //Const.DebugLog2("rv=" + rb.velocity.magnitude);
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

    float InceraseVmin(float Vmax, float Vmin)
    {
        Vmin = Vmin + (Vmax - Vmin) * (Vmax - Vmin) * 0.004f;

        return Vmin;
    }

    // 准备函数
    void PrintDataPre()
    {
        sw1 = new StreamWriter(@"Assets\Scripts\MatchScene\Robot\data1.txt", false);
        //sw1.WriteLine("Recording..\n");
        //sw1.WriteLine("mass=" + rb.mass + "\n");
        sw1.Close();

        sw2 = new StreamWriter(@"Assets\Scripts\MatchScene\Robot\data2.txt", false);
        //sw2.WriteLine("Recording..\n");
        //sw2.WriteLine("mass=" + rb.mass + "\n");
        sw2.Close();

        count = 0;
        v0 = 0;
        writeAllowed = true;
    }

    // 每拍调用
    void PrintData()
    {
        float x = rb.position.x;
        float v = rb.velocity.magnitude;
        Vector3 w = rb.angularVelocity;

        sw1 = new StreamWriter(@"Assets\Scripts\MatchScene\Robot\data1.txt", true);
        //if (v < v0) { writeAllowed = false; }
        if (writeAllowed)
        {
            sw1.WriteLine(v + "\t" + (v - v0) + "\n");
            //sw1.WriteLine(rpm + "\t" + (rpm - rpm0) + "\n");
        }
        sw1.Close();

        sw2 = new StreamWriter(@"Assets\Scripts\MatchScene\Robot\data2.txt", true);
        //sw2.WriteLine((w.y * Mathf.Rad2Deg) + "\t" + ((w.y - w0) * Mathf.Rad2Deg) + "\n");
        //sw2.WriteLine(rot + "\n");
        //sw2.WriteLine(((w.y - w0) * Mathf.Rad2Deg) + "\n");
        sw2.Close();

        count++;
        x0 = x;
        v0 = v;
        w0 = w.y;
        rpm0 = rpm;
    }
}


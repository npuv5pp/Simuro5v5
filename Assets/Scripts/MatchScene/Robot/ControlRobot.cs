using UnityEngine;
using Simuro5v5;

public class ControlRobot : MonoBehaviour
{
    public bool Debugging;
    public string role = "";

    RobotParameter RobotParameter;

    Rigidbody rb;
    ControlWheel leftWheelController;
    ControlWheel rightWheelController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = (Const.Robot.maxAngularVelocity * Mathf.Deg2Rad);

        leftWheelController = transform.Find("WheelL").gameObject.GetComponent<ControlWheel>();
        rightWheelController = transform.Find("WheelR").gameObject.GetComponent<ControlWheel>();
        RobotParameter = GameObject.Find("Parameter").GetComponent<RobotParameter>();

        InitRobotParameter();
    }

    void FixedUpdate()
    {
        if (Debugging)
        {
            Debug.Log(string.Format(
                "{4} motor: {5} {6}; brake: {7}, {8}; rpm: {0}, {1}; v: {2}, {3}",
                leftWheelController.rpm, rightWheelController.rpm, rb.velocity, rb.angularVelocity, role,
                leftWheelController.motor, rightWheelController.motor, leftWheelController.brake, rightWheelController.brake));
        }
    }

    public void SetWheelVelocity(Wheel ws)
    {
        leftWheelController.SetVelocity((float)ws.left);
        rightWheelController.SetVelocity((float)ws.right);
    }

    public void SetWheelVelocity(float left, float right)
    {
        leftWheelController.SetVelocity(left);
        rightWheelController.SetVelocity(right);
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

        SetWheelVelocity((float)robot.velocityLeft, (float)robot.velocityRight);
    }

    void InitRobotParameter()
    {
        rb.mass = RobotParameter.RobotMass;
        rb.drag = RobotParameter.RobotDrag;
        rb.angularDrag = RobotParameter.RobotAngularDrag;
    }
}

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

        OnPauseBtnClick();

        ForwardFactor = 39.5040157f;
        Drag = 45.42384722f;
        
        // 1441.413627651178231 
        TorqueFactor = 1441.30362765f;
        // 3214.140962
        AngularDrag = 3214.140962f;
    }

    private int tick;
    private int playTime;

    void FixedUpdate()
    {
        tick++;
        if (tick % 2 == 0)
            return;

        forward_force = -transform.forward * (LeftVelocity + RightVelocity) * ForwardFactor;

        forward_drag = rb.velocity * -Drag;
        rb.AddForce(forward_force + forward_drag);


        torque = Vector3.up * (LeftVelocity - RightVelocity) * TorqueFactor;
        angular_drag = rb.angularVelocity * -AngularDrag;
        rb.AddTorque(torque + angular_drag);

        OutputData();
    }

    float prevA = 0;
    private float prevZ = 0;

    void OutputData()
    {
//        var eulerAngles = rb.transform.eulerAngles;
//        Debug.Log(string.Format("{0:N10}", eulerAngles.y - prevAV));
//        prevA = eulerAngles.y;

        var z = rb.transform.position.z;
        Debug.Log(string.Format("{0:N10}", z - prevZ));
        prevZ = z;
        
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
using UnityEngine;
using Simuro5v5;

public class ControlBall : MonoBehaviour
{
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

    Collider Collider;
    Rigidbody rb;

    void Start()
    {
        _physicsEnabled = true;
        Collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        InitParameter();
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="ball"></param>
    public void SetPlacement(Ball ball)
    {
        Vector3 pos = new Vector3
        {
            x = ball.pos.x,
            z = ball.pos.y,
            y = Const.Ball.HBL + Const.Field.Height
        };
        transform.position = pos;
    }

    /// <summary>
    /// 设置速度和位置
    /// </summary>
    /// <param name="ball"></param>
    public void Revert(Ball ball)
    {
        SetPlacement(ball);
        if (physicsEnabled)
        {
            rb.velocity = ball.GetLinearVelocityVector3();
            rb.angularVelocity = ball.GetAngularVelocityVector3();
        }
    }

    /// <summary>
    /// 使静止
    /// </summary>
    public void SetStill()
    {
        if (physicsEnabled)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.Sleep();
            rb.WakeUp();
        }
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
    /// 禁用碰撞体，移除刚体
    /// </summary>
    void DisableRigidBodyAndCollider()
    {
        // Collider.enabled = false;
        Destroy(rb);
    }

    /// <summary>
    /// 设置参数
    /// </summary>
    public void InitParameter()
    {
        if (!physicsEnabled)
        {
            throw new PhysicsDisabledException();
        }
        rb.mass = Const.Ball.mass;
        rb.drag = rb.angularDrag = 0;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezePositionY;
    }
}

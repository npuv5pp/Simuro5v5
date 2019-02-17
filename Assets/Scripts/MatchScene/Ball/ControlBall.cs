using UnityEngine;
using Simuro5v5;
using LinearVelocity = UnityEngine.Vector3;
using AngularVelocity = System.Double;

public class ControlBall : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {

    }

    public void SetPlacement(Ball ball)
    {
        Vector3 pos;
        pos.x = ball.pos.x;
        pos.z = ball.pos.y;
        pos.y = ball.pos.z;
        transform.position = pos;
    }

    public void Revert(Ball ball)
    {
        SetPlacement(ball);
        //rb.velocity = ball.linearVelocity.GetUnityVector3();
        //rb.angularVelocity = ball.angularVelocity.GetUnityVector3();
    }

    public void SetStill()
    {
        rb.Sleep();
        rb.WakeUp();
    }
}

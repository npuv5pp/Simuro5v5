using UnityEngine;
using System.Collections;
using Simuro5v5;

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
        Vector3 pos = new Vector3
        {
            x = ball.pos.x,
            z = ball.pos.y,
            y = Const.Ball.HBL + Const.Field.Height
        };
        transform.position = pos;
        SetStill();
    }

    public void Revert(Ball ball)
    {
        SetPlacement(ball);
        rb.velocity = ball.GetLinearVelocityVector3();
        rb.angularVelocity = ball.GetAngularVelocityVector3();
    }

    public void SetStill()
    {
        rb.Sleep();
        rb.WakeUp();
    }
}

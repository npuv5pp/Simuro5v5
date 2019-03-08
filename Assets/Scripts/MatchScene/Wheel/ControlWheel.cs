using UnityEngine;
using Simuro5v5;

public class ControlWheel : MonoBehaviour
{
    WheelCollider wheelParameter;
    WheelCollider wheelCollider;
    const float wheelMultiplier = 197000 / 125;

    void Start ()
    {
        wheelCollider = GetComponent<WheelCollider>();
        wheelParameter = GameObject.Find("Parameter").GetComponent<WheelCollider>();

        InitParameter();
    }

    public float rpm { get { return wheelCollider.rpm; } }
    public float motor { get { return wheelCollider.motorTorque; } }
    public float brake { get { return wheelCollider.brakeTorque; } }

    public void SetVelocity(float v)
    {
        wheelCollider.motorTorque = GetMotor((float)v);
        wheelCollider.brakeTorque = v == 0 ? Const.Wheel.brakeTorque : 0;
    }

    public void ResetWheel()
    {
        DestroyImmediate(wheelCollider);
        wheelCollider = gameObject.AddComponent<WheelCollider>();
    }

    public void ResetWheel(float v)
    {
        DestroyImmediate(wheelCollider);
        wheelCollider = gameObject.AddComponent<WheelCollider>();
        SetVelocity(v);
    }

    float GetMotor(float power)
    {
        return power * wheelMultiplier;
    }

    void InitParameter()
    {
        wheelCollider.ConfigureVehicleSubsteps(Const.Wheel.criticalSpeed, Const.Wheel.stepsBelow, Const.Wheel.stepsAbove);
        wheelCollider.mass = Const.Wheel.mass;
        wheelCollider.radius = Const.Wheel.radius;
        wheelCollider.mass = wheelParameter.mass;
        wheelCollider.radius = wheelParameter.radius;
        wheelCollider.wheelDampingRate = wheelParameter.wheelDampingRate;
        wheelCollider.suspensionDistance = wheelParameter.suspensionDistance;
        wheelCollider.suspensionSpring = wheelParameter.suspensionSpring;
        wheelCollider.center = wheelParameter.center;
        wheelCollider.forceAppPointDistance = wheelParameter.forceAppPointDistance;
        wheelCollider.forwardFriction = wheelParameter.forwardFriction;
        wheelCollider.sidewaysFriction = wheelParameter.sidewaysFriction;
        Debug.Log("Parameter setted");
    }

    //float GetMotor(float power)
    //{
    //    //float motor = 0;
    //    //float velocity = rb.velocity.magnitude;
    //    //float k = 1.0f / 600.0f;
    //    //float offset = 2.0f;

    //    //motor = (power - velocity) / ((velocity + offset) * k);
    //    //Debug.Log("PlayTime=" + PlayTime + " power=" + power + " velocity=" + velocity + " motor=" + motor + " name=" + transform.name);

    //    //Debug.LogWarning(motor);
    //    //return motor;
    //    return power * wheelMultiplier;
    //}
}

using UnityEngine;
using System;
using System.Collections.Generic;
using Simuro5v5;

public class ControlTest2 : MonoBehaviour
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
    public float maxMotorTorque;
    public float maxSteeringAngle;
    int PlayTime;

    void Start()
    {
        PlayTime = 0;
    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        //float brake = Input.GetKey("k") ? Const.Wheel.brakeTorque : 0;
        float brake = motor == 0 ? Const.Wheel.brakeTorque : 0;

        if (motor != 0)
        {
            leftWheel.motorTorque = motor + steering;
            rightWheel.motorTorque = motor - steering;
        }

        leftWheel.brakeTorque = brake;
        rightWheel.brakeTorque = brake;

        Debug.Log("PlayTime=" + PlayTime + "\tmotor=" + motor + "\tbrake=" + brake);

        PlayTime++;
    }
}
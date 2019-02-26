using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotTests
{
    public class RobotTest : MonoBehaviour
    {
        public float standard_motor = 100;
        public float rotation_motor = 50;
        public float brake_motor = 50;
        public Rigidbody rb;
        public WheelCollider left;
        public WheelCollider right;
        public WheelCollider Template;
        public ParameterTest Parameter;

        int playtime = 0;
        float last_x;

        FileLog Logger { get; set; }

        void Start()
        {
            Logger = new FileLog("C:\\Users\\dangy\\Desktop\\log.csv");
            //Logger.LogLine("rpm_l, rpm_r, l_v_z, motor_l, motor_r");
            Logger.LogLine("v");
            last_x = rb.position.x;

            rb.maxAngularVelocity = 100;
        }

        private void FixedUpdate()
        {
            if (rb.transform.position.x >= 10543)
            {
                rb.transform.position = new Vector3
                {
                    x = -4295f,
                    z = 488.5f,
                };
            }

            standard_motor = Parameter.Motor;
            rb.mass = Parameter.RobotMass;
            rb.drag = Parameter.Drag;
            SetParameter(left);
            SetParameter(right);

            playtime++;

            float v = rb.position.x - last_x;
            last_x = rb.position.x;

            if (Parameter.use_wheel)
            {
                if (Parameter.left_wheel_v > 125)
                {
                    Parameter.left_wheel_v = 125;
                }
                else if (Parameter.left_wheel_v < -125)
                {
                    Parameter.left_wheel_v = -125;
                }
                left.motorTorque = 1576 * Parameter.left_wheel_v;
                if (Parameter.left_wheel_v == 0)
                {
                    left.brakeTorque = 10000;
                }

                if (Parameter.right_wheel_v > 125)
                {
                    Parameter.right_wheel_v = 125;
                }
                else if (Parameter.right_wheel_v < -125)
                {
                    Parameter.right_wheel_v = -125;
                }
                right.motorTorque = 1576 * Parameter.right_wheel_v;
                if (Parameter.right_wheel_v == 0)
                {
                    right.brakeTorque = 10000;
                }
            }
            else
            {
                left.motorTorque = right.motorTorque = standard_motor;
            }

            Debug.Log(string.Format("v: {4} ({7}); rpm: {0} {1}, motor: {2}, {3}, brake: {5} {6}",
                left.rpm, right.rpm, left.motorTorque, right.motorTorque,
                v, left.brakeTorque, right.brakeTorque,
                rb.velocity.x));
            Logger.LogLine(string.Format("{0}", v));
        }

        void SetParameter(WheelCollider wheel)
        {
            wheel.mass = Parameter.WheelParameter.Mass;
            wheel.radius = Parameter.WheelParameter.Radius;
            wheel.forceAppPointDistance = Parameter.WheelParameter.ForceAppPointDistance;
            wheel.forwardFriction = new WheelFrictionCurve
            {
                extremumSlip = Parameter.WheelParameter.FrictionCurve.ExtremumSlip,
                extremumValue = Parameter.WheelParameter.FrictionCurve.ExtremumValue,
                asymptoteSlip = Parameter.WheelParameter.FrictionCurve.AsymptoteSlip,
                asymptoteValue = Parameter.WheelParameter.FrictionCurve.AsymptoteValue,
                stiffness = Parameter.WheelParameter.FrictionCurve.Stiffness,
            };
            wheel.sidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = Parameter.WheelParameter.SidewayCurve.ExtremumSlip,
                extremumValue = Parameter.WheelParameter.SidewayCurve.ExtremumValue,
                asymptoteSlip = Parameter.WheelParameter.SidewayCurve.AsymptoteSlip,
                asymptoteValue = Parameter.WheelParameter.SidewayCurve.AsymptoteValue,
                stiffness = Parameter.WheelParameter.SidewayCurve.Stiffness,
            };
            wheel.brakeTorque = Parameter.Brake;
        }

        void OnDestroy()
        {
            Logger.close();
        }

    }
}

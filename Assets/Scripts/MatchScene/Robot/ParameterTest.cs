using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotTests
{
    public class ParameterTest : MonoBehaviour
    {
        public float RobotMass;
        public float Drag;
        public bool use_wheel;
        //public float wheel_v;
        public float left_wheel_v;
        public float right_wheel_v;
        public float Motor;
        public float Brake;
        public WheelParameter WheelParameter;
    }

    [Serializable]
    public class WheelParameter
    {
        public float Mass;
        public float Radius;
        public float ForceAppPointDistance;
        public Curve FrictionCurve;
        public Curve SidewayCurve;
    }

    [Serializable]
    public class Curve
    {
        public float ExtremumSlip;
        public float ExtremumValue;
        public float AsymptoteSlip;
        public float AsymptoteValue;
        public float Stiffness;
    }
}

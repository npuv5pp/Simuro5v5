using UnityEngine;
using Simuro5v5;

public class ControlWheel : MonoBehaviour
{
    WheelCollider wheel;
    public bool Debugging;

    void Start ()
    {
        wheel = GetComponent<WheelCollider>();

        if (Debugging)
        {
        }
        SetParameter();
    }

    public void ResetCollider()
    {
    }

    void SetParameter()
    {
        wheel.ConfigureVehicleSubsteps(Const.Wheel.criticalSpeed, Const.Wheel.stepsBelow, Const.Wheel.stepsAbove);
        wheel.mass = Const.Wheel.mass;
        wheel.radius = Const.Wheel.radius;

        wheel.wheelDampingRate = Const.Wheel.wheelDampingRate;
        wheel.suspensionDistance = Const.Wheel.suspensionDistance;
        wheel.forceAppPointDistance = Const.Wheel.forceAppPointDistance;

        WheelFrictionCurve fF = new WheelFrictionCurve();
        fF.extremumSlip = Const.Wheel.extremumSlip_fF;
        fF.extremumValue = Const.Wheel.extremumValue_fF;
        fF.asymptoteSlip = Const.Wheel.asymptoteSlip_fF;
        fF.asymptoteValue = Const.Wheel.asymptoteValue_fF;
        fF.stiffness = Const.Wheel.stiffness_fF;
        wheel.forwardFriction = fF;

        WheelFrictionCurve sF = new WheelFrictionCurve();
        sF.extremumSlip = Const.Wheel.extremumSlip_sF;
        sF.extremumValue = Const.Wheel.extremumValue_sF;
        sF.asymptoteSlip = Const.Wheel.asymptoteSlip_sF;
        sF.asymptoteValue = Const.Wheel.asymptoteValue_sF;
        sF.stiffness = Const.Wheel.stiffness_sF;
        wheel.sidewaysFriction = sF;

        JointSpring sS = new JointSpring();
        sS.damper = Const.Wheel.damper;
        sS.spring = Const.Wheel.spring;
        sS.targetPosition = Const.Wheel.targetPosition;
        wheel.suspensionSpring = sS;
        Debug.Log("Parameter setted");
    }
}

using System;
using UnityEngine;

namespace Simuro5v5
{
    static class Const
    {
        public readonly static int max_vec = 125;
        public readonly static int min_vec = -125;
        public readonly static int FPS = 66;                                // 模拟频率(其实不应该叫FPS)
        public readonly static float Zeit = (1.0f / 66.0f);                 // 模拟频率的倒数，即一拍的时间
        public readonly static float TimeScale = 1.0f;                      // 默认时间流速
        public readonly static float inch2cm = 2.54f;                       // 英寸转化成厘米
        public readonly static float cm2inch = 1.0f / 2.54f;                // 厘米转化成英寸

        public static class Field
        {
            public readonly static float Top = 90.0f;
            public readonly static float Bottom = -90.0f;
            public readonly static float Left = -110.0f;
            public readonly static float Right = 110.0f;
        }

        public static class Robot
        {
            public readonly static float RL = 7.8670658f;                   // 机器人边长         // 有舍入误差
            public readonly static float HRL = 3.9335329f;                  // 机器人半边长       // 有舍入误差
            public readonly static float maxVelocity = 125.0f;              // 机器人最大线速度
            //public readonly static float maxAngularVelocity = 70.07121363f; // 机器人最大角速度，角度制
            public readonly static float maxAngularVelocity = 4024.07121363f; // 机器人最大角速度，角度制
            public readonly static float kv = 66.0f;                        // 使加速度为1时需要乘的系数
            public readonly static float kv1 = 75.81f;                      // 使加速度为1时需要乘的系数，f=1.0
            public readonly static float kv2 = 71.88599f;                   // 使加速度为1时需要乘的系数，f=0.6
            public readonly static float kw = 11.89119f;                    // 使角加速度为1时需要乘的系数
            public readonly static float k1Dym = -0.0007292f;               // 修正引擎角速度非线性规律的参数
            public readonly static float k2Dym = 1.0f;                      // 修正引擎角速度非线性规律的参数
            public readonly static float range = 0.001f;                    // 重整角度阈值
            public readonly static float kf1 = Mathf.Exp(-1.0f / 15.71f);   // 直线运动一般情况下加速度系数，0.93833
            public readonly static float kf2 = Mathf.Exp(-1.0f / 0.9231f);  // 直线运动零减速情况下加速度系数，0.338475
            public readonly static float kt1 = Mathf.Exp(-1.0f / 3.096f);   // 转角时角加速度系数，0.723976
            public readonly static float r = 0.53461992f;                   // 小车对z轴的惯性半径
            public readonly static float r2 = 0.56057f;                     // 小车对z轴的惯性半径2
            public readonly static float dyF = 0.1486f;                     // 小车的动摩擦因数
            public readonly static float stF = dyF;                         // 小车的静摩擦因数
            public readonly static float bonc = 0.2f;                       // 小车的弹性系数
        }

        public static class Ball
        {
            public readonly static float BL = 5.043424f;                    // 球的直径
            public readonly static float HBL = 2.521712f;                   // 球的半径
            public readonly static float dyF = 1.0f;                        // 球的动摩擦因数
            public readonly static float stF = dyF;                         // 球的静摩擦因数
            public readonly static float bonc = 0.8f;                       // 球的弹性系数
        }

        public static class Wheel
        {
            public readonly static float mass = 10f;
            public readonly static float radius = 3.54018f * 10;

            public readonly static float wheelDampingRate = 0.0001f;      // 0.25
            public readonly static float suspensionDistance = 0.0f;     // 0.0
            public readonly static float forceAppPointDistance = 0.0f;  // 0.3

            public readonly static float brakeTorque = 30000f;          // 30000

            public readonly static float extremumSlip_fF = 100f;        // 0.4
            public readonly static float extremumValue_fF = 10f;       // 1.0
            public readonly static float asymptoteSlip_fF = 100f;       // 0.8
            public readonly static float asymptoteValue_fF = 10f;      // 0.5
            public readonly static float stiffness_fF = 15f;           // 1.0

            public readonly static float extremumSlip_sF = 0.2f;        // 0.2
            public readonly static float extremumValue_sF = 1.0f;       // 1.0
            public readonly static float asymptoteSlip_sF = 0.5f;       // 0.5
            public readonly static float asymptoteValue_sF = 0.75f;     // 0.75
            public readonly static float stiffness_sF = 0.0f;           // 1.0

            public readonly static float damper = 0;                // 4500
            public readonly static float spring = 0;               // 35000
            public readonly static float targetPosition = 0;

            // 低速时容易震动，所以多迭代几次
            public readonly static float criticalSpeed = 25f;          // 5.0
            public readonly static int stepsBelow = 8;                  // 5
            public readonly static int stepsAbove = 8;                  // 1
        }

        public static void DebugLog(string message)
        {
            Debug.Log(message);
        }

        public static void DebugLog2(string message)
        {
            Debug.Log(message);
        }
    }
}

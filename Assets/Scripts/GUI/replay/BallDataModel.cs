using System.Collections;
using System.Collections.Generic;
using Simuro5v5;
using UnityEngine;
using UnityEngine.UI;

public class BallDataModel : MonoBehaviour
{
    public InputField x, y, rotation;

    Ball Ball;

    void Update()
    {
        x.text = Ball.pos.x.ToString();
        y.text = Ball.pos.y.ToString();
    }

    public void RenderData(Ball ball)
    {
        Ball = ball;
    }
}

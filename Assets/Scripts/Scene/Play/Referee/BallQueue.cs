using System.Collections.Generic;
using UnityEngine;
using Simuro5v5;
using UnityEditor;

public class BallQueue
{
    public Queue<Vector2D> BallPosQueue;

    //争球十秒的拍数
    private readonly int Capacity = 10 * Const.FramePerSecond;
    private float LimitMove = Const.Robot.RL;

    Vector2D[] testpos;

    public BallQueue()
    {
        //660拍，10秒的
        BallPosQueue = new Queue<Vector2D>(Capacity);
        testpos = new Vector2D[Capacity];
    }

    public void Enqueue(Vector2D ballPos)
    {
        if (BallPosQueue.Count < Capacity)
        {
            BallPosQueue.Enqueue(ballPos);
        }
        else
        {
            if (BallPosQueue.Count != 0)
            {
                BallPosQueue.Dequeue();
            }
            BallPosQueue.Enqueue(ballPos);
        }
    }

    /// <summary>
    /// 当球的坐标骤变时将队列清空
    /// </summary>
    public void EmptyQueue()
    {
        BallPosQueue.Clear();
    }

    /// <summary>
    /// 判断是否需要进入争球
    /// </summary>
    /// <param name="newBallPos"></param>
    /// <returns></returns>
    public bool IsInFree(Vector2D newBallPos)
    {
        //test for limitcapacity

        //int test = 0;
        //int testCap = Capacity;
        //if (BallPosQueue.Count < Capacity)
        //    return false;
        //foreach (var pos in BallPosQueue)
        //{
        //    testpos[--testCap] = pos;
        //}
        //for (int i = 0; i < Capacity; i++)
        //{
        //    if (Vector2D.Distance(testpos[i], newBallPos) > LimitMove)
        //    {
        //        return false;
        //    }
        //    test++;
        //    if (test == 3* Const.FramePerSecond)
        //    {
        //        Time.timeScale = 0;
        //    }
        //}
        ////此时判罚是进入争球，进行清空
        //BallPosQueue.Clear();
        //return true;


        //real code
        if (BallPosQueue.Count < Capacity)
            return false;
        foreach (var pos in BallPosQueue)
        {
            if (Vector2D.Distance(pos, newBallPos) > LimitMove)
            {
                return false;
            }
        }
        //此时判罚是进入争球，进行清空
        BallPosQueue.Clear();
        return true;
    }
}
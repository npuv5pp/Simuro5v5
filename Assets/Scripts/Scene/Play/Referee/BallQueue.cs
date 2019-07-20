using System.Collections.Generic;
using Simuro5v5;

public class BallQueue
{
    public Queue<Vector2D> BallPosQueue;

    //争球十秒的拍数
    private int Capacity = 660;

    public BallQueue()
    {
        //660拍，10秒的
        BallPosQueue = new Queue<Vector2D>(Capacity);
    }

    public void Enqueue(Vector2D ballPos)
    {
        if (BallPosQueue.Count < Capacity)
        {
            BallPosQueue.Enqueue(ballPos);
        }
        else
        {
            BallPosQueue.Dequeue();
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
        if (BallPosQueue.Count < Capacity)
            return false;
        foreach (var pos in BallPosQueue)
        {
            if (Vector2D.Distance(pos, newBallPos) > 2 * Const.Robot.HRL)
            {
                return false;
            }
        }
        //此时判罚是进入争球，进行清空
        BallPosQueue.Clear();
        return true;
    }
}
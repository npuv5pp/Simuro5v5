using Simuro5v5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simuro5v5.Util
{
    public abstract class RectangleBase
    {
        // 1 --- 3
        // |  o  |
        // 4  -- 2

        /// <summary>
        /// 和 Point2 构成对角线
        /// </summary>
        protected abstract Vector2D Point1 { get; }
        
        /// <summary>
        /// 和 Point1 构成对角线
        /// </summary>
        protected abstract Vector2D Point2 { get; }
        
        /// <summary>
        /// 和 Point4 构成对角线
        /// </summary>
        protected abstract Vector2D Point3 { get; }
        
        /// <summary>
        /// 和 Point3 构成对角线
        /// </summary>
        protected abstract Vector2D Point4 { get; }

        /// <summary>
        /// 矩形中心点
        /// </summary>
        protected virtual Vector2D Midpoint => (Point1 + Point2) / 2;
        
        /// <summary>
        /// 获取一个集合，表示这个矩形的边缘线
        /// </summary>
        protected virtual List<(Vector2D, Vector2D)> Lines =>
            new List<(Vector2D, Vector2D)>()
            {
                (Point1, Point3), (Point1, Point4),
                (Point3, Point2), (Point4, Point2),
            };


        /// <summary>
        /// 判断两条直线 AB 与 CD 是否相交
        /// </summary>
        static bool LineCross(Vector2D a, Vector2D b, Vector2D c, Vector2D d)
        {
            var ac = c - a;
            var ad = d - a;
            var bc = c - b;
            var bd = d - b;
            var ca = -ac;
            var cb = -bc;
            var da = -ad;
            var db = -bd;

            return (ac * ad) * (bc * bd) <= 0 &&
                   (ca * cb) * (da * db) <= 0;
        }

        // TODO: Rename to `IsCrossedBy`
        public bool IsCrossBy(RectangleBase rect)
        {
            foreach (var (a, b) in Lines)
            {
                foreach (var (c, d) in rect.Lines)
                {
                    if (LineCross(a, b, c, d))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsInRectangle(UprightRectangle area)
            => area.PointIn(this.Midpoint) && !IsCrossBy(area);
    }
    
    public class UprightRectangle : RectangleBase
    {
        private float LeftX { get; }
        private float RightX { get; }
        private float TopY { get; }
        private float BotY { get; }

        protected override Vector2D Point1 => new Vector2D(LeftX, TopY);
        protected override Vector2D Point2 => new Vector2D(RightX, BotY);
        protected override Vector2D Point3 => new Vector2D(RightX, TopY);
        protected override Vector2D Point4 => new Vector2D(LeftX, BotY);

        public UprightRectangle(float leftX, float rightX, float topY, float botY)
        {
            this.LeftX = leftX;
            this.TopY = topY;
            this.RightX = rightX;
            this.BotY = botY;
        }

        public static UprightRectangle RobotSquare(Vector2D robotPosition)
        {
            float HRL = Const.Robot.HRL;
            UprightRectangle robotSquare = new UprightRectangle(robotPosition.x - HRL, robotPosition.x + HRL, robotPosition.y + HRL, robotPosition.y - HRL);
            return robotSquare;
        }

        public bool PointIn(Vector2D point)
        {
            return point.x < RightX && point.x > LeftX && point.y > BotY && point.y < TopY;
        }
    }

    public class Square : RectangleBase
    {
        /// <summary>
        /// 通过正方形的两个对角点构造正方形
        /// </summary>
        public Square(Vector2D point1, Vector2D point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        /// <summary>
        /// 通过机器人中心与半径以及角度来构造机器人正方形
        /// </summary>
        public Square(Vector2D robotPosition, float angle)
        {
            float robotRadius = (float)(Const.Robot.HRL * 1.414);
            //角度规整
            while (angle > 90 || angle < 0)
            {
                if (angle > 90)
                {
                    angle -= 90;
                }
                else
                    angle += 90;
            }
            float point1X = (float)(robotPosition.x + robotRadius * Math.Cos(angle));
            float point1Y = (float)(robotPosition.y + robotRadius * Math.Sin(angle));
            float point2X = (float)(robotPosition.x - robotRadius * Math.Cos(angle));
            float point2Y = (float)(robotPosition.y - robotRadius * Math.Sin(angle));
            Point1 = new Vector2D(point1X, point1Y);
            Point2 = new Vector2D(point2X, point2Y);
        }
        /// <summary>
        /// 第一个对角点
        /// </summary>
        protected override Vector2D Point1 { get; }

        /// <summary>
        /// 第二个对角点
        /// </summary>
        protected override Vector2D Point2 { get; }

        protected override Vector2D Point3 => (Point1 - Midpoint).Rotate(Mathf.PI / 4)
                                  + Midpoint;

        protected override Vector2D Point4 => (Point1 - Midpoint).Rotate(-Mathf.PI / 4)
                                  + Midpoint;

        public bool IsInCycle(Vector2D cenpos , float radius)
        {
            if (Vector2D.Distance(Point1, cenpos) < radius)
            {
                return false;
            }
            if (Vector2D.Distance(Point2, cenpos) < radius)
            {
                return false;
            }
            if (Vector2D.Distance(Point3, cenpos) < radius)
            {
                return false;
            }
            if (Vector2D.Distance(Point4, cenpos) < radius)
            {
                return false;
            }
            return true;
        }
    }
}

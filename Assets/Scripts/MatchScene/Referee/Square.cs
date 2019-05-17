using Simuro5v5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simuro5v5.Util
{
    public class UprightRectangle
    {
        private Vector2D TopLeftPoint;
        private Vector2D BotRightPoint;
        private float LeftX;
        private float RightX;
        private float TopY;
        private float BotY;

        public UprightRectangle(float LeftX, float RightX, float TopY, float BotY)
        {
            this.LeftX = LeftX;
            this.TopY = TopY;
            this.RightX = RightX;
            this.BotY = BotY;
        }

        public static UprightRectangle RobotSquare(Vector2D RobotPos)
        {
            float HRL = Const.Robot.HRL;
            UprightRectangle robotsquare = new UprightRectangle(RobotPos.x - HRL, RobotPos.x + HRL, RobotPos.y + HRL, RobotPos.y - HRL);
            return robotsquare;
        }

        public UprightRectangle(Vector2D TopLeftPoint, Vector2D BotRightPoint)
        {
            this.TopLeftPoint = TopLeftPoint;
            this.BotRightPoint = BotRightPoint;
        }

        public bool PointIn(Vector2D Point)
        {
            return Point.x < RightX && Point.x > LeftX && Point.y > BotY && Point.y < TopY;
        }
    }

    public class Square
    {
        // 1 --- 3
        // |  o  |
        // 4  -- 2

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
        public Square(Vector2D robotPos, float angle)
        {
            float robotRadiu = (float)(Const.Robot.HRL * 1.414);
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
            float point1x = (float)(robotPos.x + robotRadiu * Math.Cos(angle));
            float point1y = (float)(robotPos.y + robotRadiu * Math.Sin(angle));
            float point2x = (float)(robotPos.x - robotRadiu * Math.Cos(angle));
            float point2y = (float)(robotPos.y - robotRadiu * Math.Sin(angle));
            Point1 = new Vector2D(point1x, point1y);
            Point2 = new Vector2D(point2x, point2y);
        }
        /// <summary>
        /// 第一个对角点
        /// </summary>
        public Vector2D Point1 { get; }

        /// <summary>
        /// 第二个对角点
        /// </summary>
        public Vector2D Point2 { get; }

        public Vector2D Point3 => (Point1 - Midpoint).Rotate(Mathf.PI / 4)
                                  + Midpoint;

        public Vector2D Point4 => (Point1 - Midpoint).Rotate(-Mathf.PI / 4)
                                  + Midpoint;

        Vector2D Midpoint => (Point1 + Point2) / 2;

        List<(Vector2D, Vector2D)> Lines =>
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

        public bool IsCrossBy(Square rhs)
        {
            foreach (var (a, b) in Lines)
            {
                foreach (var (c, d) in rhs.Lines)
                {
                    if (LineCross(a, b, c, d))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsInCycle(Vector2D cenpos , float radiu)
        {
            if (Vector2D.Distance(Point1, cenpos) < radiu)
            {
                return false;
            }
            if (Vector2D.Distance(Point2, cenpos) < radiu)
            {
                return false;
            }
            if (Vector2D.Distance(Point3, cenpos) < radiu)
            {
                return false;
            }
            if (Vector2D.Distance(Point4, cenpos) < radiu)
            {
                return false;
            }
            return true;
        }
    }
}

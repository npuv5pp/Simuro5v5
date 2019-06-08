using System.Collections.Generic;
using NUnit.Framework;
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
        public abstract Vector2D Point1 { get; }
        
        /// <summary>
        /// 和 Point1 构成对角线
        /// </summary>
        public abstract Vector2D Point2 { get; }
        
        /// <summary>
        /// 和 Point4 构成对角线
        /// </summary>
        public abstract Vector2D Point3 { get; }
        
        /// <summary>
        /// 和 Point3 构成对角线
        /// </summary>
        public abstract Vector2D Point4 { get; }

        /// <summary>
        /// 矩形中心点
        /// </summary>
        protected virtual Vector2D Midpoint => (Point1 + Point2) / 2;
        
        /// <summary>
        /// 获取一个集合，表示这个矩形的边缘线
        /// </summary>
        protected virtual List<(Vector2D, Vector2D)> Lines =>
            new List<(Vector2D, Vector2D)>
            {
                (Point1, Point3), (Point1, Point4),
                (Point3, Point2), (Point4, Point2)
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

        public bool IsCrossedBy(RectangleBase rect)
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

        public virtual bool ContainsPoint(Vector2D p)
        {
            return (Point3 - Point1).Cross(p - Point1) >= 0
                   && (Point2 - Point3).Cross(p - Point3) >= 0
                   && (Point4 - Point2).Cross(p - Point2) >= 0
                   && (Point1 - Point4).Cross(p - Point4) >= 0;
        }
    }
    
    public class UprightRectangle : RectangleBase
    {
        private float LeftX { get; }
        private float RightX { get; }
        private float TopY { get; }
        private float BotY { get; }

        public override Vector2D Point1 => new Vector2D(LeftX, TopY);
        public override Vector2D Point2 => new Vector2D(RightX, BotY);
        public override Vector2D Point3 => new Vector2D(RightX, TopY);
        public override Vector2D Point4 => new Vector2D(LeftX, BotY);

        public UprightRectangle(float leftX, float rightX, float topY, float botY)
        {
            LeftX = leftX;
            TopY = topY;
            RightX = rightX;
            BotY = botY;
        }

        public override bool ContainsPoint(Vector2D point)
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
        public Square(Vector2D robotPosition, float angleInDegree = 0, float HRL = Const.Robot.HRL)
        {
            float robotRadius = (float) (HRL * 1.414);
            
            //角度规整
            while (angleInDegree > 45)
            {
                angleInDegree -= 90;
            }
            while (angleInDegree < -45)
            {
                angleInDegree += 90;
            }
            angleInDegree += 45;
            
            float point1X = robotPosition.x + robotRadius * Mathf.Cos(angleInDegree * Mathf.PI / 180);
            float point1Y = robotPosition.y + robotRadius * Mathf.Sin(angleInDegree * Mathf.PI / 180);
            float point2X = robotPosition.x - robotRadius * Mathf.Cos(angleInDegree * Mathf.PI / 180);
            float point2Y = robotPosition.y - robotRadius * Mathf.Sin(angleInDegree * Mathf.PI / 180);
            Point1 = new Vector2D(point1X, point1Y);
            Point2 = new Vector2D(point2X, point2Y);
        }
        /// <summary>
        /// 第一个对角点
        /// </summary>
        public override Vector2D Point1 { get; }

        /// <summary>
        /// 第二个对角点
        /// </summary>
        public override Vector2D Point2 { get; }

        public override Vector2D Point3 => (Point1 - Midpoint).Rotate(Mathf.PI / 2)
                                  + Midpoint;

        public override Vector2D Point4 => (Point1 - Midpoint).Rotate(-Mathf.PI / 2)
                                  + Midpoint;

        /// <summary>
        /// 判断正方形是否与园相交
        /// <br />
        /// TODO: 这个实现是不完整的
        /// </summary>
        public bool OverlapWithCircle(Vector2D center, float radius = Const.Ball.HBL)
        {
            foreach (var point in new[] {Point1, Point2, Point3, Point4})
            {
                if (Vector2D.Distance(point, center) < radius)
                {
                    return true;
                }
            }

            return ContainsPoint(center);
        }

        /// <summary>
        /// 测试是否在完全在一个矩形区域内部
        /// </summary>
        /// <param name="area">待判断的矩形区域</param>
        /// <returns></returns>
        public bool IsInRectangle(UprightRectangle area)
        {
            float width = Vector2D.Distance(Point1, Point3);
            float outer = Mathf.Max(
                Vector2D.Distance(area.Point1, area.Point3),
                Vector2D.Distance(area.Point1, area.Point4));
            return area.ContainsPoint(Midpoint) && !IsCrossedBy(area) && width < outer;
        }
    }

    [TestFixture]
    public class RectangleBaseTest
    {
        [Test]
        public void TestContainsPoint()
        {
            var square1 = new Square(new Vector2D(0, 0), new Vector2D(1, 1));
            Assert.IsTrue(square1.ContainsPoint(new Vector2D(0.5f, 0.5f)));
            Assert.IsTrue(square1.ContainsPoint(new Vector2D(0.01f, 0.99f)));
            Assert.IsFalse(square1.ContainsPoint(new Vector2D(0, -0.01f)));
            Assert.IsFalse(square1.ContainsPoint(new Vector2D(0.5f, 1.01f)));
            
            var square2 = new Square(new Vector2D(0, 1), new Vector2D(2, 1));
            Assert.IsTrue(square2.ContainsPoint(new Vector2D(1, 1)));
            Assert.IsTrue(square2.ContainsPoint(new Vector2D(0.01f, 1)));
            Assert.IsTrue(square2.ContainsPoint(new Vector2D(0.5f, 1.01f)));
            Assert.IsFalse(square2.ContainsPoint(new Vector2D(1.5f,  0.499f)));
            
            var square3 = new Square(new Vector2D(0, -1), new Vector2D(0, 1));
            Assert.IsTrue(square3.ContainsPoint(new Vector2D(0, -0.99f)));
            Assert.IsTrue(square3.ContainsPoint(new Vector2D(0, 0.99f)));
            Assert.IsTrue(square3.ContainsPoint(new Vector2D(0.5f, 0.499f)));
            Assert.IsFalse(square3.ContainsPoint(new Vector2D(1.01f, 0)));
            Assert.IsFalse(square3.ContainsPoint(new Vector2D(-1.01f, 0)));
        }

        [Test]
        public void TestIsCrossBy()
        {
            var square1 = new Square(new Vector2D(0, 0), new Vector2D(1, 1));
            var square2 = new Square(new Vector2D(1, 0.5f), new Vector2D(2, 0.5f));
            Assert.IsTrue(square1.IsCrossedBy(square2));
            
            var square3 = new Square(new Vector2D(0.2f, 0.2f), new Vector2D(0.8f, 0.8f));
            Assert.IsFalse(square1.IsCrossedBy(square3));
        }
    }

    [TestFixture]
    class UprightRectangleTest
    {
        [Test]
        public void TestContainsPoint()
        {
            var rect = new UprightRectangle(0, 1, 1, 0);
            Assert.IsTrue(rect.ContainsPoint(new Vector2D(0.1f, 0.1f)));
            Assert.IsTrue(rect.ContainsPoint(new Vector2D(0.5f, 0.99f)));
            Assert.IsFalse(rect.ContainsPoint(new Vector2D(0.5f, 1.01f)));
        }
    }

    [TestFixture]
    class SquareTest
    {
        [Test]
        public void TestIsInRectangle()
        {
            var rectangle = new UprightRectangle(0, 2, 2, 0);
            var square1 = new Square(new Vector2D(0.01f, 0.01f), new Vector2D(0.99f, 0.99f));
            Assert.IsTrue(square1.IsInRectangle(rectangle));
            
            var square2 = new Square(new Vector2D(0.5f, 0.01f), new Vector2D(0.5f, 0.99f));
            Assert.IsTrue(square2.IsInRectangle(rectangle));
            
            var square3 = new Square(new Vector2D(0, 0), new Vector2D(-1, 1));
            Assert.IsFalse(square3.IsInRectangle(rectangle));
            
            var square4 = new Square(new Vector2D(-1, -1), new Vector2D(3, 3));
            Assert.IsFalse(square4.IsInRectangle(rectangle));
        }

        [Test]
        public void TestCtor()
        {
            var square1 = new Square(new Vector2D(0, 0), 0, 1);
            Assert.IsFalse(square1.Point1.IsNotNear(new Vector2D(1, 1)));
            Assert.IsFalse(square1.Point2.IsNotNear(new Vector2D(-1, -1)));

            float sqrt2 = Mathf.Sqrt(2);
            var square2 = new Square(new Vector2D(0, 0), 45, sqrt2 / 2);
            var square3 = new Square(new Vector2D(0, 0), 45 + 360, sqrt2 / 2);
            var square4 = new Square(new Vector2D(0, 0), 45 - 360, sqrt2 / 2);
            Assert.IsFalse(square2.Point1.IsNotNear(new Vector2D(0, 1)));
            Assert.IsFalse(square3.Point1.IsNotNear(new Vector2D(0, 1)));
            Assert.IsFalse(square4.Point1.IsNotNear(new Vector2D(1, 0)));
        }

        [Test]
        public void TestOverlapWithCircle()
        {
            var square = new Square(new Vector2D(-1, -1), new Vector2D(1, 1));
            Assert.IsTrue(square.OverlapWithCircle(Vector2D.Zero, 1.42f));
            Assert.IsTrue(square.OverlapWithCircle(Vector2D.Zero, 1.4f));
            Assert.IsTrue(square.OverlapWithCircle(new Vector2D(1, 0), 1.4f));
            Assert.IsFalse(square.OverlapWithCircle(new Vector2D(2, 0), 0.9f));
        }
    }
}

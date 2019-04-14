using Simuro5v5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.MatchScene.Referee
{

    class Square
    {
        private Vector2D TopLeftPoint;
        private Vector2D BotRightPoint;
        private float LeftX;
        private float RightX;
        private float TopY;
        private float BotY;

        public Square(float LeftX, float RightX, float TopY, float BotY)
        {
            this.LeftX = LeftX;
            this.TopY = TopY;
            this.RightX = RightX;
            this.BotY = BotY;
        }

        public Square(Vector2D TopLeftPoint, Vector2D BotRightPoint)
        {
            this.TopLeftPoint = TopLeftPoint;
            this.BotRightPoint = BotRightPoint;
        }

        public bool InSquare(Vector2D Point)
        {
            if (Point.x < RightX && Point.x > LeftX && Point.y > BotY && Point.y < TopY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

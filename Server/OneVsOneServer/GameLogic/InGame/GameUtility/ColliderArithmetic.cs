using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OneVsOneServer.GameLogic.InGame
{
    public class ColliderArithmetic
    {

        private static float epsilon = 0.00001f;

        //public class Collision {
        //    public bool isCollision;
        //    public Vector3 collisionPoint;

        //    public Collision(bool isColl, Vector3 point) {
        //        isCollision = isColl;
        //        collisionPoint = point;
        //    }

        //    public static Collision DoesNotCollide
        //    {
        //        get
        //        {
        //            return new Collision(false, Vector3.zero);
        //        }
        //    }
        //}

        #region 충돌체들

        public class Line2D
        {
            public Vector2 p1;
            public Vector2 p2;

            public Line2D(Vector2 v1, Vector2 v2)
            {
                p1 = v1;
                p2 = v2;
            }
        }

        public class Triangle2D
        {
            public Vector2 p1;
            public Vector2 p2;
            public Vector2 p3;

            public Triangle2D(Vector2 v1, Vector2 v2, Vector2 v3)
            {
                p1 = v1;
                p2 = v2;
                p3 = v3;
            }
        }

        public class Rectangle2D
        {
            public Vector2 leftUp;
            public Vector2 rightUp;
            public Vector2 rightDown;
            public Vector2 leftDown;

            public Rectangle2D(Vector2 leftUpPivot, float height, float width)
            {
                leftUp = leftUpPivot;
                rightUp = new Vector2(leftUp.X + width, leftUp.Y);
                rightDown = new Vector2(rightUp.X, rightUp.Y - height);
                leftDown = new Vector2(rightDown.X - width, rightDown.Y);
            }
        }

        public class Circle2D
        {
            public Vector2 point;
            public float radius;

            public Circle2D(Vector2 v, float r)
            {
                point = v;
                radius = r;
            }
        }
        #endregion


        #region 선과 관련된 충돌 함수들

        public static bool IsCollision(Line2D line1, Line2D line2)
        {
            float den = (line2.p2.Y - line2.p1.Y) * (line1.p2.X - line1.p1.X) - (line2.p2.X - line2.p1.X) * (line1.p2.Y - line1.p1.Y);

            if (-epsilon < den && epsilon > den)
            { //평행임
                return false;
            }

            float ua = ((line2.p2.X - line2.p1.X) * (line1.p1.Y - line2.p1.Y) - (line2.p2.Y - line2.p1.Y) * (line1.p1.X - line2.p1.X)) / den;
            float ub = ((line1.p2.X - line1.p1.X) * (line1.p1.Y - line2.p1.Y) - (line1.p2.Y - line1.p1.Y) * (line1.p1.X - line2.p1.X)) / den;

            if (0 <= ua && 1 > ua && 0 <= ub && 1 > ub)
            {
                return true; //교차함
            }

            return false;
        }

        public static bool IsCollision(Line2D line, Triangle2D tri)
        {
            //선의 점이 삼각형에 포함되어있는지 검사

            if (IsCollision(tri, line.p1) || IsCollision(tri, line.p2))
            { //둘중 하나라도 포함되어있다면 그것은 충돌한 것이다.
                return true;
            }

            //선이 삼각형의 각 변과 교차하여 있는지 검사

            if (IsCollision(line, new Line2D(tri.p1, tri.p2)) || IsCollision(line, new Line2D(tri.p1, tri.p3)) || IsCollision(line, new Line2D(tri.p2, tri.p3))) //한 선이라도 교차했다면 그것은 충돌한 것 이다.
            {
                return true;
            }

            return false;
        }

        public static bool IsCollision(Line2D line, Rectangle2D rect)
        {
            //사각형의 네 선중 하나라도 교차된게 있다면 충돌함
            if (IsCollision(line, new Line2D(rect.leftUp, rect.rightUp)) || IsCollision(line, new Line2D(rect.rightUp, rect.rightDown))
                || IsCollision(line, new Line2D(rect.rightDown, rect.leftDown)) || IsCollision(line, new Line2D(rect.leftDown, rect.leftUp)))
            {
                return true;
            }

            // 사각형을 삼각형 두개로 쪼개고 선의 양 끝점과 충돌검사를 한다.
            if (IsCollision(new Triangle2D(rect.leftUp, rect.leftDown, rect.rightDown), line.p1) || IsCollision(new Triangle2D(rect.leftUp, rect.rightUp, rect.rightDown), line.p1)
                || IsCollision(new Triangle2D(rect.leftUp, rect.leftDown, rect.rightDown), line.p2) || IsCollision(new Triangle2D(rect.leftUp, rect.rightUp, rect.rightDown), line.p2))
            {
                return true;
            }

            return false;
        }

        public static bool IsCollision(Line2D line, Circle2D circle)
        {

            Vector2 v = circle.point - line.p1;

            Vector3 outer = Vector3.Cross(new Vector3(v.X, v.Y, 0.0f), new Vector3(line.p2.X, line.p2.Y, 0.0f));
            float sqrMagnitude = (outer.X * outer.X) + (outer.Y * outer.Y);
            if (sqrMagnitude > (circle.radius * circle.radius))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 삼각형과 관련된 충돌 함수들

        public static bool IsCollision(Triangle2D tri, Vector2 point)
        {
            //점이 삼각형 내에 있는지 검사
            Vector2 mark; //기준벡터
            Vector2 comp; //비교벡터

            mark = tri.p2 - tri.p1;
            comp = tri.p3 - tri.p1;

            Vector2[] normals = { new Vector2(tri.p1.Y, -tri.p1.X), new Vector2(tri.p2.Y, -tri.p2.X), new Vector2(tri.p3.Y, -tri.p3.X) };

            for (int i = 0; i < normals.Length; i++)
            {
                if (Vector2.Dot(normals[i], mark) * Vector2.Dot(normals[i], comp) <= 0)
                { // 음수이면 각도 내에 존재하지 않는다는 뜻임(한곳이라도 각도내에 존재하지 않는다면 충돌하지 않았다는것)
                    return false;
                }
            }

            return true;
        }

        public static bool IsCollision(Triangle2D tri, Line2D line)
        {
            return IsCollision(line, tri);
        }

        public static bool IsCollision(Triangle2D tri1, Triangle2D tri2)
        {
            if (IsCollision(tri1, tri2.p1) || IsCollision(tri1, tri2.p2) || IsCollision(tri1, tri2.p3)
                || IsCollision(tri2, tri1.p1) || IsCollision(tri2, tri1.p2) || IsCollision(tri2, tri1.p3))
            {
                return true;
            }
            return false;
        }

        public static bool IsCollision(Triangle2D tri, Rectangle2D rect)
        {
            if (IsCollision(tri, new Triangle2D(rect.leftUp, rect.leftDown, rect.rightDown)) || IsCollision(tri, new Triangle2D(rect.leftUp, rect.rightUp, rect.rightDown)))
            {
                return true;
            }
            return false;
        }

        public static bool IsCollision(Triangle2D tri, Circle2D circle)
        {
            if (IsCollision(new Line2D(tri.p1, tri.p2), circle) || IsCollision(new Line2D(tri.p1, tri.p3), circle) || IsCollision(new Line2D(tri.p2, tri.p3), circle))
            {
                return true;
            }

            if (IsCollision(tri, circle.point))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 사각형과 관련된 충돌 함수들

        public static bool IsCollision(Rectangle2D rect, Line2D line)
        {
            return IsCollision(line, rect);
        }

        public static bool IsCollision(Rectangle2D rect, Triangle2D tri)
        {
            return IsCollision(tri, rect);
        }

        public static bool IsCollision(Rectangle2D rect1, Rectangle2D rect2)
        {
            if (IsCollision(rect1, new Triangle2D(rect2.leftUp, rect2.leftDown, rect2.rightDown)) || IsCollision(rect1, new Triangle2D(rect2.leftUp, rect2.rightUp, rect2.rightDown)))
            {
                return true;
            }

            return false;
        }

        public static bool IsCollision(Rectangle2D rect, Circle2D circle)
        {
            if (IsCollision(new Line2D(rect.leftUp, rect.rightUp), circle) || IsCollision(new Line2D(rect.rightUp, rect.rightDown), circle)
                || IsCollision(new Line2D(rect.rightDown, rect.leftDown), circle) || IsCollision(new Line2D(rect.leftDown, rect.leftUp), circle))
            {
                return true;
            }

            if (IsCollision(new Triangle2D(rect.leftUp, rect.rightUp, rect.rightDown), circle.point) || IsCollision(new Triangle2D(rect.leftUp, rect.leftDown, rect.rightDown), circle))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region 원과 관련된 충돌 함수

        public static bool IsCollision(Circle2D circle, Line2D line)
        {
            return IsCollision(line, circle);
        }

        public static bool IsCollision(Circle2D circle, Triangle2D tri)
        {
            return IsCollision(tri, circle);
        }

        public static bool IsCollision(Circle2D circle, Rectangle2D rect)
        {
            return IsCollision(rect, circle);
        }

        public static bool IsCollision(Circle2D circle1, Circle2D circle2)
        {
            if (Vector2.Distance(circle1.point, circle2.point) > (circle1.radius + circle2.radius))
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}

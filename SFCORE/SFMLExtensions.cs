using SFML.Graphics;
using SFML.System;
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace SFCORE
{
    public class LineInfo
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
    }
    public class LineSegment
    {
        public Vector2f PointA { get; set; }
        public Vector2f PointB { get; set; }
    }

    public static class MathUtil
    {

        static readonly RandomNumberGenerator Rng = new RNGCryptoServiceProvider();
        static readonly byte[] Bytes = new byte[4];

        public static float Bottom(this FloatRect rect)
        {
            return rect.Top + rect.Height;
        }
        public static bool Exists(this Vector2f v)
        {
            return !float.IsNaN(v.X) && !float.IsNaN(v.Y);
        }
        public static Vector2f Copy(this Vector2f v)
        {
            return new Vector2f(v.X, v.Y);
        }

        public static Vector2f ToVector(this float rotation)
        {
            return new Vector2f(MathF.Cos(rotation), MathF.Sin(rotation));
        }

        public static float Rotation(this Vector2f v)
        {
            var arctan = (float)(Math.Atan2(v.Y, v.X) * 180 / Math.PI);
            if (v.X < 0)
            {
                arctan -= 360;
            }
            return arctan;
        }
        public static FloatRect GetTargetRegion(this View view)
        {
            var center = view.Center;
            var size = view.Size;
            return new FloatRect(new Vector2f(center.X - size.X / 2, center.Y - size.Y / 2), size);
        }

        public static float Right(this FloatRect rect)
        {
            return rect.Left + rect.Width;
        }

        public static Random RNG = new Random();
        //public static float GetRandom()
        //{
        //    //Rng.GetBytes(Bytes);
        //    //return BitConverter.ToSingle(Bytes, 0);
        //}

        public static float Magnitude(this Vector2f vector)
        {
            return MathF.Sqrt(vector.SquareMagnitude());
        }

        public static int SquareMagnitude(this Vector2i vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y;
        }

        public static Vector2f ToFloat(this Vector2i v)
        {
            return new Vector2f(v.X, v.Y);
        }
        public static Vector2f ToFloat(this Vector2u v)
        {
            return new Vector2f(v.X, v.Y);
        }
        public static Vector2f ToSFVec(this Vector2 v)
        {
            return new Vector2f(v.X, v.Y);
        }
        public static float SquareMagnitude(this Vector2f vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y;
        }
        public static Vector2f Normalize(this Vector2f vector)
        {
            if (vector.X == 0 && vector.Y == 0)
                return vector;
            return vector / vector.Magnitude();
        }

        public static float Lerp(this float from, float to, float fraction)
        {
            return from + (to - from) * fraction;
        }

        public static byte Lerp(this byte from, byte to, float fraction)
        {
            return (byte)(from + (to - from) * fraction);
        }

        public static Color Lerp(this Color from, Color to, float fraction)
        {
            return new Color(from.R.Lerp(to.R, fraction), from.G.Lerp(to.G, fraction), from.B.Lerp(to.B, fraction), from.A.Lerp(to.A, fraction));
        }


        public static float Dot(this Vector2f me, Vector2f other)
        {
            return me.X * other.X + me.Y * other.Y;
        }




        public static LineInfo GetLineInfo(Vector2f origin, Vector2f direction)
        {
            var point2 = origin + direction;
            return GetLineInfo(origin.X, point2.X, origin.Y, point2.Y);
        }

        public static LineInfo GetLineInfo(float x1, float x2, float y1, float y2)
        {
            var a = -(y2 - y1);
            var b = x2 - x1;
            return new LineInfo
            {
                A = a,
                B = b,
                C = a * x1 + b * y1
            };
        }

        public static float SquaredDistanceTo(this LineSegment segment, Vector2f point)
        {
            var n = (segment.PointB - segment.PointA).Normalize();
            var aToP = segment.PointA - point;
            return (aToP - n * (n.Dot(aToP))).SquareMagnitude();
        }


        public static Vector2f Reflect(this Vector2f point, LineSegment segment)
        {
            var dx = segment.PointB.X - segment.PointA.X;
            var dy = segment.PointB.Y - segment.PointA.Y;

            var dDot = dx * dx + dy * dy;

            var a = (dx * dx - dy * dy) / dDot;
            var b = 2 * dx * dy / dDot;

            return new Vector2f(a * (point.X - segment.PointA.X) + b * (point.Y - segment.PointA.Y) + segment.PointA.X, b * (point.X - segment.PointA.X) - a * (point.Y - segment.PointA.Y) + segment.PointA.Y);
        }
    }
}

// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.17  Engin.Kırmacı            Initial creation
// ============================================================================

using System.Windows;

namespace NISDKExtendedEffects.Extensions
{
    static public class PointExtension
    {
        static public Point Add(this Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }

        static public Point Add(this Point point, int valueToAdd)
        {
            return new Point(point.X + valueToAdd, point.Y + valueToAdd);
        }

        static public Point Subtract(this Point point1, Point point2)
        {
            return new Point(point1.X - point2.X, point1.Y - point2.Y);
        }

        static public Point Divide(this Point point, int factor)
        {
            return new Point(point.X / factor, point.Y / factor);
        }

        static public float DistanceTo(this Point point, Point anotherPoint)
        {
            int dx = (int)(point.X - anotherPoint.X);
            int dy = (int)(point.Y - anotherPoint.Y);

            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
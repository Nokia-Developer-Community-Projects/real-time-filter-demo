// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.17  Engin.Kırmacı            Initial creation
// ============================================================================

using NISDKExtendedEffects.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NISDKExtendedEffects.Maths
{
    public static class PointsCloud
    {
        /// <summary>
        /// Get bounding rectangle of the specified list of points.
        /// </summary>
        ///
        /// <param name="cloud">Collection of points to get bounding rectangle for.</param>
        /// <param name="minXY">Point comprised of smallest X and Y coordinates.</param>
        /// <param name="maxXY">Point comprised of biggest X and Y coordinates.</param>
        ///
        public static void GetBoundingRectangle(IEnumerable<Point> cloud, out Point minXY, out Point maxXY)
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (Point pt in cloud)
            {
                int x = (int)pt.X;
                int y = (int)pt.Y;

                // check X coordinate
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;

                // check Y coordinate
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
            }

            if (minX > maxX) // if no point appeared to set either minX or maxX
                throw new ArgumentException("List of points can not be empty.");

            minXY = new Point(minX, minY);
            maxXY = new Point(maxX, maxY);
        }

        /// <summary>
        /// Get center of gravity for the specified list of points.
        /// </summary>
        ///
        /// <param name="cloud">List of points to calculate center of gravity for.</param>
        ///
        /// <returns>Returns center of gravity (mean X-Y values) for the specified list of points.</returns>
        ///
        public static Point GetCenterOfGravity(IEnumerable<Point> cloud)
        {
            int numberOfPoints = 0;
            float xSum = 0, ySum = 0;

            foreach (Point pt in cloud)
            {
                xSum += (int)pt.X;
                ySum += (int)pt.Y;
                numberOfPoints++;
            }

            xSum /= numberOfPoints;
            ySum /= numberOfPoints;

            return new Point(xSum, ySum);
        }

        /// <summary>
        /// Find furthest point from the specified point.
        /// </summary>
        ///
        /// <param name="cloud">Collection of points to search furthest point in.</param>
        /// <param name="referencePoint">The point to search furthest point from.</param>
        ///
        /// <returns>Returns a point, which is the furthest away from the <paramref name="referencePoint"/>.</returns>
        ///
        public static Point GetFurthestPoint(IEnumerable<Point> cloud, Point referencePoint)
        {
            Point furthestPoint = referencePoint;
            float maxDistance = -1;

            int rx = (int)referencePoint.X;
            int ry = (int)referencePoint.Y;

            foreach (Point point in cloud)
            {
                int dx = (int)(rx - point.X);
                int dy = (int)(ry - point.Y);
                // we are not calculating square root for finding "real" distance,
                // since it is really not important for finding furthest point
                float distance = dx * dx + dy * dy;

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestPoint = point;
                }
            }

            return furthestPoint;
        }

        public static void GetFurthestPointsFromLine(IEnumerable<Point> cloud, Point linePoint1, Point linePoint2,
          out Point furthestPoint1, out Point furthestPoint2)
        {
            float d1, d2;

            GetFurthestPointsFromLine(cloud, linePoint1, linePoint2,
                out furthestPoint1, out d1, out furthestPoint2, out d2);
        }

        public static void GetFurthestPointsFromLine(IEnumerable<Point> cloud, Point linePoint1, Point linePoint2,
            out Point furthestPoint1, out float distance1, out Point furthestPoint2, out float distance2)
        {
            furthestPoint1 = linePoint1;
            distance1 = 0;

            furthestPoint2 = linePoint2;
            distance2 = 0;

            if (linePoint2.X != linePoint1.X)
            {
                // line's equation y(x) = k * x + b
                float k = (float)(linePoint2.Y - linePoint1.Y) / (float)(linePoint2.X - linePoint1.X);
                float b = (float)(linePoint1.Y - k * linePoint1.X);

                float div = (float)Math.Sqrt(k * k + 1);
                float distance = 0;

                foreach (Point point in cloud)
                {
                    distance = (float)(k * point.X + b - point.Y) / div;

                    if (distance > distance1)
                    {
                        distance1 = distance;
                        furthestPoint1 = point;
                    }
                    if (distance < distance2)
                    {
                        distance2 = distance;
                        furthestPoint2 = point;
                    }
                }
            }
            else
            {
                int lineX = (int)linePoint1.X;
                float distance = 0;

                foreach (Point point in cloud)
                {
                    distance = (float)(lineX - point.X);

                    if (distance > distance1)
                    {
                        distance1 = distance;
                        furthestPoint1 = point;
                    }
                    if (distance < distance2)
                    {
                        distance2 = distance;
                        furthestPoint2 = point;
                    }
                }
            }

            distance2 = -distance2;
        }

        /// <summary>
        /// Relative distortion limit allowed for quadrilaterals, [0.0, 0.25].
        /// </summary>
        ///
        /// <remarks><para>The value of this property is used to calculate distortion limit used by
        /// <see cref="FindQuadrilateralCorners"/>, when processing potential corners and making decision
        /// if the provided points form a quadrilateral or a triangle. The distortion limit is
        /// calculated as:
        /// <code lang="none">
        /// distrtionLimit = RelativeDistortionLimit * ( W * H ) / 2,
        /// </code>
        /// where <b>W</b> and <b>H</b> are width and height of the "points cloud" passed to the
        /// <see cref="FindQuadrilateralCorners"/>.
        /// </para>
        ///
        /// <para>To explain the idea behind distortion limit, let’s suppose that quadrilateral finder routine found
        /// the next candidates for corners:<br />
        /// <img src="img/math/potential_corners.png" width="151" height="128" /><br />
        /// As we can see on the above picture, the shape there potentially can be a triangle, but not quadrilateral
        /// (suppose that points list comes from a hand drawn picture or acquired from camera, so some
        /// inaccuracy may exist). It may happen that the <b>D</b> point is just a distortion (noise, etc).
        /// So the <see cref="FindQuadrilateralCorners"/> check what is the distance between a potential corner
        /// (D in this case) and a line connecting two adjacent points (AB in this case). If the distance is smaller
        /// then the distortion limit, then the point may be rejected, so the shape turns into triangle.
        /// </para>
        ///
        /// <para>An exception is the case when both <b>C</b> and <b>D</b> points are very close to the <b>AB</b> line,
        /// so both their distances are less than distortion limit. In this case both points will be accepted as corners -
        /// the shape is just a flat quadrilateral.</para>
        ///
        /// <para>Default value is set to <b>0.1</b>.</para>
        /// </remarks>
        ///
        public static float QuadrilateralRelativeDistortionLimit
        {
            get { return quadrilateralRelativeDistortionLimit; }
            set { quadrilateralRelativeDistortionLimit = Math.Max(0.0f, Math.Min(0.25f, value)); }
        }
        private static float quadrilateralRelativeDistortionLimit = 0.1f;

        /// <summary>
        /// Find corners of quadrilateral or triangular area, which contains the specified collection of points.
        /// </summary>
        ///
        /// <param name="cloud">Collection of points to search quadrilateral for.</param>
        ///
        /// <returns>Returns a list of 3 or 4 points, which are corners of the quadrilateral or
        /// triangular area filled by specified collection of point. The first point in the list
        /// is the point with lowest X coordinate (and with lowest Y if there are several points
        /// with the same X value). The corners are provided in counter clockwise order
        /// (<a href="http://en.wikipedia.org/wiki/Cartesian_coordinate_system">Cartesian
        /// coordinate system</a>).</returns>
        ///
        /// <remarks><para>The method makes an assumption that the specified collection of points
        /// form some sort of quadrilateral/triangular area. With this assumption it tries to find corners
        /// of the area.</para>
        ///
        /// <para><note>The method does not search for <b>bounding</b> quadrilateral/triangular area,
        /// where all specified points are <b>inside</b> of the found quadrilateral/triangle. Some of the
        /// specified points potentially may be outside of the found quadrilateral/triangle, since the
        /// method takes corners only from the specified collection of points, but does not calculate such
        /// to form true bounding quadrilateral/triangle.</note></para>
        ///
        /// <para>See <see cref="QuadrilateralRelativeDistortionLimit"/> property for additional information.</para>
        /// </remarks>
        ///
        public static List<Point> FindQuadrilateralCorners(IEnumerable<Point> cloud)
        {
            // quadrilateral's corners
            List<Point> corners = new List<Point>();

            // get bounding rectangle of the points list
            Point minXY, maxXY;
            PointsCloud.GetBoundingRectangle(cloud, out minXY, out maxXY);
            // get cloud's size
            Point cloudSize = maxXY.Subtract(minXY);
            // calculate center point
            Point center = minXY.Add(cloudSize.Divide(2));
            // acceptable deviation limit
            float distortionLimit = quadrilateralRelativeDistortionLimit * (float)((cloudSize.X + cloudSize.Y) / 2);

            // get the furthest point from (0,0)
            Point point1 = PointsCloud.GetFurthestPoint(cloud, center);
            // get the furthest point from the first point
            Point point2 = PointsCloud.GetFurthestPoint(cloud, point1);

            corners.Add(point1);
            corners.Add(point2);

            // get two furthest points from line
            Point point3, point4;
            float distance3, distance4;

            PointsCloud.GetFurthestPointsFromLine(cloud, point1, point2,
                out point3, out distance3, out point4, out distance4);

            // ideally points 1 and 2 form a diagonal of the
            // quadrilateral area, and points 3 and 4 form another diagonal

            // but if one of the points (3 or 4) is very close to the line
            // connecting points 1 and 2, then it is one the same line ...
            // which means corner was not found.
            // in this case we deal with a trapezoid or triangle, where
            // (1-2) line is one of it sides.

            // another interesting case is when both points (3) and (4) are
            // very close the (1-2) line. in this case we may have just a flat
            // quadrilateral.

            if (
                 ((distance3 >= distortionLimit) && (distance4 >= distortionLimit)) ||

                 ((distance3 < distortionLimit) && (distance3 != 0) &&
                   (distance4 < distortionLimit) && (distance4 != 0)))
            {
                // don't add one of the corners, if the point is already in the corners list
                // (this may happen when both #3 and #4 points are very close to the line
                // connecting #1 and #2)
                if (!corners.Contains(point3))
                {
                    corners.Add(point3);
                }
                if (!corners.Contains(point4))
                {
                    corners.Add(point4);
                }
            }
            else
            {
                // it seems that we deal with kind of trapezoid,
                // where point 1 and 2 are on the same edge

                Point tempPoint = (distance3 > distance4) ? point3 : point4;

                // try to find 3rd point
                PointsCloud.GetFurthestPointsFromLine(cloud, point1, tempPoint,
                    out point3, out distance3, out point4, out distance4);

                bool thirdPointIsFound = false;

                if ((distance3 >= distortionLimit) && (distance4 >= distortionLimit))
                {
                    if (point4.DistanceTo(point2) > point3.DistanceTo(point2))
                        point3 = point4;

                    thirdPointIsFound = true;
                }
                else
                {
                    PointsCloud.GetFurthestPointsFromLine(cloud, point2, tempPoint,
                        out point3, out distance3, out point4, out distance4);

                    if ((distance3 >= distortionLimit) && (distance4 >= distortionLimit))
                    {
                        if (point4.DistanceTo(point1) > point3.DistanceTo(point1))
                            point3 = point4;

                        thirdPointIsFound = true;
                    }
                }

                if (!thirdPointIsFound)
                {
                    // failed to find 3rd edge point, which is away enough from the temp point.
                    // this means that the clound looks more like triangle
                    corners.Add(tempPoint);
                }
                else
                {
                    corners.Add(point3);

                    // try to find 4th point
                    float tempDistance;

                    PointsCloud.GetFurthestPointsFromLine(cloud, point1, point3,
                        out tempPoint, out tempDistance, out point4, out distance4);

                    if ((distance4 >= distortionLimit) && (tempDistance >= distortionLimit))
                    {
                        if (tempPoint.DistanceTo(point2) > point4.DistanceTo(point2))
                            point4 = tempPoint;
                    }
                    else
                    {
                        PointsCloud.GetFurthestPointsFromLine(cloud, point2, point3,
                            out tempPoint, out tempDistance, out point4, out distance4);

                        if ((tempPoint.DistanceTo(point1) > point4.DistanceTo(point1)) &&
                             (tempPoint != point2) && (tempPoint != point3))
                        {
                            point4 = tempPoint;
                        }
                    }

                    if ((point4 != point1) && (point4 != point2) && (point4 != point3))
                        corners.Add(point4);
                }
            }

            // put the point with lowest X as the first
            for (int i = 1, n = corners.Count; i < n; i++)
            {
                if ((corners[i].X < corners[0].X) ||
                     ((corners[i].X == corners[0].X) && (corners[i].Y < corners[0].Y)))
                {
                    Point temp = corners[i];
                    corners[i] = corners[0];
                    corners[0] = temp;
                }
            }

            // sort other points in counter clockwise order
            double k1 = (corners[1].X != corners[0].X) ?
                ((corners[1].Y - corners[0].Y) / (corners[1].X - corners[0].X)) :
                ((corners[1].Y > corners[0].Y) ? double.PositiveInfinity : double.NegativeInfinity);

            double k2 = (corners[2].X != corners[0].X) ?
                ((corners[2].Y - corners[0].Y) / (corners[2].X - corners[0].X)) :
                ((corners[2].Y > corners[0].Y) ? double.PositiveInfinity : double.NegativeInfinity);

            if (k2 < k1)
            {
                Point temp = corners[1];
                corners[1] = corners[2];
                corners[2] = temp;

                double tk = k1;
                k1 = k2;
                k2 = tk;
            }

            if (corners.Count == 4)
            {
                double k3 = (corners[3].X != corners[0].X) ?
                    ((corners[3].Y - corners[0].Y) / (corners[3].X - corners[0].X)) :
                    ((corners[3].Y > corners[0].Y) ? double.PositiveInfinity : double.NegativeInfinity);

                if (k3 < k1)
                {
                    Point temp = corners[1];
                    corners[1] = corners[3];
                    corners[3] = temp;

                    double tk = k1;
                    k1 = k3;
                    k3 = tk;
                }
                if (k3 < k2)
                {
                    Point temp = corners[2];
                    corners[2] = corners[3];
                    corners[3] = temp;

                    double tk = k2;
                    k2 = k3;
                    k3 = tk;
                }
            }

            return corners;
        }
    }
}
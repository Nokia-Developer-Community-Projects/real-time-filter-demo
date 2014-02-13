// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// ============================================================================

using System;
using Windows.Foundation;

namespace NISDKExtendedEffects.Entities
{
    public class EdgePoints
    {
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }
        public Rect Bounds { get; set; }

        public EdgePoints()
        {
            TopLeft = new Point();
            TopRight = new Point();
            BottomLeft = new Point();
            BottomRight = new Point();

            Bounds = Rect.Empty;
        }

        public EdgePoints(EdgePoints edgePoints)
        {
            TopLeft = edgePoints.TopLeft;
            TopRight = edgePoints.TopRight;
            BottomLeft = edgePoints.BottomLeft;
            BottomRight = edgePoints.BottomRight;

            Bounds = edgePoints.Bounds;
        }

        public EdgePoints(Rect bounds)
        {
            TopLeft = new Point();
            TopRight = new Point();
            BottomLeft = new Point();
            BottomRight = new Point();

            Bounds = bounds;
        }

        public EdgePoints(System.Windows.Rect bounds)
        {
            TopLeft = new Point();
            TopRight = new Point();
            BottomLeft = new Point();
            BottomRight = new Point();

            Bounds = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public EdgePoints ZoomIn(double scale)
        {
            EdgePoints result = new EdgePoints(Bounds);
            result.TopLeft = new Point(TopLeft.X / scale, TopLeft.Y / scale);
            result.TopRight = new Point(TopRight.X / scale, TopRight.Y / scale);
            result.BottomLeft = new Point(BottomLeft.X / scale, BottomLeft.Y / scale);
            result.BottomRight = new Point(BottomRight.X / scale, BottomRight.Y / scale);

            return result;
        }

        public void SetMaximum()
        {
            this.TopLeft = new Point(Bounds.X, Bounds.Y);
            this.TopRight = new Point(Bounds.X + Bounds.Width, Bounds.Y);
            this.BottomLeft = new Point(Bounds.X, Bounds.Y + Bounds.Height);
            this.BottomRight = new Point(Bounds.X + Bounds.Width, Bounds.Y + Bounds.Height);
        }

        public Size EstimatedRectangleSize()
        {
            var result = new Size();
            double heightLeft, heightRight, widthTop, widthBottom;

            heightLeft = Math.Abs(BottomLeft.Y - TopLeft.Y);
            heightRight = Math.Abs(BottomRight.Y - TopRight.Y);

            widthTop = Math.Abs(TopRight.X - TopLeft.X);
            widthBottom = Math.Abs(BottomRight.X - BottomLeft.X);

            if (heightLeft < heightRight)
                result.Height = (int)heightLeft;
            else
                result.Height = (int)heightRight;

            if (widthTop < widthBottom)
                result.Width = (int)widthTop;
            else
                result.Width = (int)widthBottom;

            return result;
        }
    }
}
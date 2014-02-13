// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// ============================================================================

using System.Windows;
using Windows.UI;

namespace NISDKExtendedEffects.Entities
{
    public class Blob
    {
        // blob's rectangle in the original image
        private Rect rect;

        // blob's ID in the original image
        private int id;

        // area of the blob
        private int area;

        // center of gravity
        private Point cog;

        // fullness of the blob ( area / ( width * height ) )
        private double fullness;

        // mean color of the blob
        private Color colorMean = Color.FromArgb(255, 255, 255, 255);

        // color's standard deviation of the blob
        private Color colorStdDev = Color.FromArgb(255, 255, 255, 255);

        public Rect Rectangle
        {
            get { return rect; }
        }

        public int ID
        {
            get { return id; }
            internal set { id = value; }
        }

        public int Area
        {
            get { return area; }
            internal set { area = value; }
        }

        public double Fullness
        {
            get { return fullness; }
            internal set { fullness = value; }
        }

        public Point CenterOfGravity
        {
            get { return cog; }
            internal set { cog = value; }
        }

        public Color ColorMean
        {
            get { return colorMean; }
            internal set { colorMean = value; }
        }

        public Color ColorStdDev
        {
            get { return colorStdDev; }
            internal set { colorStdDev = value; }
        }

        public Blob(int id, Rect rect)
        {
            this.id = id;
            this.rect = rect;
        }

        public Blob(Blob source)
        {
            id = source.id;
            rect = source.rect;
            cog = source.cog;
            area = source.area;
            fullness = source.fullness;
            colorMean = source.colorMean;
            colorStdDev = source.colorStdDev;
        }
    }
}
// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// ============================================================================

using Nokia.Graphics.Imaging;
using Windows.UI;

namespace NISDKExtendedEffects.ImageEffects
{
    public class HistogramCalculation : CustomEffectBase
    {
        public int[] Red { get; set; }

        public int[] Green { get; set; }

        public int[] Blue { get; set; }

        public int[] Luminance { get; set; }

        public HistogramCalculation(IImageProvider source, out int[] red, out int[] green, out int[] blue, out int[] luminance)
            : base(source, true)
        {
            Red = new int[byte.MaxValue + 1];
            Green = new int[byte.MaxValue + 1];
            Blue = new int[byte.MaxValue + 1];
            Luminance = new int[byte.MaxValue + 1];

            red = Red;
            green = Green;
            blue = Blue;
            luminance = Luminance;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    Color c = ToColor(sourcePixelRegion.ImagePixels[index]);

                    Red[c.R]++;

                    Green[c.G]++;

                    Blue[c.B]++;

                    double luminance = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
                    if (luminance < byte.MinValue)
                        luminance = byte.MinValue;
                    else if (luminance > byte.MaxValue)
                        luminance = byte.MaxValue;

                    Luminance[(byte)luminance]++;
                }
            });
        }
    }
}
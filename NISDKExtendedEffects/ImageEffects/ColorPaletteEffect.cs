// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// ============================================================================

using Nokia.Graphics.Imaging;
using System;
using Windows.UI;

namespace NISDKExtendedEffects.ImageEffects
{
    public class ColorPaletteEffect : CustomEffectBase
    {
        private Color[] _colorList { get; set; }

        public ColorPaletteEffect(IImageProvider source, Color[] colorList)
            : base(source)
        {
            _colorList = colorList;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    Color c = ToColor(sourcePixelRegion.ImagePixels[index]);

                    double dbl_input_red = c.R;
                    double dbl_input_green = c.G;
                    double dbl_input_blue = c.B;
                    // the Euclidean distance to be computed
                    // set this to an arbitrary number
                    // must be greater than the largest possible distance (appr. 441.7)
                    double distance = 500.0;
                    // store the interim result
                    double temp;
                    // RGB-Values of test colors
                    double dbl_test_red;
                    double dbl_test_green;
                    double dbl_test_blue;

                    foreach (var o in _colorList)
                    {
                        // compute the Euclidean distance between the two colors
                        // note, that the alpha-component is not used in this example
                        dbl_test_red = Math.Pow(Convert.ToDouble(o.R) - dbl_input_red, 2.0);
                        dbl_test_green = Math.Pow(Convert.ToDouble(o.G) - dbl_input_green, 2.0);
                        dbl_test_blue = Math.Pow(Convert.ToDouble(o.B) - dbl_input_blue, 2.0);
                        temp = Math.Sqrt(dbl_test_blue + dbl_test_green + dbl_test_red);
                        // explore the result and store the nearest color
                        if (temp < distance)
                        {
                            distance = temp;
                            c = o;
                        }
                    }

                    targetPixelRegion.ImagePixels[index] = FromColor(c);
                }
            });
        }
    }
}
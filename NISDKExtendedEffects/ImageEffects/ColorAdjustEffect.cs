// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.15  Rob.Kachmar              Initial creation
// ============================================================================
// NOTE: The built-in ColorAdjustFilter() is 3 times faster than this. It was 
//       simply created to show the logic that can be used to achieve an effect
//       that allows adjusting the color components of an image.
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class ColorAdjustEffect : CustomEffectBase
    {
        private double m_RedPercentage = 0;
        private double m_GreenPercentage = 0;
        private double m_BluePercentage = 0;

        public ColorAdjustEffect(IImageProvider source, double red = 0, double green = 0, double blue = 0)
            : base(source)
        {
            m_RedPercentage = red;
            m_GreenPercentage = green;
            m_BluePercentage = blue;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    uint currentPixel = sourcePixels[index]; // get the current pixel
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    // Take the percentage of each color component and add it to itself
                    // ex. A -1.0 or -100% will completely reduce a color component to 0
                    // ex. A 1.0 or 100% will double the value of the color component, up to the maximum of 255 of course
                    red = (uint)Math.Max(0, Math.Min(255, (red + (red * m_RedPercentage))));
                    green = (uint)Math.Max(0, Math.Min(255, (green + (green * m_GreenPercentage))));
                    blue = (uint)Math.Max(0, Math.Min(255, (blue + (blue * m_BluePercentage))));

                    uint newPixel = 0xff000000 | (red << 16) | (green << 8) | blue; // reassembling each component back into a pixel
                    targetPixels[index] = newPixel; // assign the newPixel to the equivalent location in the output image
                }
            });
        }
    }
}
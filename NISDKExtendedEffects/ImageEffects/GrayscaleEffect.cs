// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.15  Rob.Kachmar              Initial creation
// 2014.02.03  Rob.Kachmar              Allow adjustments to weighted avg algo
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class GrayscaleEffect : CustomEffectBase
    {
        private double m_RedPercentage = 0.2126;
        private double m_GreenPercentage = 0.7152;
        private double m_BluePercentage = 0.0722;

        public GrayscaleEffect(IImageProvider source, double red = 0.2126, double green = 0.7152, double blue = 0.0722)
            : base(source)
        {
            // Self correct negative percentages to just zero them out and ensure no percentage goes above 1.0 (100%)
            m_RedPercentage = (red < 0.00) ? 0.00 : (red > 1.00) ? 1.00 : red;
            m_GreenPercentage = (green < 0.00) ? 0.00 : (green > 1.00) ? 1.00 : green;
            m_BluePercentage = (blue < 0.00) ? 0.00 : (blue > 1.00) ? 1.00 : blue;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    // 16-17 FPS with built-in GrayscaleFilter() and 11 FPS with this technique on Lumia 920
                    uint currentPixel = sourcePixels[index]; // get the current pixel
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    // Calculate the weighted avearge of all the color components
                    // REFERENCE: http://en.wikipedia.org/wiki/Grayscale
                    uint grayscaleAverage = (uint)Math.Max(0, Math.Min(255, (
                        m_RedPercentage * red +
                        m_GreenPercentage * green +
                        m_BluePercentage * blue)));

                    // Assign the result to each component
                    red = green = blue = grayscaleAverage;

                    uint newPixel = 0xff000000 | (red << 16) | (green << 8) | blue; // reassembling each component back into a pixel
                    targetPixels[index] = newPixel; // assign the newPixel to the equivalent location in the output image
                }
            });
        }
    }
}
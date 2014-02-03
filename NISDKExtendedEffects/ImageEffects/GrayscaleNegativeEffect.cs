// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.15  Rob.Kachmar              Initial creation
// 2014.02.03  Rob.Kachmar              Allow adjustments to weighted avg algo
//                                      and handling static images
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class GrayscaleNegativeEffect : CustomEffectBase
    {
        private double m_RedPercentage = 0.2126;
        private double m_GreenPercentage = 0.7152;
        private double m_BluePercentage = 0.0722;

        public GrayscaleNegativeEffect(IImageProvider source, double red = 0.2126, double green = 0.7152, double blue = 0.0722)
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
                    uint pixel = sourcePixels[index]; // get the current pixel

                    if (pixel > 0) // Only manipulate pixels that have color
                    {
                        uint red = (pixel & 0x00ff0000) >> 16; // red color component
                        uint green = (pixel & 0x0000ff00) >> 8; // green color component
                        uint blue = pixel & 0x000000ff; // blue color component

                        // Calculate the weighted avearge of all the color components
                        // REFERENCE: http://en.wikipedia.org/wiki/Grayscale
                        uint grayscaleAverage = (uint)Math.Max(0, Math.Min(255, (
                            m_RedPercentage * red +
                            m_GreenPercentage * green +
                            m_BluePercentage * blue)));

                        // Assign the result to each component
                        red = green = blue = grayscaleAverage;

                        // Reassembling each component back into a pixel
                        pixel = 0xff000000 | (red << 16) | (green << 8) | blue;

                        // Flip the bits of the pixel to create the negative effect
                        pixel = ~pixel;

                        // Assign the pixels back by each component so we can ensure the alpha channel 
                        // is at 255. Otherwise the image will not be visible if it is static.
                        targetPixels[index] = 0xff000000 | (pixel & 0x00ff0000) | (pixel & 0x0000ff00) | pixel & 0x000000ff;
                    }
                }
            });
        }
    }
}
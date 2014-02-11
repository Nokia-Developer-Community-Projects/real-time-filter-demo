// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.15  Rob.Kachmar              Initial creation
// 2014.02.01  Rob.Kachmar              Reducing Math calc for performance
// ============================================================================
// NOTE: The built-in BrightnessFilter() is more than twice as fast as this.  
//       This was simply created to show the logic that can be used to achieve 
//       an effect that allows adjusting the brightness of an image.
//       Lumia 920 - 16-17 FPS with BrightnessFilter() 
//                   vs 
//                   7-8 FPS with BrightnessEffect()
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class BrightnessEffect : CustomEffectBase
    {
        private double m_BrightnessPercentage = 0;

        public BrightnessEffect(IImageProvider source, double brightness = 0)
            : base(source)
        {
            m_BrightnessPercentage = brightness;
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

                    if (!currentPixel.Equals(0)) // Only process if it is not transparent and part of the image
                    {
                        uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                        uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                        uint blue = currentPixel & 0x000000ff; // blue color component


                        if (m_BrightnessPercentage < 0)
                        {
                            // Dimming the image - A value of -1 or -100%, will completely reduce all colors to 0
                            // Reduce each color component by the passed in percentage of the amount of color
                            // it is currently displaying.
                            red = (uint)Math.Max(0, (red + (red * m_BrightnessPercentage)));
                            green = (uint)Math.Max(0, (green + (green * m_BrightnessPercentage)));
                            blue = (uint)Math.Max(0, (blue + (blue * m_BrightnessPercentage)));
                        }
                        else
                        {
                            // Brightening the image - A value of 1 or 100%, will completely increase all colors to 255
                            // Increase each color component by the passed in percentage of the amount of color
                            // is has left before it reaches the max of 255.
                            red = (uint)Math.Min(255, (red + ((255 - red) * m_BrightnessPercentage)));
                            green = (uint)Math.Min(255, (green + ((255 - green) * m_BrightnessPercentage)));
                            blue = (uint)Math.Min(255, (blue + ((255 - blue) * m_BrightnessPercentage)));
                        }


                        // Reassembling each component back into a pixel for the target pixel location
                        targetPixels[index] = 0xff000000 | (red << 16) | (green << 8) | blue;                         
                    }
                }
            });
        }
    }
}




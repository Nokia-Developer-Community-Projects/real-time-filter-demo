// NOTE: The built-in BrightnessFilter() is 3 times faster than this.  This was simply created to show the 
//       logic that can be used to achieve an effect that allows adjusting the brightness of an image.

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
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    if (m_BrightnessPercentage < 0)
                    {
                        // Dimming the image - A value of -1 or -100%, will completely reduce all colors to 0
                        // Reduce each color component by the passed in percentage of the amount of color
                        // it is currently displaying.
                        red = (uint)Math.Max(0, Math.Min(255, (red + (red * m_BrightnessPercentage))));
                        green = (uint)Math.Max(0, Math.Min(255, (green + (green * m_BrightnessPercentage))));
                        blue = (uint)Math.Max(0, Math.Min(255, (blue + (blue * m_BrightnessPercentage))));
                    }
                    else
                    {
                        // Brightening the image - A value of 1 or 100%, will completely increase all colors to 255
                        // Increase each color component by the passed in percentage of the amount of color
                        // is has left before it reaches the max of 255.
                        red = (uint)Math.Max(0, Math.Min(255, (red + ((255 - red) * m_BrightnessPercentage))));
                        green = (uint)Math.Max(0, Math.Min(255, (green + ((255 - green) * m_BrightnessPercentage))));
                        blue = (uint)Math.Max(0, Math.Min(255, (blue + ((255 - blue) * m_BrightnessPercentage))));
                    }

                    uint newPixel = 0xff000000 | (red << 16) | (green << 8) | blue; // reassembling each component back into a pixel
                    targetPixels[index] = newPixel; // assign the newPixel to the equivalent location in the output image
                }
            });
        }
    }
}




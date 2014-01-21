using System;
using Nokia.Graphics.Imaging;

namespace NISDKExtendedEffects.ImageEffects
{
    public class SkipPixelEffect : CustomEffectBase
    {
        private int m_ProcessEveryNthRow = 1;
        private int m_ProcessEveryNthColumn = 1;
        private int m_RowModuloTarget = 0;
        private int m_ColumnModuloTarget = 0;
        private double m_BrightnessPercentage = 0;

        public SkipPixelEffect(IImageProvider source, int processEveryNthRow = 1, int processEveryNthColumn = 1, 
            double brightness = 0) : base(source)
        {
            m_ProcessEveryNthRow = (processEveryNthRow <= 0) ? 1 : processEveryNthRow; // Protect against divide by zero
            m_ProcessEveryNthColumn = (processEveryNthColumn <= 0) ? 1 : processEveryNthColumn; // Protect against divide by zero
            m_RowModuloTarget = m_ProcessEveryNthRow - 1;
            m_ColumnModuloTarget = m_ProcessEveryNthColumn - 1;
            m_BrightnessPercentage = brightness;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            int rowIndex = 0;
            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                if ((rowIndex % m_ProcessEveryNthRow).Equals(m_RowModuloTarget)) // only process on every other Nth pixel per row
                {
                    for (int x = 0; x < width; ++x, ++index)
                    {
                        if ((x % m_ProcessEveryNthColumn).Equals(m_ColumnModuloTarget)) // only process on every other Nth pixel per column
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
                    }
                }
                rowIndex++;
            });
        }
    }
}



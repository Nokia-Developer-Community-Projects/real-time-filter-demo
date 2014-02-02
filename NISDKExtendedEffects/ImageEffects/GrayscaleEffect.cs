using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class GrayscaleEffect : CustomEffectBase
    {
        public GrayscaleEffect(IImageProvider source)
            : base(source)
        {
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

                    // Calculate the weighted avearge of all the color components and assign the result to each component
                    // REFERENCE: http://en.wikipedia.org/wiki/Grayscale
                    uint grayscaleAverage = (uint)(0.2126 * red + 0.7152 * green + 0.0722 * blue);
                    red = green = blue = grayscaleAverage;

                    uint newPixel = 0xff000000 | (red << 16) | (green << 8) | blue; // reassembling each component back into a pixel
                    targetPixels[index] = newPixel; // assign the newPixel to the equivalent location in the output image
                }
            });
        }
    }
}
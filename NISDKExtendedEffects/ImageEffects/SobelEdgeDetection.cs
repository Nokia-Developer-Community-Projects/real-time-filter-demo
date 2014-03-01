// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// 2014.02.28  Engin.Kırmacı            Remove unused property
// 2014.03.01  Engin.Kırmacı            Applied AForge.NET version of algorithm
// ============================================================================

using NISDKExtendedEffects.Extensions;
using Nokia.Graphics.Imaging;
using System;
using System.Windows;

namespace NISDKExtendedEffects.ImageEffects
{
    public class SobelEdgeDetection : CustomEffectBase
    {
        private bool scaleIntensity = true;
        public bool ScaleIntensity
        {
            get { return scaleIntensity; }
            set { scaleIntensity = value; }
        }

        public SobelEdgeDetection(IImageProvider source)
            : base(source)
        {
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            int width = (int)sourcePixelRegion.Bounds.Width;
            int height = (int)sourcePixelRegion.Bounds.Height;

            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            Rect rect = new Rect(0, 0, width, height);
            // processing start and stop X,Y positions
            int startX = (int)rect.Left + 1;
            int startY = (int)rect.Top + 1;
            int stopX = startX + (int)rect.Width - 2;
            int stopY = startY + (int)rect.Height - 2;

            int srcStride = width;

            int srcOffset = srcStride - (int)rect.Width + 2;

            // variables for gradient calculation
            uint g, max = 0;
            int pos = srcStride * startY + startX;
            // for each line
            for (int y = startY; y < stopY; y++)
            {
                // for each pixel
                for (int x = startX; x < stopX; x++, pos++)
                {
                    g = (uint)Math.Min(255, Math.Abs(sourcePixels[pos - srcStride - 1].ToGray() + sourcePixels[pos - srcStride + 1].ToGray()
                                - sourcePixels[pos + srcStride - 1].ToGray() - sourcePixels[pos + srcStride + 1].ToGray()
                                + 2 * (sourcePixels[pos - srcStride].ToGray() - sourcePixels[pos + srcStride].ToGray()))
                      + Math.Abs(sourcePixels[pos - srcStride + 1].ToGray() + sourcePixels[pos + srcStride + 1].ToGray()
                                - sourcePixels[pos - srcStride - 1].ToGray() - sourcePixels[pos + srcStride - 1].ToGray()
                                + 2 * (sourcePixels[pos + 1].ToGray() - sourcePixels[pos - 1].ToGray())));

                    if (g > max)
                        max = g;
                    targetPixels[pos] = 0xff000000 | (g << 16) | (g << 8) | g;
                }
                pos += srcOffset;
            }

            // do we need scaling
            if ((scaleIntensity) && (max != 255))
            {
                // make the second pass for intensity scaling
                double factor = 255.0 / (double)max;
                pos = srcStride;

                // for each line
                for (int y = startY; y < stopY; y++)
                {
                    pos++;
                    // for each pixel
                    for (int x = startX; x < stopX; x++, pos++)
                    {
                        var tmp = (byte)(factor * targetPixels[pos].ToGray());
                        targetPixels[pos] = 0xff000000 | ((uint)tmp << 16) | ((uint)tmp << 8) | (uint)tmp;
                    }
                    pos += srcOffset;
                }
            }

            uint black = 0xff000000 | (0 << 16) | (0 << 8) | 0;
            //Remove edge pixels
            for (int j = 0; j < width; j++)
            {
                targetPixels[j] = black;
                targetPixels[(height - 1) * width + j] = black;
            }
            for (int i = 0; i < height; i++)
            {
                targetPixels[i * width] = black;
                targetPixels[i * width + width - 1] = black;
            }
        }
    }
}
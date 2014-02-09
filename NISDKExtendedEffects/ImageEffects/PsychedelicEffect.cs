// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.20  Rob.Kachmar              Initial creation
// ============================================================================

using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;

namespace NISDKExtendedEffects.ImageEffects
{
    public class PsychedelicEffect : CustomEffectBase
    {
        private byte m_factor = 50;

        public PsychedelicEffect(IImageProvider source, byte factor = 50) : base(source)
        {
            m_factor = factor;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    // NOTE: Just pulling out the color components and reassembling them brings you down to 14-15 FPS
                    uint currentPixel = sourcePixels[index]; // get the current pixel
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    // Original accidental code
                    //red = Math.Max(0, Math.Min(255, (uint)(int)(red - m_factor)));
                    //green = Math.Max(0, Math.Min(255, (uint)(int)(green - m_factor)));
                    //blue = Math.Max(0, Math.Min(255, (uint)(int)(blue - m_factor)));

                    // Max out any color component that falls below zero - 12-13 FPS
                    red = (red < m_factor ? 255 : red - m_factor);
                    green = (green < m_factor ? 255 : green - m_factor);
                    blue = (blue < m_factor ? 255 : blue - m_factor);

                    // Reassemble each component back into a pixel and assign it to the equivalent output image location
                    targetPixels[index] = 0xff000000 | (red << 16) | (green << 8) | blue;
                }
            });
        }
    }
}

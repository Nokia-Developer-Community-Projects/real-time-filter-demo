// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.20  Rob.Kachmar              Initial creation
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

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
                    uint currentPixel = sourcePixels[index]; // get the current pixel
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    // Original accidental code
                    //red = Math.Max(0, Math.Min(255, (uint)(int)(red - m_factor)));
                    //green = Math.Max(0, Math.Min(255, (uint)(int)(green - m_factor)));
                    //blue = Math.Max(0, Math.Min(255, (uint)(int)(blue - m_factor)));

                    // Max out any color component that falls below zero
                    red = (uint)((red - m_factor < 0) ? 255 : Math.Max(0, (red - m_factor)));
                    green = (uint)((green - m_factor < 0) ? 255 : Math.Max(0, (green - m_factor)));
                    blue = (uint)((blue - m_factor < 0) ? 255 : Math.Max(0, (blue - m_factor)));

                    uint newPixel = 0xff000000 | (red << 16) | (green << 8) | blue; // reassembling each component back into a pixel
                    targetPixels[index] = newPixel; // assign the newPixel to the equivalent location in the output image
                }
            });
        }
    }


}

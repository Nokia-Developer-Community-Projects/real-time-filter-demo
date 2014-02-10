// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.01  Rob.Kachmar              Initial creation
// 2014.02.08  Rob.Kachmar              Added parameter for sepia intensity
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class SepiaEffect : CustomEffectBase
    {
        // http://www.aforgenet.com/framework/docs/html/10a0f824-445b-dcae-02ef-349d4057da45.htm
        // 920--> 19-20 FPS with inbuilt filter and 6-7 FPS with the custom effect
        uint m_Intensity = 42; // 42 seems to be the closest to the SDK

        public SepiaEffect(IImageProvider source, double intensity = 0.42) : base(source)
        {
            // Self correct negative percentages to just zero them out and ensure no percentage goes above 1.0 (100%)
            m_Intensity = (uint)((intensity < 0.00) ? 0.00 : (intensity > 1.00) ? 1.00 : intensity * 100);
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
                    uint alpha = (currentPixel & 0xff000000) >> 24; // alpha component

                    if (!alpha.Equals(0)) // Only process if it is not transparent
                    {
                        uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                        uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                        uint blue = currentPixel & 0x000000ff; // blue color component

                        // RGB to YIQ
                        uint Y = (uint)(0.299 * red + 0.587 * green + 0.114 * blue);
                        uint I = (uint)Math.Max((0.596 * red - 0.274 * green - 0.322 * blue), 0);
                        //uint Q = (uint)Math.Max((0.212 * red - 0.523 * green + 0.311 * blue), 0); // No need to calculate since we zero out
                        //uint Q = 0; // No need to even create the variable if we are not using the exact original formula

                        // Update for Sepia look
                        I = m_Intensity;

                        // YIQ to RGB
                        // Original formula for converting back from YIQ to RGB
                        //red = (uint)Math.Min((1.0 * Y + 0.956 * I + 0.621 * Q), 255);
                        //green = (uint)Math.Min(Math.Max((1.0 * Y - 0.272 * I - 0.647 * Q), 0), 255);
                        //blue = (uint)Math.Min(Math.Max((1.0 * Y - 1.105 * I + 1.702 * Q), 0), 255);
                        // No need to waste multiplications with 1.0 or with the Q component we zero out
                        red = (uint)Math.Min((Y + (0.956 * I)), 255);
                        green = (uint)Math.Min(Math.Max((Y - (0.272 * I)), 0), 255);
                        blue = (uint)Math.Min(Math.Max((Y - (1.105 * I)), 0), 255);

                        // Reassembling each component back into a pixel and assigning it to the output location 
                        targetPixels[index] = (alpha << 24) | (red << 16) | (green << 8) | blue;
                    }
                }
            });
        }
    }

}

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
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component


                    // RGB to YIQ
                    uint Y = (uint)Math.Min((0.299 * red + 0.587 * green + 0.114 * blue), 255);
                    uint I = (uint)Math.Min((0.596 * red - 0.274 * green - 0.322 * blue), 255);
                    uint Q = (uint)Math.Min((0.212 * red - 0.523 * green + 0.311 * blue), 255);
                    
                    // Update for Sepia look
                    I = m_Intensity;
                    Q = 0;
                    
                    // YIQ to RGB
                    red = (uint)Math.Min((1.0 * Y + 0.956 * I + 0.621 * Q), 255);
                    green = (uint)Math.Min((1.0 * Y - 0.272 * I - 0.647 * Q), 255);
                    blue = (uint)Math.Min((1.0 * Y - 1.105 * I + 1.702 * Q), 255);


                    // Reassembling each component back into a pixel and assigning it to the output location 
                    targetPixels[index] = 0xff000000 | (red << 16) | (green << 8) | blue;
                }
            });
        }
    }

}

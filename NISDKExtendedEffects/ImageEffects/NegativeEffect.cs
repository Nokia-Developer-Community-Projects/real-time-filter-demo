// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.15  Rob.Kachmar              Initial creation
// 2014.02.03  Rob.Kachmar              Handling static images
// ============================================================================

using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class NegativeEffect : CustomEffectBase
    {
        public NegativeEffect(IImageProvider source)
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
                    // This only works for a realtime feed.  It does not work with static images.
                    // 16-17 FPS with built-in NegativeFilter() and 15 FPS with this technique on Lumia 920
                    //targetPixels[index] = ~sourcePixels[index]; 


                    // 16-17 FPS with built-in NegativeFilter() and 13-14 FPS with this technique on Lumia 920
                    // This techinque will work with both a realtime feed and static images.
                    ///*
                    uint pixel = sourcePixels[index]; // get the current pixel
                    uint alpha = (pixel & 0xff000000) >> 24; // alpha component

                    if (!alpha.Equals(0)) // Only process if it is not transparent
                    {
                        // Flip the bits of the pixel to create the negative effect
                        pixel = ~pixel;

                        // Assign the pixels back by each component so we can ensure the alpha channel 
                        // is at 255. Otherwise the image will not be visible if it is static.
                        targetPixels[index] = 0xff000000 | (pixel & 0x00ff0000) | (pixel & 0x0000ff00) | pixel & 0x000000ff;
                    }
                    //*/
                }
            });
        }
    }
}
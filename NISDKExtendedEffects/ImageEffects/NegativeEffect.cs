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
                    targetPixels[index] = ~sourcePixels[index];
                }
            });
        }
    }
}
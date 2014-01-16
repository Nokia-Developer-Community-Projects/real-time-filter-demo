using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class MirrorEffect : CustomEffectBase
    {
        public enum MirrorType
        {
            Horizontal = 0,
            Vertical = 1
        }

        MirrorType m_MirrorType = MirrorType.Horizontal;

        public MirrorEffect(IImageProvider source, MirrorType mirrorType = MirrorType.Horizontal) : base(source)
        {
            m_MirrorType = mirrorType;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            int rowIndex = 0;
            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                // get the vertical midpoint >>> L = (P / W) >>> V = (L / 2)
                int verticalMidPoint = (sourcePixels.Length / width) / 2;
                // get the horizontal midpoint >>> M = (W / 2)
                int horizontalMidPoint = width / 2;

                if (m_MirrorType.Equals(MirrorType.Vertical))
                {
                    for (int x = 0; x < width; ++x, ++index)
                    {
                        if (rowIndex < verticalMidPoint)
                        {
                            // Just keep the first half of the column as is
                            targetPixels[index] = sourcePixels[index];
                        }
                        else
                        {
                            // Now we start repeating the mirror image from the first half of the column
                            // index - (((i - V) * 2 * W) - 1) 
                            int sourceIndex = index - ((rowIndex - verticalMidPoint) * 2 * width) - 1;
                            if (sourceIndex > 0)
                            {
                                targetPixels[index] = sourcePixels[sourceIndex];
                            }
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < width; ++x, ++index)
                    {
                        if (x < horizontalMidPoint)
                        {
                            // Just keep the first half of the row as is
                            targetPixels[index] = sourcePixels[index];
                        }
                        else
                        {
                            // Now we start repeating the mirror image from the first half of the row
                            // index - (((x - H) * 2) - 1) 
                            int sourceIndex = index - ((x - horizontalMidPoint) * 2) - 1;
                            if (sourceIndex > 0)
                            {
                                targetPixels[index] = sourcePixels[sourceIndex];
                            }                  
                        }
                    }
                }
                rowIndex++;
            });
        }
    }
}

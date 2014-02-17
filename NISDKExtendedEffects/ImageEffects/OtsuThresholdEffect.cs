// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// ============================================================================

using Nokia.Graphics.Imaging;
using Windows.UI;

namespace NISDKExtendedEffects.ImageEffects
{
    public class OtsuThresholdEffect : CustomEffectBase
    {
        public byte Threshold { get; set; }

        public OtsuThresholdEffect(IImageProvider source)
            : base(source, true)
        {
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            float[] vet = new float[256];
            int[] hist = new int[256];

            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; x += 3, index += 3)
                {
                    var p = (byte)sourcePixelRegion.ImagePixels[index];
                    hist[p]++;
                }
            });

            float p1, p2, p12;
            int k;

            for (k = 1; k != 255; k++)
            {
                p1 = Px(0, k, hist);
                p2 = Px(k + 1, 255, hist);
                p12 = p1 * p2;
                if (p12 == 0)
                    p12 = 1;
                float diff = (Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1);
                vet[k] = (float)diff * diff / p12;
                //vet[k] = (float)Math.Pow((Mx(0, k, hist) * p2) - (Mx(k + 1, 255, hist) * p1), 2) / p12;
            }

            Threshold = (byte)findMax(vet, 256);

            uint white = 0xff000000 | (255 << 16) | (255 << 8) | 255;
            uint black = 0xff000000 | (0 << 16) | (0 << 8) | 0;

            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    uint currentPixel = sourcePixelRegion.ImagePixels[index];

                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    if (red < Threshold || green < Threshold || blue < Threshold)
                        sourcePixelRegion.ImagePixels[index] = black;
                    else
                        sourcePixelRegion.ImagePixels[index] = white;
                }
            });
        }

        private float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        private float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            int i;
            for (i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        private int findMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            for (i = 1; i < n - 1; i++)
            {
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }
            }
            return idx;
        }
    }
}
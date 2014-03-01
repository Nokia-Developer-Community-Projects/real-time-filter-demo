// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// 2014.02.28  Engin.Kırmacı            Remove unused property
// ============================================================================

using Nokia.Graphics.Imaging;

namespace NISDKExtendedEffects.ImageEffects
{
    public class SobelEdgeDetection : CustomEffectBase
    {
        public SobelEdgeDetection(IImageProvider source)
            : base(source)
        {
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var width = (int)sourcePixelRegion.Bounds.Width;
            var height = (int)sourcePixelRegion.Bounds.Height;

            uint white = 0xff000000 | (255 << 16) | (255 << 8) | 255;
            uint black = 0xff000000 | (0 << 16) | (0 << 8) | 0;

            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            int[,] allPixR = new int[width, height];
            int[,] allPixG = new int[width, height];
            int[,] allPixB = new int[width, height];

            int limit = 128 * 128;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    allPixR[i, j] = (byte)sourcePixelRegion.ImagePixels[j * width + i];

            int new_rx = 0, new_ry = 0;
            int new_gx = 0, new_gy = 0;
            int new_bx = 0, new_by = 0;
            int rc, gc, bc;
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    new_rx = 0;
                    new_ry = 0;
                    new_gx = 0;
                    new_gy = 0;
                    new_bx = 0;
                    new_by = 0;
                    rc = 0;
                    gc = 0;
                    bc = 0;

                    for (int wi = -1; wi < 2; wi++)
                    {
                        for (int hw = -1; hw < 2; hw++)
                        {
                            rc = allPixR[i + hw, j + wi];
                            new_rx += gx[wi + 1, hw + 1] * rc;
                            new_ry += gy[wi + 1, hw + 1] * rc;

                            gc = allPixG[i + hw, j + wi];
                            new_gx += gx[wi + 1, hw + 1] * gc;
                            new_gy += gy[wi + 1, hw + 1] * gc;

                            bc = allPixB[i + hw, j + wi];
                            new_bx += gx[wi + 1, hw + 1] * bc;
                            new_by += gy[wi + 1, hw + 1] * bc;
                        }
                    }
                    if (new_rx * new_rx + new_ry * new_ry > limit || new_gx * new_gx + new_gy * new_gy > limit || new_bx * new_bx + new_by * new_by > limit)
                        targetPixelRegion.ImagePixels[j * width + i] = white;
                    else
                        targetPixelRegion.ImagePixels[j * width + i] = black;
                }
            }
        }
    }
}
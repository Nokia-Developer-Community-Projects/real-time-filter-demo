using System;
using Nokia.Graphics.Imaging;

namespace NISDKExtendedEffects.ImageEffects
{
    public class PixelationEffect : CustomEffectBase
    {
        private int m_Scale = 1;
        private int m_ProcessEveryNthRow = 1;
        private int m_ProcessEveryNthColumn = 1;
        private int m_RowModuloTarget = 0;
        private int m_ColumnModuloTarget = 0;

        public PixelationEffect(IImageProvider source, int scale = 1) : base(source)
        {
            m_Scale = (scale <= 0) ? 1 : scale; // Protect against divide by zero;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            m_ProcessEveryNthRow = m_Scale;
            m_ProcessEveryNthColumn = m_Scale;
            m_RowModuloTarget = m_ProcessEveryNthRow - 1;
            m_ColumnModuloTarget = m_ProcessEveryNthColumn - 1;

            int rowIndex = 0;
            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                if ((rowIndex % m_ProcessEveryNthRow).Equals(m_RowModuloTarget)) // only process on every other Nth pixel per row
                {
                    for (int x = 0; x < width; ++x, ++index)
                    {
                        if ((x % m_ProcessEveryNthColumn).Equals(m_ColumnModuloTarget)) // only process on every other Nth pixel per column
                        {
                            // Get the center pixel for the given scale we are working with, and manipulate as desired
                            int centerRowOffset = -1 * ((m_ProcessEveryNthRow - 1) / 2);
                            int centerColumnOffset = -1 * ((m_ProcessEveryNthColumn - 1) / 2);
                            uint targetPixel = sourcePixels[FindIndex(rowIndex, x, width, centerRowOffset, centerColumnOffset)];

                            // Get the top left position of the pixel block, given the current scale
                            int topRowOffset = -1 * (m_ProcessEveryNthRow - 1);
                            int leftColumnOffset = -1 * (m_ProcessEveryNthColumn - 1);

                            // Loop from the top left position down to the bottom right, where we stopped to process
                            for (int y1 = topRowOffset; y1 <= 0; y1++)
                            {
                                for (int x1 = leftColumnOffset; x1 <= 0; x1++)
                                {
                                    targetPixels[FindIndex(rowIndex, x, width, y1, x1)] = targetPixel;
                                }
                            }
                        }
                    }
                }
                rowIndex++;
            });
        }

        private int FindIndex(int rowIndex, int columnIndex, int width, int rowOffset, int columnOffset)
        {
            return ((rowIndex + rowOffset) * width) + (columnIndex + columnOffset);
        }
    }
}

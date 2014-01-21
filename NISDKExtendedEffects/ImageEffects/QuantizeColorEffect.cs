using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace NISDKExtendedEffects.ImageEffects
{
    public class QuantizeColorEffect : CustomEffectBase
    {
        public Dictionary<uint, Color> m_AssignedColorCache;
        private bool m_Cache;

        // Save 1 FPS by avoiding this conversion with every pixel
        Color m_DefaultColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);

        int m_LeastDistanceDefault = int.MaxValue;

        private List<Color> m_TargetColors;

        public List<Color> TargetColors
        {
            get
            {
                if (m_TargetColors == null)
                {
                    // Use a basic 16 color pallette if no list is defined
                    // REFERENCE: http://en.wikipedia.org/wiki/Web_colors
                    m_TargetColors = new List<Color>();
                    m_TargetColors.Add(Color.FromArgb(255,0,0,0)); // Black
                    m_TargetColors.Add(Color.FromArgb(255, 0, 0, 128)); // Low Blue (Navy)
                    m_TargetColors.Add(Color.FromArgb(255, 0, 128, 0)); // Low Green (Green)
                    m_TargetColors.Add(Color.FromArgb(255, 0, 128, 128)); // Low Cyan (Teal)
                    m_TargetColors.Add(Color.FromArgb(255, 128, 0, 0)); // Low Red (Maroon)
                    m_TargetColors.Add(Color.FromArgb(255, 128, 0, 128)); // Low Magenta (Purple)
                    m_TargetColors.Add(Color.FromArgb(255, 128, 128, 0)); // Brown (Olive)
                    m_TargetColors.Add(Color.FromArgb(255, 192, 192, 192)); // Light Gray (Silver)
                    m_TargetColors.Add(Color.FromArgb(255, 169, 169, 169)); // Dark Gray (Gray)
                    m_TargetColors.Add(Color.FromArgb(255, 0, 0, 255)); // High Blue (Blue)
                    m_TargetColors.Add(Color.FromArgb(255, 0, 255, 0)); // High Green (Lime)
                    m_TargetColors.Add(Color.FromArgb(255, 0, 255, 255)); // High Cyan (Aqua)
                    m_TargetColors.Add(Color.FromArgb(255, 255, 0, 0)); // High Red (Red)
                    m_TargetColors.Add(Color.FromArgb(255, 255, 0, 255)); // High Magenta (Fuchsia)
                    m_TargetColors.Add(Color.FromArgb(255, 255, 255, 0)); // Yellow
                    m_TargetColors.Add(Color.FromArgb(255, 255, 255, 255)); // White
                }

                return m_TargetColors;
            }

            set
            {
                m_TargetColors = value;
            }
        }

        public QuantizeColorEffect(IImageProvider source, ref Dictionary<uint, Color> assignedColorCache, 
                List<Color> targetColors = null) : base(source)
        {
            TargetColors = (targetColors == null) ? TargetColors : targetColors; // Initialize TargetColors
            m_Cache = (assignedColorCache != null); // If color cache object passed in, then we will cache
            m_AssignedColorCache = assignedColorCache;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    // Assign the Quantized source pixel to the equivalent location in the output image
                    targetPixels[index] = QunatizeColor(sourcePixels[index]); 
                }
            });
        }

        // I basically took this guys approach to the least-squares algorithm and made a few adjustments
        // REFERENCE: http://msdn.microsoft.com/en-us/library/aa479306.aspx
        private uint QunatizeColor(uint pixel)
        {
            Color colorResult = m_DefaultColor;
            bool foundColor = false;
            if (m_Cache)
            {
                foundColor = m_AssignedColorCache.TryGetValue(pixel, out colorResult); // 2 FPS cost
            }

            // This section has a 5 FPS cost with 9 colors :-(
            if (!foundColor)
            {
                int leastDistance = m_LeastDistanceDefault;
                int red = (int)((pixel & 0x00ff0000) >> 16); // red color component
                int green = (int)((pixel & 0x0000ff00) >> 8); // green color component
                int blue = (int)(pixel & 0x000000ff); // blue color component

                // Loop through the entire palette, looking for the closest color match
                foreach (Color paletteColor in m_TargetColors)
                {
                    // Compute the distance from our source color to the palette color
                    int redDistance = paletteColor.R - red;
                    int greenDistance = paletteColor.G - green;
                    int blueDistance = paletteColor.B - blue;

                    int distance = (redDistance * redDistance) +
                                   (greenDistance * greenDistance) +
                                   (blueDistance * blueDistance);

                    // If the color is closer than any other found so far, use it
                    if (distance < leastDistance)
                    {
                        colorResult = paletteColor;
                        leastDistance = distance;

                        // And if it's an exact match, exit the loop
                        if (0 == distance)
                            break;
                    }
                }

                // Cache the color assignment
                if (m_Cache)
                {
                    m_AssignedColorCache.Add(pixel, colorResult);
                }
            }

            //// If close to white, make it black - experimenting with creating a Magic Pen effect
            //if (Math.Min(colorResult.R, Math.Min(colorResult.G, colorResult.B)) >= 255)
            //{
            //    colorResult = m_DefaultColor;
            //}

            // Return the color converted to a uint - 1 FPS cost - slightly faster than FromColor()
            return (uint)((colorResult.A << 24) | (colorResult.R << 16) | (colorResult.G << 8) | (colorResult.B << 0));
        }
    }
}

// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.20  Rob.Kachmar              Initial creation
// 2014.02.13  Rob.Kachmar              Added WebSafe, WebSafeHalf, and X11
//                                      pre-configured palettes
// 2014.02.16  Rob.Kachmar              Eliminating Windows.UI.Color conversions
//                                      to increase performance
// ============================================================================

using NISDKExtendedEffects.Entities;
using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace NISDKExtendedEffects.ImageEffects
{
    public class QuantizeColorEffect : CustomEffectBase
    {
        public enum ColorPalette
        {
            Color16 = 0,
            WebSafe = 1,
            WebSafeHalf = 2,
            X11 = 3
        }

        ColorPalette m_ColorPalette = ColorPalette.Color16;

        public Dictionary<uint, uint> m_AssignedColorCache;
        private bool m_Cache;

        // Save 1 FPS by avoiding this conversion with every pixel
        uint m_DefaultColor = 0xff000000 | (0 << 16) | (0 << 8) | 0;  // Black

        int m_LeastDistanceDefault = int.MaxValue;


        public QuantizeColorEffect(IImageProvider source, ref Dictionary<uint, uint> assignedColorCache,
                List<uint> targetColors = null, ColorPalette colorPalette = ColorPalette.Color16)
            : base(source)
        {
            m_ColorPalette = colorPalette;
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
            if (!pixel.Equals(0)) // Only process if it is not transparent and part of the image
            {
                uint colorResult = m_DefaultColor;
                bool foundColor = false;
                if (m_Cache)
                {
                    foundColor = m_AssignedColorCache.TryGetValue(pixel, out colorResult); // 2 FPS cost
                }

                // This section has a 5 FPS cost with 9 colors :-(
                if (!foundColor)
                {
                    int leastDistance = m_LeastDistanceDefault;
                    uint red = ((pixel & 0x00ff0000) >> 16); // red color component
                    uint green = ((pixel & 0x0000ff00) >> 8); // green color component
                    uint blue = (pixel & 0x000000ff); // blue color component

                    // Loop through the entire palette, looking for the closest color match
                    foreach (uint paletteColor in m_TargetColors)
                    {
                        // Compute the distance from our source color to the palette color
                        uint paletteColorRed = ((paletteColor & 0x00ff0000) >> 16); // red color component
                        uint paletteColorGreen = ((paletteColor & 0x0000ff00) >> 8); // green color component
                        uint paletteColorBlue = (paletteColor & 0x000000ff); // blue color component

                        int redDistance = (int)(paletteColorRed - red);
                        int greenDistance = (int)(paletteColorGreen - green);
                        int blueDistance = (int)(paletteColorBlue - blue);

                        int distance = ((redDistance * redDistance) +
                                        (greenDistance * greenDistance) +
                                        (blueDistance * blueDistance));

                        // If the color is closer than any other found so far, use it
                        if (distance < leastDistance)
                        {
                            colorResult = paletteColor;
                            leastDistance = distance;

                            // And if it's an exact match, exit the loop
                            if (distance.Equals(0))
                                break;
                        }
                    }

                    // Cache the color assignment
                    if (m_Cache)
                    {
                        m_AssignedColorCache.Add(pixel, colorResult);
                    }
                }

                // Return the color converted to a uint - 1 FPS cost - slightly faster than FromColor()
                return colorResult;
            }
            else
            {
                return pixel;
            }
        }

        private uint BitShiftLeftARGB(uint alpha, uint red, uint green, uint blue)
        {
            return (alpha << 24) | (red << 16) | (green << 8) | blue;
        }

        // These members and propertis are at the bottom since we have a long list of pre-configured palettes 
        private List<uint> m_TargetColors;

        public List<uint> TargetColors
        {
            get
            {
                if (m_TargetColors == null)
                {
                    m_TargetColors = new List<uint>();

                    switch (m_ColorPalette)
                    {
                        case ColorPalette.Color16:
                            {
                                // Use a basic 16 color palette
                                // REFERENCE: http://en.wikipedia.org/wiki/Web_colors
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (0 << 8) | 0); // Black
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (0 << 8) | 128); // Low Blue (Navy)
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (128 << 8) | 0); // Low Green (Green)
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (128 << 8) | 128); // Low Cyan (Teal)
                                m_TargetColors.Add(0xff000000 | (128 << 16) | (0 << 8) | 0); // Low Red (Maroon)
                                m_TargetColors.Add(0xff000000 | (128 << 16) | (0 << 8) | 128); // Low Magenta (Purple)
                                m_TargetColors.Add(0xff000000 | (128 << 16) | (128 << 8) | 0); // Brown (Olive)
                                m_TargetColors.Add(0xff000000 | (192 << 16) | (192 << 8) | 192); // Light Gray (Silver)
                                m_TargetColors.Add(0xff000000 | (169 << 16) | (169 << 8) | 169); // Dark Gray (Gray)
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (0 << 8) | 255); // High Blue (Blue)
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (255 << 8) | 0); // High Green (Lime)
                                m_TargetColors.Add(0xff000000 | (0 << 16) | (255 << 8) | 255); // High Cyan (Aqua)
                                m_TargetColors.Add(0xff000000 | (255 << 16) | (0 << 8) | 0); // High Red (Red)
                                m_TargetColors.Add(0xff000000 | (255 << 16) | (0 << 8) | 255); // High Magenta (Fuchsia)
                                m_TargetColors.Add(0xff000000 | (255 << 16) | (255 << 8) | 0); // Yellow
                                m_TargetColors.Add(0xff000000 | (255 << 16) | (255 << 8) | 255); // White
                            }
                            break;

                        case ColorPalette.WebSafe:
                            {
                                // Use a Web Safe color palette
                                // REFERENCE: http://en.wikipedia.org/wiki/Web_colors
                                for (uint R = 0; R < 256; R += 51)
                                {
                                    for (uint G = 0; G < 256; G += 51)
                                    {
                                        for (uint B = 0; B < 256; B += 51)
                                        {
                                            m_TargetColors.Add(0xff000000 | (R << 16) | (G << 8) | B);
                                        }
                                    }
                                }
                            }
                            break;

                        case ColorPalette.WebSafeHalf:
                            {
                                // Use a Web Safe color palette modifed to only contain half the colors
                                // REFERENCE: http://en.wikipedia.org/wiki/Web_colors
                                for (uint R = 51; R < 256; R += 102)
                                {
                                    for (uint G = 51; G < 256; G += 102)
                                    {
                                        for (uint B = 51; B < 256; B += 102)
                                        {
                                            m_TargetColors.Add(0xff000000 | (R << 16) | (G << 8) | B);
                                        }
                                    }
                                }
                            }
                            break;

                        case ColorPalette.X11:
                            {
                                // Use X11 color palette
                                // REFERENCE: http://en.wikipedia.org/wiki/Web_colors
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 192, 203)); // Pink
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 182, 193)); // LightPink
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 105, 180)); // HotPink
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 20, 147)); // DeepPink
                                m_TargetColors.Add(BitShiftLeftARGB(255, 219, 112, 147)); // PaleVioletRed
                                m_TargetColors.Add(BitShiftLeftARGB(255, 199, 21, 133)); // MediumVioletRed
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 160, 122)); // LightSalmon
                                m_TargetColors.Add(BitShiftLeftARGB(255, 250, 128, 114)); // Salmon
                                m_TargetColors.Add(BitShiftLeftARGB(255, 233, 150, 122)); // DarkSalmon
                                m_TargetColors.Add(BitShiftLeftARGB(255, 240, 128, 128)); // LightCoral
                                m_TargetColors.Add(BitShiftLeftARGB(255, 205, 92, 92)); // IndianRed
                                m_TargetColors.Add(BitShiftLeftARGB(255, 220, 20, 60)); // Crimson
                                m_TargetColors.Add(BitShiftLeftARGB(255, 178, 34, 34)); // FireBrick
                                m_TargetColors.Add(BitShiftLeftARGB(255, 139, 0, 0)); // DarkRed
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 0, 0)); // Red
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 69, 0)); // OrangeRed
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 99, 71)); // Tomato
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 127, 80)); // Coral
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 140, 0)); // DarkOrange
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 165, 0)); // Orange
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 215, 0)); // Gold
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 255, 0)); // Yellow
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 255, 224)); // LightYellow
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 250, 205)); // LemonChiffon
                                m_TargetColors.Add(BitShiftLeftARGB(255, 250, 250, 210)); // LightGoldenrodYellow
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 239, 213)); // PapayaWhip
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 228, 181)); // Moccasin
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 218, 185)); // PeachPuff
                                m_TargetColors.Add(BitShiftLeftARGB(255, 238, 232, 170)); // PaleGoldenrod
                                m_TargetColors.Add(BitShiftLeftARGB(255, 240, 230, 140)); // Khaki
                                m_TargetColors.Add(BitShiftLeftARGB(255, 189, 183, 107)); // DarkKhaki
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 248, 220)); // Cornsilk
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 235, 205)); // BlanchedAlmond
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 228, 196)); // Bisque
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 222, 173)); // NavajoWhite
                                m_TargetColors.Add(BitShiftLeftARGB(255, 245, 222, 179)); // Wheat
                                m_TargetColors.Add(BitShiftLeftARGB(255, 222, 184, 135)); // BurlyWood
                                m_TargetColors.Add(BitShiftLeftARGB(255, 210, 180, 140)); // Tan
                                m_TargetColors.Add(BitShiftLeftARGB(255, 188, 143, 143)); // RosyBrown
                                m_TargetColors.Add(BitShiftLeftARGB(255, 244, 164, 96)); // SandyBrown
                                m_TargetColors.Add(BitShiftLeftARGB(255, 218, 165, 32)); // Goldenrod
                                m_TargetColors.Add(BitShiftLeftARGB(255, 184, 134, 11)); // DarkGoldenrod
                                m_TargetColors.Add(BitShiftLeftARGB(255, 205, 133, 63)); // Peru
                                m_TargetColors.Add(BitShiftLeftARGB(255, 210, 105, 30)); // Chocolate
                                m_TargetColors.Add(BitShiftLeftARGB(255, 139, 69, 19)); // SaddleBrown
                                m_TargetColors.Add(BitShiftLeftARGB(255, 160, 82, 45)); // Sienna
                                m_TargetColors.Add(BitShiftLeftARGB(255, 165, 42, 42)); // Brown
                                m_TargetColors.Add(BitShiftLeftARGB(255, 128, 0, 0)); // Maroon
                                m_TargetColors.Add(BitShiftLeftARGB(255, 85, 107, 47)); // DarkOliveGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 128, 128, 0)); // Olive
                                m_TargetColors.Add(BitShiftLeftARGB(255, 107, 142, 35)); // OliveDrab
                                m_TargetColors.Add(BitShiftLeftARGB(255, 154, 205, 50)); // YellowGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 50, 205, 50)); // LimeGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 255, 0)); // Lime
                                m_TargetColors.Add(BitShiftLeftARGB(255, 124, 252, 0)); // LawnGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 127, 255, 0)); // Chartreuse
                                m_TargetColors.Add(BitShiftLeftARGB(255, 173, 255, 47)); // GreenYellow
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 255, 127)); // SpringGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 250, 154)); // MediumSpringGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 144, 238, 144)); // LightGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 152, 251, 152)); // PaleGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 143, 188, 143)); // DarkSeaGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 60, 179, 113)); // MediumSeaGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 46, 139, 87)); // SeaGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 34, 139, 34)); // ForestGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 128, 0)); // Green
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 100, 0)); // DarkGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 102, 205, 170)); // MediumAquamarine
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 255, 255)); // Aqua
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 255, 255)); // Cyan
                                m_TargetColors.Add(BitShiftLeftARGB(255, 224, 255, 255)); // LightCyan
                                m_TargetColors.Add(BitShiftLeftARGB(255, 175, 238, 238)); // PaleTurquoise
                                m_TargetColors.Add(BitShiftLeftARGB(255, 127, 255, 212)); // Aquamarine
                                m_TargetColors.Add(BitShiftLeftARGB(255, 64, 224, 208)); // Turquoise
                                m_TargetColors.Add(BitShiftLeftARGB(255, 72, 209, 204)); // MediumTurquoise
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 206, 209)); // DarkTurquoise
                                m_TargetColors.Add(BitShiftLeftARGB(255, 32, 178, 170)); // LightSeaGreen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 95, 158, 160)); // CadetBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 139, 139)); // DarkCyan
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 128, 128)); // Teal
                                m_TargetColors.Add(BitShiftLeftARGB(255, 176, 196, 222)); // LightSteelBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 176, 224, 230)); // PowderBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 173, 216, 230)); // LightBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 135, 206, 235)); // SkyBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 135, 206, 250)); // LightSkyBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 191, 255)); // DeepSkyBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 30, 144, 255)); // DodgerBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 100, 149, 237)); // CornflowerBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 70, 130, 180)); // SteelBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 65, 105, 225)); // RoyalBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 0, 255)); // Blue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 0, 205)); // MediumBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 0, 139)); // DarkBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 0, 128)); // Navy
                                m_TargetColors.Add(BitShiftLeftARGB(255, 25, 25, 112)); // MidnightBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 230, 230, 250)); // Lavender
                                m_TargetColors.Add(BitShiftLeftARGB(255, 216, 191, 216)); // Thistle
                                m_TargetColors.Add(BitShiftLeftARGB(255, 221, 160, 221)); // Plum
                                m_TargetColors.Add(BitShiftLeftARGB(255, 238, 130, 238)); // Violet
                                m_TargetColors.Add(BitShiftLeftARGB(255, 218, 112, 214)); // Orchid
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 0, 255)); // Fuchsia
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 0, 255)); // Magenta
                                m_TargetColors.Add(BitShiftLeftARGB(255, 186, 85, 211)); // MediumOrchid
                                m_TargetColors.Add(BitShiftLeftARGB(255, 147, 112, 219)); // MediumPurple
                                m_TargetColors.Add(BitShiftLeftARGB(255, 138, 43, 226)); // BlueViolet
                                m_TargetColors.Add(BitShiftLeftARGB(255, 148, 0, 211)); // DarkViolet
                                m_TargetColors.Add(BitShiftLeftARGB(255, 153, 50, 204)); // DarkOrchid
                                m_TargetColors.Add(BitShiftLeftARGB(255, 139, 0, 139)); // DarkMagenta
                                m_TargetColors.Add(BitShiftLeftARGB(255, 128, 0, 128)); // Purple
                                m_TargetColors.Add(BitShiftLeftARGB(255, 75, 0, 130)); // Indigo
                                m_TargetColors.Add(BitShiftLeftARGB(255, 72, 61, 139)); // DarkSlateBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 106, 90, 205)); // SlateBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 123, 104, 238)); // MediumSlateBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 255, 255)); // White
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 250, 250)); // Snow
                                m_TargetColors.Add(BitShiftLeftARGB(255, 240, 255, 240)); // Honeydew
                                m_TargetColors.Add(BitShiftLeftARGB(255, 245, 255, 250)); // MintCream
                                m_TargetColors.Add(BitShiftLeftARGB(255, 240, 255, 255)); // Azure
                                m_TargetColors.Add(BitShiftLeftARGB(255, 240, 248, 255)); // AliceBlue
                                m_TargetColors.Add(BitShiftLeftARGB(255, 248, 248, 255)); // GhostWhite
                                m_TargetColors.Add(BitShiftLeftARGB(255, 245, 245, 245)); // WhiteSmoke
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 245, 238)); // Seashell
                                m_TargetColors.Add(BitShiftLeftARGB(255, 245, 245, 220)); // Beige
                                m_TargetColors.Add(BitShiftLeftARGB(255, 253, 245, 230)); // OldLace
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 250, 240)); // FloralWhite
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 255, 240)); // Ivory
                                m_TargetColors.Add(BitShiftLeftARGB(255, 250, 235, 215)); // AntiqueWhite
                                m_TargetColors.Add(BitShiftLeftARGB(255, 250, 240, 230)); // Linen
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 240, 245)); // LavenderBlush
                                m_TargetColors.Add(BitShiftLeftARGB(255, 255, 228, 225)); // MistyRose
                                m_TargetColors.Add(BitShiftLeftARGB(255, 220, 220, 220)); // Gainsboro
                                m_TargetColors.Add(BitShiftLeftARGB(255, 211, 211, 211)); // LightGray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 192, 192, 192)); // Silver
                                m_TargetColors.Add(BitShiftLeftARGB(255, 169, 169, 169)); // DarkGray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 128, 128, 128)); // Gray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 105, 105, 105)); // DimGray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 119, 136, 153)); // LightSlateGray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 112, 128, 144)); // SlateGray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 47, 79, 79)); // DarkSlateGray
                                m_TargetColors.Add(BitShiftLeftARGB(255, 0, 0, 0)); // Black                            }
                                break;
                            }
                    }
                }

                return m_TargetColors;
            }

            set
            {
                m_TargetColors = value;
            }
        }
    }
}

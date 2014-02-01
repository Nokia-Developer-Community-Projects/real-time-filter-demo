// NOTE: This is still a work in progress...


using Nokia.Graphics.Imaging;
using System;

namespace NISDKExtendedEffects.ImageEffects
{
    public class SepiaEffect : CustomEffectBase
    {
        public SepiaEffect(IImageProvider source) : base(source)
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
                    uint currentPixel = sourcePixels[index]; // get the current pixel
                    uint red = (currentPixel & 0x00ff0000) >> 16; // red color component
                    uint green = (currentPixel & 0x0000ff00) >> 8; // green color component
                    uint blue = currentPixel & 0x000000ff; // blue color component

                    /*
                    // Calculate the weighted avearge of all the color components and assign the result to each component
                    // REFERENCE: http://en.wikipedia.org/wiki/Grayscale
                    uint grayscaleAverage = (uint)(0.2126 * red + 0.7152 * green + 0.0722 * blue);
                    red = green = blue = grayscaleAverage;

                    // Calculate Sepia
                    // http://stackoverflow.com/questions/1061093/how-is-a-sepia-tone-created
                    // https://code.google.com/p/aforge/source/browse/trunk/Sources/Imaging/Filters/Color%20Filters/Sepia.cs
                    // http://www.techrepublic.com/blog/how-do-i/how-do-i-convert-images-to-grayscale-and-sepia-tone-using-c/#.
                    uint iRed = red; uint iGreen = green; uint iBlue = blue;
                    red = (uint)Math.Min((0.3930 * iRed + 0.7690 * iGreen + 0.1890 * iBlue), 255); // Red component of Sepia
                    green = (uint)Math.Min((0.3490 * iRed + 0.6860 * iGreen + 0.1680 * iBlue), 255); // Green component of Sepia
                    blue = (uint)Math.Min((0.2720 * iRed + 0.5340 * iGreen + 0.1310 * iBlue), 255); // Blue component of Sepia

                    red = (uint)Math.Max(0, Math.Min(255, (red + (red * 0.50))));
                    green = (uint)Math.Max(0, Math.Min(255, (green + (green * 0.05))));
                    //blue = (uint)Math.Max(0, Math.Min(255, (blue + (blue * 0.50))));
                    //*/

                    // http://www.aforgenet.com/framework/docs/html/10a0f824-445b-dcae-02ef-349d4057da45.htm
                    // RGB to YIQ
                    uint Y = (uint)Math.Min((0.299 * red + 0.587 * green + 0.114 * blue), 255);
                    uint I = (uint)Math.Min((0.596 * red - 0.274 * green - 0.322 * blue), 255);
                    uint Q = (uint)Math.Min((0.212 * red - 0.523 * green + 0.311 * blue), 255);
                    // Update for Brown Sepia look
                    //I = 51; // Recommended from article, but browner than SDK
                    I = 60;
                    Q = 0;
                    // YIQ to RGB
                    red = (uint)Math.Min((1.0 * Y + 0.956 * I + 0.621 * Q), 255);
                    green = (uint)Math.Min((1.0 * Y - 0.272 * I - 0.647 * Q), 255);
                    blue = (uint)Math.Min((1.0 * Y - 1.105 * I + 1.702 * Q), 255);

                    //http://paulbourke.net/texture_colour/colourspace/
                    //Speia R-94 G-38 B-18
                    // Lot's a conversion forumulas, which is a good reference

                    // http://www.google.it/patents/US6937758 // Patents for Sepia???

                    uint newPixel = 0xff000000 | (red << 16) | (green << 8) | blue; // reassembling each component back into a pixel
                    targetPixels[index] = newPixel; // assign the newPixel to the equivalent location in the output image
                }
            });
        }
    }

}

/*
 * //http://paulbourke.net/texture_colour/colourspace/
Appendix: Other standard colour space conversions
RGB -> CIE XYZitu (D65)
   X = 0.431 R + 0.342 G + 0.178 B
   Y = 0.222 R + 0.707 G + 0.071 B
   Z = 0.020 R + 0.130 G + 0.939 B
CIE XYZitu (D65) -> RGB
   R =  3.063 X - 1.393 Y - 0.476 Z
   G = -0.969 X + 1.876 Y + 0.042 Z
   B =  0.068 X - 0.229 Y + 1.069 Z
RGB -> CIE XYZrec601-1 (C illuminant)
   X = 0.607 R + 0.174 G + 0.200 B
   Y = 0.299 R + 0.587 G + 0.114 B
   Z = 0.000 R + 0.066 G + 1.116 B
CIE XYZrec601-1 (C illuminant) -> RGB
   R =  1.910 X - 0.532 Y - 0.288 Z
   G = -0.985 X + 1.999 Y - 0.028 Z
   B =  0.058 X - 0.118 Y + 0.898 Z
RGB -> CIE XYZccir709 (D65)
    X = 0.412 R + 0.358 G + 0.180 B
    Y = 0.213 R + 0.715 G + 0.072 B
    Z = 0.019 R + 0.119 G + 0.950 B
CIE XYZccir709 (D65) -> RGB
   R =  3.241 X - 1.537 Y - 0.499 Z
   G = -0.969 X + 1.876 Y + 0.042 Z
   B =  0.056 X - 0.204 Y + 1.057 Z
PAL television standard
RGB -> YUV
    Y =  0.299 R + 0.587 G + 0.114 B
    U = -0.147 R - 0.289 G + 0.436 B
    V =  0.615 R - 0.515 G - 0.100 B
YUV -> RGB
   R = Y + 0.000 U + 1.140 V
   G = Y - 0.396 U - 0.581 V
   B = Y + 2.029 U + 0.000 V
NTSC television standard
RGB -> YIQ
    Y =  0.299 R + 0.587 G + 0.114 B
    I =  0.596 R - 0.274 G - 0.322 B
    Q =  0.212 R - 0.523 G + 0.311 B
YIQ -> RGB
   R = Y + 0.956 I + 0.621 Q
   G = Y - 0.272 I - 0.647 Q
   B = Y - 1.105 I + 1.702 Q
*/
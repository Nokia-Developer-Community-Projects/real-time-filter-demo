namespace NISDKExtendedEffects.Extensions
{
    static public class UintExtension
    {
        static public byte ToGray(this uint color)
        {
            return (byte)((((color & 0x00ff0000) >> 16) + ((color & 0x0000ff00) >> 8) + color & 0x000000ff) / 3);
        }
    }
}
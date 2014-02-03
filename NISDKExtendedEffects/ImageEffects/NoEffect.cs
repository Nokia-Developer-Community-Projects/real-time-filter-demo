// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.01.15  Rob.Kachmar              Initial creation
// ============================================================================

using Nokia.Graphics.Imaging;

namespace NISDKExtendedEffects.ImageEffects
{
    public class NoEffect : CustomEffectBase
    {
        public NoEffect(IImageProvider source = null) : base(source, true) {}
        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion) {}
    }
}

// ============================================================================
// DATE        AUTHOR                   DESCRIPTION
// ----------  -----------------------  ---------------------------------------
// 2014.02.13  Engin.Kırmacı            Initial creation
// ============================================================================

using NISDKExtendedEffects.Entities;
using Nokia.Graphics.Imaging;
using System.Windows;

namespace NISDKExtendedEffects.ImageEffects
{
    public class QuadTransformation : CustomEffectBase
    {
        private QuadDirection Direction;
        private Size Size;

        public EdgePoints EdgePoints { get; set; }

        public QuadTransformation(IImageProvider source, Size size, QuadDirection direction, EdgePoints edgePoints)
            : base(source)
        {
            Direction = direction;
            Size = size;

            EdgePoints = edgePoints;
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            double xs0, ys0, xs1, ys1, xs2, ys2, xs3, ys3;  // Four corners of the source (a quadrilateral area)
            double xt0, yt0, xt1, yt1, xt2, yt2, xt3, yt3;  // Four corners of the target (a rectangle)
            int view_org_xs, view_org_ys, view_org_xt, view_org_yt;

            // Four corners of the source
            xs0 = EdgePoints.TopLeft.X; ys0 = EdgePoints.TopLeft.Y;
            xs1 = EdgePoints.TopRight.X; ys1 = EdgePoints.TopRight.Y;
            xs2 = EdgePoints.BottomRight.X; ys2 = EdgePoints.BottomRight.Y;
            xs3 = EdgePoints.BottomLeft.X; ys3 = EdgePoints.BottomLeft.Y;

            // Margins (the source will be displayed to the left)
            view_org_xs = 0;
            view_org_ys = 0;

            if (Size.Width > sourcePixelRegion.ImageSize.Width)
                Size.Width = sourcePixelRegion.ImageSize.Width;

            if (Size.Height > sourcePixelRegion.ImageSize.Height)
                Size.Height = sourcePixelRegion.ImageSize.Height;

            // Four corners of the target
            xt0 = 0; yt0 = 0;
            xt1 = Size.Width; yt1 = 0;
            xt2 = Size.Width; yt2 = Size.Height;
            xt3 = 0; yt3 = Size.Height;

            // Margins (the target will be displayed to the right)
            view_org_xt = 0;
            view_org_yt = 0;

            int width = (int)sourcePixelRegion.Bounds.Width;
            int height = (int)sourcePixelRegion.Bounds.Height;

            double xt, yt; // Target (rectangle)
            double xs, ys = 0; // Source (quadrilatreal area)
            uint color;    // Pixel

            // Formula f(x) = ax + b for the edges and diagonals
            double a_top, b_top, a_bottom, b_bottom, a_left, b_left, a_right, b_right;

            // Data for any line between the top and the bottom edge and more or less parallel to them
            double xs_horiz_left, ys_horiz_left, xs_horiz_right, ys_horiz_right;
            double a_horiz, b_horiz;

            // Data for any line between the left and the right edge and more or less parallel to them
            double xs_verti_top, ys_verti_top, xs_verti_bottom, ys_verti_bottom;
            double a_verti, b_verti;

            // Data for perspective
            double perspv_a, perspv_b, perspv_4d05;
            double persph_a, persph_b, persph_4d05;
            double tmp_01;

            // -----------------------------------------------------------------------------
            // Get the equations, f(x) = ax + b, of the four edges of the quadrilateral area
            // (for vertical lines, a = 0 means a = infinity for x = b)
            // -----------------------------------------------------------------------------
            // Top edge
            if (xs1 == xs0) { a_top = 0; b_top = xs1; } // special case of vertical line
            else { a_top = (ys1 - ys0) / (xs1 - xs0); b_top = ys0 - a_top * xs0; }

            // Bottom edge
            if (xs2 == xs3) { a_bottom = 0; b_bottom = xs2; }
            else { a_bottom = (ys2 - ys3) / (xs2 - xs3); b_bottom = ys3 - a_bottom * xs3; }

            // Left edge
            if (xs3 == xs0) { a_left = 0; b_left = xs3; }
            else { a_left = (ys3 - ys0) / (xs3 - xs0); b_left = ys0 - a_left * xs0; }

            // Right edge
            if (xs2 == xs1) { a_right = 0; b_right = xs2; }
            else { a_right = (ys2 - ys1) / (xs2 - xs1); b_right = ys1 - a_right * xs1; }

            // Data for perspective
            perspv_4d05 = ((xs1 - xs0) / (xs2 - xs3)) * 2;
            perspv_a = 2 - perspv_4d05;
            perspv_b = perspv_4d05 - 1;

            persph_4d05 = ((ys3 - ys0) / (ys2 - ys1)) * 2;
            persph_a = 2 - persph_4d05;
            persph_b = persph_4d05 - 1;

            // Loop for each horizontal line
            for (yt = yt0; yt < yt3; yt++)
            {
                // Find the corresponding y on the left edge of the quadrilateral area
                //   - adjust according to the lengths
                ys_horiz_left = (yt * (ys3 - ys0) / (yt3 - yt0));

                if (a_left != a_right)
                { // left edge not parallel to the right edge
                    //   - adjust according to the perspective
                    tmp_01 = (ys_horiz_left) / (ys3 - ys0);
                    ys_horiz_left = (ys3 - ys0) * (tmp_01 * tmp_01 * perspv_a + tmp_01 * perspv_b);
                }

                ys_horiz_left += ys0;

                // Find the corresponding x on the left edge of the quadrilateral area
                if (a_left == 0) xs_horiz_left = b_left;
                else xs_horiz_left = (ys_horiz_left - b_left) / a_left;

                // Find the corresponding of y on the right edge of the quadrilateral area
                //   - adjust according to the lengths
                ys_horiz_right = (yt * (ys2 - ys1) / (yt2 - yt1));

                if (a_left != a_right)
                { // left edge not parallel to the right edge
                    //   - adjust according to the perspective
                    tmp_01 = (ys_horiz_right) / (ys2 - ys1);
                    ys_horiz_right = (ys2 - ys1) * (tmp_01 * tmp_01 * perspv_a + tmp_01 * perspv_b);
                }

                ys_horiz_right += ys1;

                // Find the corresponding x on the left edge of the quadrilateral area
                if (a_right == 0) xs_horiz_right = b_right;
                else xs_horiz_right = (ys_horiz_right - b_right) / a_right;

                // Find the equation of the line joining the points on the left and the right edges
                if (xs_horiz_right == xs_horiz_left) { a_horiz = 0; b_horiz = xs_horiz_right; }
                else
                {
                    a_horiz = (ys_horiz_right - ys_horiz_left) / (xs_horiz_right - xs_horiz_left);
                    b_horiz = ys_horiz_left - a_horiz * xs_horiz_left;
                }

                // Loop for each point in an horizontal line
                for (xt = xt0; xt < xt1; xt++)
                {
                    // Find the corresponding x
                    //   - adjust according to the lengths
                    xs = (xt * (xs_horiz_right - xs_horiz_left) / (xt1 - xt0));
                    xs += xs_horiz_left;

                    // - adjust for perspective

                    // Find the corresponding point on the top edge of the quadrilateral area
                    xs_verti_top = (xs - xs_horiz_left) * (xs1 - xs0) / (xs_horiz_right - xs_horiz_left);

                    if (a_top != a_bottom)
                    { // top edge not parallel to the bottom edge
                        tmp_01 = (xs_verti_top) / (xs1 - xs0);
                        xs_verti_top = (xs1 - xs0) * (tmp_01 * tmp_01 * persph_a + tmp_01 * persph_b);
                    }

                    xs_verti_top += xs0;
                    ys_verti_top = a_top * xs_verti_top + b_top;

                    // Find the corresponding of x on the bottom edge of the quadrilateral area
                    xs_verti_bottom = (xs - xs_horiz_left) * (xs2 - xs3) / (xs_horiz_right - xs_horiz_left);

                    if (a_top != a_bottom)
                    { // top edge not parallel to the bottom edge
                        tmp_01 = (xs_verti_bottom) / (xs2 - xs3);
                        xs_verti_bottom = (xs2 - xs3) * (tmp_01 * tmp_01 * persph_a + tmp_01 * persph_b);
                    }

                    xs_verti_bottom += xs3;
                    ys_verti_bottom = a_bottom * xs_verti_bottom + b_bottom;

                    // Find the equation of the line joining the points on the top and the bottom edges
                    if (xs_verti_top != xs_verti_bottom)
                    {
                        a_verti = (ys_verti_bottom - ys_verti_top) / (xs_verti_bottom - xs_verti_top);
                        b_verti = ys_verti_top - a_verti * xs_verti_top;

                        xs = (ys - b_verti) / a_verti; // new xs
                        // ys = a_horiz * xs + b_horiz;   // adjust ys
                    }

                    // Find the corresponding y with the equation of the line
                    ys = a_horiz * xs + b_horiz;

                    // Copy a pixel
                    switch (Direction)
                    {
                        case QuadDirection.QuadToRect:
                            color = sourcePixelRegion.ImagePixels[((int)(ys) + view_org_ys) * width + ((int)(xs) + view_org_xs)];
                            targetPixelRegion.ImagePixels[((int)(yt + 0.5) + view_org_yt) * width + ((int)(xt + 0.5) + view_org_xt)] = color;
                            break;

                        case QuadDirection.RectToQuad:
                            color = sourcePixelRegion.ImagePixels[((int)(yt) + view_org_yt) * width + ((int)(xt) + view_org_xt)];
                            targetPixelRegion.ImagePixels[((int)(ys + 0.5) + view_org_ys) * width + ((int)(xs + 0.5) + view_org_xs)] = color;
                            break;
                    }
                }
            }
        }
    }
}
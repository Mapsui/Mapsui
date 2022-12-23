using Mapsui.Utilities;

namespace Mapsui.Extensions
{
    public static class ViewportExtensions
    {
        /// <summary>
        /// True if Width and Height are not zero
        /// </summary>
        public static bool HasSize(this IReadOnlyViewport viewport) => 
            !viewport.Width.IsNanOrInfOrZero() && !viewport.Height.IsNanOrInfOrZero();

        /// <summary>
        /// IsRotated is true, when viewport displays map rotated
        /// </summary>
        public static bool IsRotated(this IReadOnlyViewport viewport) => 
            !double.IsNaN(viewport.Rotation) && RotationCalculations.RotationShortestDistance(viewport.Rotation, 0) > Constants.Epsilon;

        /// <summary>
        /// Calculates extent from the viewport
        /// </summary>
        /// <remarks>
        /// This MRect is horizontally and vertically aligned, even if the viewport
        /// is rotated. So this MRect perhaps contain parts, that are not visible.
        /// </remarks>
        public static MRect GetExtent(this IReadOnlyViewport viewport)
        {
            // calculate the window extent 
            var halfSpanX = viewport.Width * viewport.Resolution * 0.5;
            var halfSpanY = viewport.Height * viewport.Resolution * 0.5;
            var minX = viewport.CenterX - halfSpanX;
            var minY = viewport.CenterY - halfSpanY;
            var maxX = viewport.CenterX + halfSpanX;
            var maxY = viewport.CenterY + halfSpanY;

            if (!viewport.IsRotated())
            {
                return new MRect(minX, minY, maxX, maxY);
            }
            else
            {
                var windowExtent = new MQuad
                {
                    BottomLeft = new MPoint(minX, minY),
                    TopLeft = new MPoint(minX, maxY),
                    TopRight = new MPoint(maxX, maxY),
                    BottomRight = new MPoint(maxX, minY)
                };

                // Calculate the extent that will encompass a rotated viewport (slightly larger - used for tiles).
                // Perform rotations on corner offsets and then add them to the Center point.
                return windowExtent.Rotate(-viewport.Rotation, viewport.CenterX, viewport.CenterY).ToBoundingBox();
            }
        }
    }
}

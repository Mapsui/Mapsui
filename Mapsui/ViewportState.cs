// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Extensions;
using Mapsui.Utilities;

namespace Mapsui
{
    public record class ViewportState
    {
        public double CenterX { get; init; }
        public double CenterY { get; init; }
        public double Resolution { get; init; }
        public double Rotation { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public MRect Extent { get; private init; }
        public bool IsRotated { get; private init; }

        public ViewportState(double centerX, double centerY, double resolution, double rotation, double width, double height)
        {
            CenterX = centerX;
            CenterY = centerY;
            Resolution = resolution;
            Rotation = rotation;
            Width = width;
            Height = height;

            // Secondary fields
            IsRotated = !double.IsNaN(Rotation) && Rotation > Constants.Epsilon && Rotation < 360 - Constants.Epsilon;
            if (!IsRotated) Rotation = 0; // If not rotated set _rotation explicitly to exactly 0
            Extent = this.GetExtent();
        }
    }
}

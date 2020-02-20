using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Utilities;
using System;

namespace Mapsui.Styles
{
    public class CustomStyle : IStyle
    {
        /// <summary>
        /// Gets or sets the rotation of the symbol in degrees (clockwise is positive)
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// When true a symbol will rotate along with the rotation of the map.
        /// This is useful if you need to symbolize the direction in which a vehicle
        /// is moving. When the symbol is false it will retain it's position to the
        /// screen.
        /// </summary>
        public bool RotateWithMap { get; set; }

        public double MinVisible { get; set; } = 0;
        public double MaxVisible { get; set; } = double.MaxValue;

        public bool Enabled { get; set; } = true;

        public float Opacity { get; set; }

        public virtual void Render(object canvas, IFeature feature, float mapRotation)
        {
        }
    }
}

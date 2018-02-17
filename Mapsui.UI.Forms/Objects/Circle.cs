using Mapsui.UI.Objects;
using Mapsui.Styles;
using Mapsui.Providers;
using Mapsui.UI.Forms.Extensions;
using System.Runtime.CompilerServices;
using System.Linq;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public sealed class Circle : Drawable
    {
        public static readonly BindableProperty CenterProperty = BindableProperty.Create(nameof(Center), typeof(Position), typeof(Circle), default(Position));
        public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(Distance), typeof(Circle), Distance.FromMeters(1));
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Xamarin.Forms.Color), typeof(Circle), Xamarin.Forms.Color.DarkGray);

        public Circle()
        {
            CreateFeature();
        }

        /// <summary>
        /// Center of circle
        /// </summary>
        public Position Center
        {
            get { return (Position)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Radius of circle in meters
        /// </summary>
        public Distance Radius
        {
            get { return (Distance)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        ///  Color to fill circle with
        /// </summary>
        public Xamarin.Forms.Color FillColor
        {
            get { return (Xamarin.Forms.Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Center):
                    ((Mapsui.Geometries.Point)Feature.Geometry).X = Center.ToMapsui().X;
                    ((Mapsui.Geometries.Point)Feature.Geometry).Y = Center.ToMapsui().Y;
                    break;
                case nameof(Radius):
                    ((SymbolStyle)Feature.Styles.First()).SymbolScale = Radius.Meters / 32.0;
                    break;
                case nameof(FillColor):
                    ((VectorStyle)Feature.Styles.First()).Fill = new Brush(FillColor.ToMapsui());
                    break;
            }
        }

        private object sync = new object();

        private void CreateFeature()
        {
            lock (sync)
            {
                if (Feature == null)
                {
                    // Create a new one
                    Feature = new Feature
                    {
                        Geometry = new Mapsui.Geometries.Point(),
                        ["Label"] = Label,
                    };
                    Feature.Styles.Clear();
                    Feature.Styles.Add(new SymbolStyle
                    {
                        UnitType = UnitType.WorldUnit,
                        SymbolScale = Radius.Meters / 32.0,
                        Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },
                        Fill = new Brush { Color = FillColor.ToMapsui() }
                    });
                }
            }
        }
    }
}
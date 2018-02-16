using Mapsui.UI.Objects;
using Mapsui.Styles;
using System;
using Mapsui.Providers;
using Mapsui.UI.Forms.Extensions;
using System.Runtime.CompilerServices;
using System.Linq;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public sealed class Circle : BindableObject, IFeatureProvider
    {
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));
        public static readonly BindableProperty CenterProperty = BindableProperty.Create(nameof(Center), typeof(Position), typeof(Circle), default(Position));
        public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(Distance), typeof(Circle), Distance.FromMeters(1));
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(Circle), 1f);
        public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(nameof(StrokeColor), typeof(Xamarin.Forms.Color), typeof(Circle), Xamarin.Forms.Color.Black);
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Xamarin.Forms.Color), typeof(Circle), Xamarin.Forms.Color.DarkGray);
        public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(nameof(IsClickable), typeof(bool), typeof(Circle), false);
        public static readonly BindableProperty ZIndexProperty = BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(Circle), 0);

        public Circle()
        {
            CreateFeature();
        }

        /// <summary>
        /// Label of polyline
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public Position Center
        {
            get { return (Position)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public Distance Radius
        {
            get { return (Distance)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public float StrokeWidth
        {
            get { return (float)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        public Xamarin.Forms.Color StrokeColor
        {
            get { return (Xamarin.Forms.Color)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        public Xamarin.Forms.Color FillColor
        {
            get { return (Xamarin.Forms.Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public bool IsClickable
        {
            get { return (bool)GetValue(IsClickableProperty); }
            set { SetValue(IsClickableProperty, value); }
        }

        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
        }

        public object Tag { get; set; }

        private Feature feature;

        public Feature Feature
        {
            get
            {
                return feature;
            }
            set
            {
                if (feature == null || !feature.Equals(value))
                    feature = value;
            }
        }

        public event EventHandler Clicked;

        internal bool SendTap()
        {
            EventHandler handler = Clicked;
            if (handler == null)
                return false;

            handler(this, EventArgs.Empty);
            return true;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Center):
                    ((Mapsui.Geometries.Point)feature.Geometry).X = Center.ToMapsui().X;
                    ((Mapsui.Geometries.Point)feature.Geometry).Y = Center.ToMapsui().Y;
                    break;
                case nameof(Radius):
                    ((SymbolStyle)feature.Styles.First()).SymbolScale = Radius.Meters / 32.0;
                    break;
                case nameof(StrokeWidth):
                    ((VectorStyle)feature.Styles.First()).Line.Width = StrokeWidth;
                    break;
                case nameof(StrokeColor):
                    ((VectorStyle)feature.Styles.First()).Line.Color = StrokeColor.ToMapsui();
                    break;
                case nameof(FillColor):
                    ((VectorStyle)feature.Styles.First()).Fill = new Brush(FillColor.ToMapsui());
                    break;
            }
        }

        private object sync = new object();

        private void CreateFeature()
        {
            lock (sync)
            {
                if (feature == null)
                {
                    // Create a new one
                    feature = new Feature
                    {
                        Geometry = new Mapsui.Geometries.Point(),
                        ["Label"] = Label,
                    };
                    feature.Styles.Clear();
                    feature.Styles.Add(new SymbolStyle
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
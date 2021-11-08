using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public class Polygon : Drawable
    {
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Xamarin.Forms.Color), typeof(Polygon), Xamarin.Forms.Color.DarkGray);

        private readonly ObservableRangeCollection<Position> _positions = new ObservableRangeCollection<Position>();
        private readonly ObservableRangeCollection<Position[]> _holes = new ObservableRangeCollection<Position[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Polygon"/> class.
        /// </summary>
        public Polygon()
        {
            _positions.CollectionChanged += OnPositionsCollectionChanged;
            _holes.CollectionChanged += OnHolesCollectionChanged;

            CreateFeature();
        }

        /// <summary>
        ///  Color to fill circle with
        /// </summary>
        public Xamarin.Forms.Color FillColor
        {
            get => (Xamarin.Forms.Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        /// <summary>
        /// Outer contour of polygon
        /// </summary>
        public IList<Position> Positions => _positions;

        /// <summary>
        /// Holes contained by polygon
        /// </summary>
        public IList<Position[]> Holes => _holes;

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (Feature == null)
                return;

            switch (propertyName)
            {
                case nameof(Positions):
                    ((Geometries.Polygon)Feature.Geometry).ExteriorRing = new LinearRing(Positions.Select(p => p.ToPoint()).ToList());
                    break;
                case nameof(Holes):
                    ((Geometries.Polygon)Feature.Geometry).InteriorRings = Holes.Select(h => new LinearRing(h.Select(p => p.ToPoint()).ToList())).ToList();
                    break;
                case nameof(FillColor):
                    ((VectorStyle)Feature.Styles.First()).Fill = new Styles.Brush(FillColor.ToMapsui());
                    break;
                case nameof(StrokeColor):
                    var outline = ((VectorStyle)Feature.Styles.First()).Outline;
                    if (outline != null)
                        outline.Color = StrokeColor.ToMapsui();
                    break;
                case nameof(StrokeWidth):
                    var pen = ((VectorStyle)Feature.Styles.First()).Outline;
                    if (pen != null)
                        pen.Width = StrokeWidth;
                    break;
            }
        }

        private void OnPositionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Positions));
        }

        private void OnHolesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Holes));
        }

        private readonly object sync = new object();

        private void CreateFeature()
        {
            lock (sync)
            {
                if (Feature == null)
                {
                    // Create a new one
                    Feature = new GeometryFeature
                    {
                        Geometry = new Mapsui.Geometries.Polygon(),
                        ["Label"] = Label,
                    };
                    Feature.Styles.Clear();
                    Feature.Styles.Add(new VectorStyle
                    {
                        Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },
                        Fill = new Styles.Brush { Color = FillColor.ToMapsui() }
                    });
                }
            }
        }
    }
}
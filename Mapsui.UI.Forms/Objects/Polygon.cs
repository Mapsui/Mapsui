using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
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
            get { return (Xamarin.Forms.Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        /// <summary>
        /// Outer contour of polygon
        /// </summary>
        public IList<Position> Positions
        {
            get { return _positions; }
        }

        /// <summary>
        /// Holes contained by polygon
        /// </summary>
        public IList<Position[]> Holes
        {
            get { return _holes; }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Positions):
                    ((Mapsui.Geometries.Polygon)Feature.Geometry).ExteriorRing = new LinearRing(Positions.Select(p => p.ToMapsui()).ToList());
                    break;
                case nameof(Holes):
                    ((Mapsui.Geometries.Polygon)Feature.Geometry).InteriorRings = Holes.Select(h => new LinearRing(h.Select(p => p.ToMapsui()).ToList())).ToList();
                    break;
                case nameof(FillColor):
                    ((VectorStyle)Feature.Styles.First()).Fill = new Brush(FillColor.ToMapsui());
                    break;
                case nameof(StrokeColor):
                    ((VectorStyle)Feature.Styles.First()).Outline.Color = StrokeColor.ToMapsui();
                    break;
                case nameof(StrokeWidth):
                    ((VectorStyle)Feature.Styles.First()).Outline.Width = StrokeWidth;
                    break;
            }
        }

        void OnPositionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Positions));
        }

        void OnHolesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Holes));
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
                        Geometry = new Mapsui.Geometries.Polygon(), 
                        ["Label"] = Label,
                    };
                    Feature.Styles.Clear();
                    Feature.Styles.Add(new VectorStyle
                    {
                        Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },
                        Fill = new Brush { Color = FillColor.ToMapsui() }
                    });
                }
            }
        }
    }
}
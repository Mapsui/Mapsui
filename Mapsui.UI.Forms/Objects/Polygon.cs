using System;
using Mapsui.Styles;
using Mapsui.UI.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

#if __MAUI__
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Mapsui.UI.Maui.Extensions;
using Mapsui.UI.Maui.Utils;

using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using Mapsui.UI.Forms.Extensions;
using Xamarin.Forms;

using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
#endif


#if __MAUI__
namespace Mapsui.UI.Maui
#else
namespace Mapsui.UI.Forms
#endif
{
    public class Polygon : Drawable
    {
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(Polygon), KnownColor.DarkGray);
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
        public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        /// <summary>
        /// Outer contour of polygon
        /// </summary>
        public IList<Position> Positions => _positions;

        /// <summary>
        /// Holes contained by polygon
        /// </summary>
        public IList<Position[]> Holes => _holes;

        private readonly object _sync = new object();

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (Feature == null)
                return;

            switch (propertyName)
            {
                case nameof(Positions):
                    var coordinates = Positions.Select(p => p.ToCoordinate()).ToList();
                    if (coordinates.Count == 1)
                        coordinates.Add(coordinates[0].Copy()); // LineString needs at least two coordinates
                    if (coordinates.Count == 2)
                        coordinates.Add(coordinates[0].Copy()); // LinearRing needs at least three coordinates
                    if (!coordinates.First().Equals2D(coordinates.Last()))
                        coordinates.Add(coordinates[0].Copy()); // LinearRing needs to be 'closed' (first should equal last)
                    Feature.Geometry = new NetTopologySuite.Geometries.Polygon(new LinearRing(coordinates.ToArray()));
                    break;
                case nameof(Holes):
                    if (Feature.Geometry is null)
                        throw new Exception("Geometry can not be null when adding holes");
                    var polygon = (NetTopologySuite.Geometries.Polygon)Feature.Geometry;
                    var holes = Holes.Select(h => h.Select(p => p.ToCoordinate()).ToList()).ToList();
                    foreach (var hole in holes)
                        if (!hole.First().Equals2D(hole.Last()))
                            hole.Add(hole[0].Copy()); // LinearRing needs to be 'closed' (first should equal last)
                    var holesAsLinearRings = holes.Select(h => new LinearRing(h.ToArray())).ToArray();
                    Feature.Geometry = new NetTopologySuite.Geometries.Polygon(new LinearRing(polygon.ExteriorRing.Coordinates), holesAsLinearRings);
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

        private void OnPositionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Positions));
        }

        private void OnHolesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Holes));
        }

        private void CreateFeature()
        {
            lock (_sync)
            {
                if (Feature == null)
                {
                    // Create a new one
                    Feature = new GeometryFeature
                    {
                        ["Label"] = Label
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
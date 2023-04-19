using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Objects;
using NetTopologySuite.Geometries;
using Mapsui.Nts.Extensions;
#if __MAUI__
using Mapsui.UI.Maui.Extensions;
using Mapsui.UI.Maui.Utils;
#else
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Forms.Utils;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

public class Polyline : Drawable
{
    // Todo: Rename, Polyline indicates a MultiLineString but it is a single LineString.

    private readonly ObservableRangeCollection<Position> _positions = new ObservableRangeCollection<Position>();

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Polyline"/> class.
    /// </summary>
    public Polyline()
    {
        _positions.CollectionChanged += OnCollectionChanged;

        CreateFeature();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Polyline"/> class from encoded string as described here
    /// https://developers.google.com/maps/documentation/utilities/polylinealgorithm
    /// </summary>
    /// <param name="encodedPolyline">Encoded polyline</param>
    public Polyline(string encodedPolyline)
    {
        _positions.CollectionChanged += OnCollectionChanged;

        CreateFeature();
        DecodePolyline(encodedPolyline);
    }

    /// <summary>
    /// Positions of line
    /// </summary>
    public IList<Position> Positions => _positions;

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(Positions):
                if (Feature == null)
                    CreateFeature();
                else
                    Feature.Geometry = Positions.Select(p => p.ToCoordinate()).ToLineString();
                break;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Positions));
    }

    private readonly object _sync = new();

    /// <summary>
    /// Create feature
    /// </summary>
    private void CreateFeature()
    {
        lock (_sync)
        {
            if (Feature == null)
            {
                // Create a new one
                Feature = new GeometryFeature
                {
                    Geometry = new LineString(Positions.Select(p => p.ToCoordinate()).ToArray()),
                    ["Label"] = Label,
                };
                Feature.Styles.Clear();
                Feature.Styles.Add(new VectorStyle
                {
                    Line = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },

                });
            }
        }
    }

    /// <summary>
    /// Decode polyline
    /// </summary>
    /// <param name="encodedPolyline">Encoded polyline</param>
    private void DecodePolyline(string encodedPolyline)
    {
        var positions = PolylineConverter.DecodePolyline(encodedPolyline);
        if (positions != null)
            positions.ForEach(o => Positions.Add(o));
    }
}

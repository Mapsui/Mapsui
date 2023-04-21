using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Mapsui.Nts.Extensions;
using Mapsui.Utilities;

#if __MAUI__
namespace Mapsui.UI.Maui;
#elif __UWP__
namespace Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
namespace Mapsui.UI.Android;
#elif __IOS__ && !HAS_UNO_WINUI && !__FORMS__
namespace Mapsui.UI.iOS;
#elif __WINUI__
namespace Mapsui.UI.WinUI;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
namespace Mapsui.UI.Blazor;
#elif __WPF__
namespace Mapsui.UI.Wpf;
#else
namespace Mapsui.UI;
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

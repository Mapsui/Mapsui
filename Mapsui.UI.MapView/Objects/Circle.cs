using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Objects;
using NetTopologySuite.Geometries;

#if __MAUI__
using Mapsui.UI.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Mapsui.UI.Maui;

using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using Xamarin.Forms;
using Mapsui.UI.Forms.Extensions;

using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

public class Circle : Drawable
{
    private Position _center;
    private Distance _radius = Distance.FromMeters(1);
    private double _quality = 3.0;
    private Color _fillColor = KnownColor.DarkGray;
    
    public Circle()
    {
        CreateFeature();
    }

    private readonly object _sync = new();

    /// <summary>
    /// Center of circle
    /// </summary>
    public Position Center
    {
        get => _center;
        set
        {
            if (value.Equals(_center)) return;
            _center = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Radius of circle in meters
    /// </summary>
    public Distance Radius
    {
        get => _radius;
        set
        {
            if (value.Equals(_radius)) return;
            _radius = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Color to fill circle with
    /// </summary>
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            if (value.Equals(_fillColor)) return;
            _fillColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Quality for circle. Determines, how many points used to draw circle. 3 is poorest, 360 best quality.
    /// </summary>
    public double Quality
    {
        get => _quality;
        set
        {
            if (value.Equals(_quality)) return;
            _quality = value;
            OnPropertyChanged();
        }
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(Center):
                UpdateFeature();
                break;
            case nameof(Radius):
                UpdateFeature();
                break;
            case nameof(Quality):
                UpdateFeature();
                break;
            case nameof(FillColor):
                if (Feature != null)
                    ((VectorStyle)Feature.Styles.First()).Fill = new Styles.Brush(FillColor.ToMapsui());
                break;
            case nameof(StrokeWidth):
                if (Feature != null)
                {
                    var outline = ((VectorStyle)Feature.Styles.First()).Outline;
                    if (outline != null)
                        outline.Width = StrokeWidth;
                }

                break;
            case nameof(StrokeColor):
                if (Feature != null)
                {
                    var outline = ((VectorStyle)Feature.Styles.First()).Outline;
                    if (outline != null)
                        outline.Color = StrokeColor.ToMapsui();
                }

                break;
        }
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
                    Outline = new Pen { Width = StrokeWidth, Color = StrokeColor.ToMapsui() },
                    Fill = new Styles.Brush { Color = FillColor.ToMapsui() }
                });
            }
        }
    }

    private void UpdateFeature()
    {
        if (Feature == null)
        {
            // Create a new one
            CreateFeature();
        }

        // Create new circle
        var centerX = Center.ToMapsui().X;
        var centerY = Center.ToMapsui().Y;
        var radius = Radius.Meters / Math.Cos(Center.Latitude / 180.0 * Math.PI);
        var increment = 360.0 / (Quality < 3.0 ? 3.0 : (Quality > 360.0 ? 360.0 : Quality));
        var exteriorRing = new List<Coordinate>();

        for (double angle = 0; angle < 360; angle += increment)
        {
            var angleRad = angle / 180.0 * Math.PI;
            exteriorRing.Add(new Coordinate(radius * Math.Sin(angleRad) + centerX, radius * Math.Cos(angleRad) + centerY));
        }

        exteriorRing.Add(exteriorRing[0].Copy());

        Feature!.Geometry = new NetTopologySuite.Geometries.Polygon(new LinearRing(exteriorRing.ToArray()));
    }
}

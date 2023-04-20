using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Nts;
using Mapsui.Styles;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface ICallout : IDisposable
{
    GeometryFeature Feature { get; }
    bool IsVisible { get; }
    CalloutType Type { get; set; }
    string Title { get; set; }
    IPin Pin { get; }
    string Subtitle { get; set; }
    void HandleCalloutClicked(object? sender, ICalloutClicked calloutArgs);

    /// <summary>
    /// Arrow alignment of Callout
    /// </summary>
    ArrowAlignment ArrowAlignment { get; set; }

    /// <summary>
    /// Width from arrow of Callout
    /// </summary>
    double ArrowWidth { get; set; }

    /// <summary>
    /// Height from arrow of Callout
    /// </summary>
    double ArrowHeight { get; set; }

    /// <summary>
    /// Relative position of anchor of Callout on the side given by <see cref="ArrowAlignment"/>
    /// </summary>
    double ArrowPosition { get; set; }

    /// <summary>
    /// Shadow width around Callout
    /// </summary>
    double ShadowWidth { get; set; }

    /// <summary>
    /// Stroke width of frame around Callout
    /// </summary>
    double StrokeWidth { get; set; }

    /// <summary>
    /// Rotation of Callout around the anchor
    /// </summary>
    double Rotation { get; set; }

    /// <summary>
    /// Rotate Callout with map
    /// </summary>
    bool RotateWithMap { get; set; }

    /// <summary>
    /// Radius of rounded corners of Callout
    /// </summary>
    double RectRadius { get; set; }

    /// <summary>
    /// Space between Title and Subtitle of Callout
    /// </summary>
    double Spacing { get; set; }

    /// <summary>
    /// MaxWidth for Title and Subtitle of Callout
    /// </summary>
    double MaxWidth { get; set; }

    /// <summary>
    /// Is Callout closable by a click on the callout
    /// </summary>
    bool IsClosableByClick { get; set; }

    /// <summary>
    /// Content of Callout
    /// </summary>
    int Content { get; set; }

    /// <summary>
    /// Font name to use rendering title
    /// </summary>
    string TitleFontName { get; set; }

    /// <summary>
    /// Font size to rendering title
    /// </summary>
    double TitleFontSize { get; set; }

    /// <summary>
    /// Font name to use rendering subtitle
    /// </summary>
    string SubtitleFontName { get; set; }

    /// <summary>
    /// Font size to rendering subtitle
    /// </summary>
    double SubtitleFontSize { get; set; }

    public event EventHandler<ICalloutClicked>? CalloutClicked;
}

using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface ICalloutClicked
{
    /// <summary>
    /// Callout that is clicked
    /// </summary>
    ICallout? Callout { get; }

    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    Position Point { get; }

    /// <summary>
    /// Number of taps
    /// </summary>
    int NumOfTaps { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    bool Handled { get; set; }
}

using Mapsui.Styles;
using System;
using System.Collections.Generic;

namespace Mapsui;

public delegate void CoordinateSetter(double x, double y);

/// <summary>
/// Interface for a feature which could be displayed on the map.
/// </summary>
public interface IFeature : ICloneable
{
    /// <summary>
    /// Styles used for this feature
    /// </summary>
    ICollection<IStyle> Styles { get; }

    /// <summary>
    /// Additional data that can be stored under specific keys.
    /// </summary>
    /// <param name="key">Key used to store or retrieve specific data fields.</param>
    /// <returns></returns>
    object? this[string key] { get; set; }

    /// <summary>
    /// Keys used to store information for feature.
    /// </summary>
    IEnumerable<string> Fields { get; }

    /// <summary>
    /// Extent of the feature.
    /// </summary>
    MRect? Extent { get; }

    /// <summary>
    /// Implementation of visitor pattern for coordinates
    /// </summary>
    /// <param name="visit">Function for visiting each coordinate X or Y value</param>
    void CoordinateVisitor(Action<double, double, CoordinateSetter> visit);

    /// <summary>
    /// Unique Id for feature.
    /// </summary>
    long Id => 0;

    /// <summary>
    /// Object to store additional data.
    /// </summary>
    object? Data { get; set; }

    /// <summary>
    /// Function to call whenever something changes in settings of feature.
    /// </summary>
    void Modified() { } // default implementation

    /// <summary>
    /// Function to call if the rendered feature is invalid.
    /// </summary>
    void ClearRenderedGeometry() { } // default implementation
}

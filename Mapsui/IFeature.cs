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
    /// Added informations to this feature
    /// </summary>
    /// <param name="key">Key used to store informations for feature</param>
    /// <returns></returns>
    object? this[string key] { get; set; }
    /// <summary>
    /// Keys used to store informations for feature
    /// </summary>
    IEnumerable<string> Fields { get; }
    /// <summary>
    /// Order of features retrieved from GetFeatures() 
    /// </summary>
    /// <remarks>
    /// Smaller values are retrieved later
    /// </remarks>
    int ZOrder { get; }
    /// <summary>
    /// Extent of the feature
    /// </summary>
    MRect? Extent { get; }
    /// <summary>
    /// Implementation of visitor pattern for coordinates
    /// </summary>
    /// <param name="visit">Function for visiting each coordinate X or Y value</param>
    void CoordinateVisitor(Action<double, double, CoordinateSetter> visit);
    /// <summary>
    /// Unique Id for feature
    /// </summary>
    long Id => 0;
    /// <summary>
    /// Function to call whenever something changes in settings of feature
    /// </summary>
    void Modified() { } // default implementation
    /// <summary>
    /// Function to call if the rendered feature is invalide
    /// </summary>
    void ClearRenderedGeometry() { } // default implementation
}

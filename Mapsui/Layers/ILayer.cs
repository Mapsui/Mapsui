// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Animations;
using Mapsui.Fetcher;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Layers;

/// <summary>
/// Interface for map layers
/// </summary>
public interface ILayer : IAnimatable, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Numerical Id of layer
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets or sets an arbitrary object value that can be used to store custom information about this element
    /// </summary>
    object? Tag { get; set; }

    /// <summary>
    /// Minimum visible zoom level
    /// </summary>
    double MinVisible { get; }

    /// <summary>
    /// Minimum visible zoom level
    /// </summary>
    double MaxVisible { get; }

    /// <summary>
    /// Specifies whether this layer should be rendered or not
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Name of layer
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the MRect of the entire layer. Can be null if there are no features in the layer.
    /// </summary>
    MRect? Extent { get; }

    /// <summary>
    /// Gets or sets rendering style of layer
    /// </summary>
    IStyle? Style { get; set; }

    /// <summary>
    /// Opacity of layer
    /// </summary>
    double Opacity { get; set; }

    /// <summary>
    /// Flag, if layer is busy
    /// </summary>
    bool Busy { get; set; }

    /// <summary>
    /// Get all features in a given MRect for a given resolution
    /// </summary>
    /// <param name="extent">Bounding box</param>
    /// <param name="resolution">Resolution of viewport</param>
    /// <returns></returns>
    IEnumerable<IFeature> GetFeatures(MRect extent, double resolution);

    /// <summary>
    /// Attribution for layer
    /// </summary>
    Hyperlink Attribution { get; }

    /// <summary>
    /// List of native resolutions
    /// </summary>
    IReadOnlyList<double> Resolutions { get; }

    /// <summary>
    /// Indicates if the layer should be taken into account for the GetMapInfo request
    /// </summary>
    bool IsMapInfoLayer { get; set; }

    /// <summary>
    /// Event called when the data within the layer has changed allowing
    /// listeners to react to this.
    /// </summary>
    event DataChangedEventHandler DataChanged;

    /// <summary>
    /// To indicate the data withing the layer has changed. This triggers a DataChanged event.
    /// This is necessary for situations where the layer can not know about changes to it's data
    /// as in the case of editing of a geometry.
    /// </summary>
    void DataHasChanged();
}

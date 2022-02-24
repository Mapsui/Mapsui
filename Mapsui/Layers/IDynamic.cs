// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Fetcher;

namespace Mapsui.Layers;

public interface IDynamic
{
    /// <summary>
    /// Event called when the data within the layer has changed allowing
    /// listeners to react to this.
    /// </summary>
    event DataChangedEventHandler DataChanged;

    /// <summary>
    /// To indicate the data withing the class has changed. This triggers a DataChanged event.
    /// This is necessary for situations where the class itself can not know about changes to it's data.
    /// </summary>
    void DataHasChanged();
}

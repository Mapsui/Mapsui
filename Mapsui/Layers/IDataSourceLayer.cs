using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Providers;

namespace Mapsui.Layers;

public interface ILayerDataSource
{
    public IProvider<IFeature>? DataSource { get; set; }
}

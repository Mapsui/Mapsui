using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Providers;

namespace Mapsui.Layers;

public interface IDataSourceLayer
{
    public IProvider<IFeature>? DataSource { get; set; }
}

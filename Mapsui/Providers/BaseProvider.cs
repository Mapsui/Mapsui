using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Layers;

namespace Mapsui.Providers;
public abstract class BaseProvider : IProvider
{
    // Id of provider and layer need to be unique.
    public int Id { get; } = BaseLayer.NextId();

    public virtual string? CRS { get; set; }
    public abstract MRect? GetExtent();
    public abstract Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo);
}

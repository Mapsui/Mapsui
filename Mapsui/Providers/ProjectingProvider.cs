using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;

namespace Mapsui.Providers;

public class ProjectingProvider : IProvider
{
    private readonly IProvider _provider;
    private readonly IProjection _projection;

    public ProjectingProvider(IProvider provider, IProjection? projection = null)
    {
        _provider = provider;
        _projection = projection ?? ProjectionDefaults.Projection;
    }

    /// <summary>
    /// The CRS of the target. The source CRS will be projected to this target CRS. This should be equal to the
    /// CRS of the Map and the FetchInfo.CRS.
    /// </summary>
    public string? CRS { get; set; }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        if (GetFetchInfo(ref fetchInfo))
            return Enumerable.Empty<IFeature>();

        var features = await _provider.GetFeaturesAsync(fetchInfo);
        return features.Project(_provider.CRS, CRS, _projection);
    }

    private bool GetFetchInfo(ref FetchInfo fetchInfo)
    {
        // Note that the FetchInfo.CRS is ignored in this method. A better solution
        // would be to use the fetchInfo.CRS everywhere, but that would only make 
        // sense if GetExtent would also get a CRS argument. Room for improvement.
        if (fetchInfo.Extent == null) return true;

        var copiedExtent = new MRect(fetchInfo.Extent);

        // throws exception when CRS or _provider.CRS is null (so I don't have to check it here)
        _projection.Project(CRS!, _provider.CRS!, copiedExtent);
        fetchInfo = new FetchInfo(copiedExtent, fetchInfo.Resolution, CRS, fetchInfo.ChangeType);
        return false;
    }

    public MRect? GetExtent()
    {
        if (_provider.GetExtent() == null) return null;
        var extent = _provider.GetExtent()!;

        if (!CrsHelper.IsProjectionNeeded(_provider.CRS, CRS)) return extent;

        if (!CrsHelper.IsCrsProvided(_provider.CRS, CRS))
            throw new NotSupportedException($"CRS is not provided. From CRS: {_provider.CRS}. To CRS {CRS}");

        // This projects the full extent of the source. Usually the full extent of the source does not change,
        // so perhaps this should be calculated just once. Then again, there are probably situations where it does
        // change so a way to refresh this should be possible.
        var copiedExtent = new MRect(extent);
        _projection.Project(_provider.CRS, CRS, copiedExtent);
        return copiedExtent;
    }
}

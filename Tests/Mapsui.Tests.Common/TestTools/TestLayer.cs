using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common.TestTools;

/// <summary>
/// This layer calls the DataSource directly from the GetFeatures method. This should be avoided
/// in a real application because the GetFeatures methods is called in the rendering loop. For 
/// testing this layer is used to generate an image from the data source without having to wait for 
/// the asynchronous data fetch call to finish.
/// </summary>
public class TestLayer : BaseLayer
{
    public IProvider? DataSource { get; set; }
    public string? CRS { get; set; }
    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        if (box == null)
            return Enumerable.Empty<IFeature>();

        var biggerBox = box.Grow(
                SymbolStyle.DefaultWidth * 2 * resolution,
                SymbolStyle.DefaultHeight * 2 * resolution);
        var fetchInfo = new FetchInfo(biggerBox, resolution, CRS);

        if (DataSource is null)
            return Enumerable.Empty<IFeature>();

#pragma warning disable VSTHRD002 // Allow use of .Result for test purposes
        return DataSource.GetFeaturesAsync(fetchInfo).Result;
#pragma warning restore VSTHRD002 // 
    }

    public override MRect? Extent => DataSource?.GetExtent();
}

using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common.TestTools
{
    /// <summary>
    /// This layer calls the DataSource directly from the GetFeatures method. This should be avoided
    /// in a real application because the GetFeatures methods is called in the rendering loop. For 
    /// testing this can be useful when wnat to generate an image from the data source without 
    /// having to wait for the asynchronous data fetch call.
    /// </summary>
    public class TestLayer : BaseLayer, ILayerDataSource<IProvider>
    {
        public IProvider? DataSource { get; set; }
        public string? CRS { get; set; }
        public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
        {
            if (box == null)
                yield break;

            var biggerBox = box.Grow(
                    SymbolStyle.DefaultWidth * 2 * resolution,
                    SymbolStyle.DefaultHeight * 2 * resolution);
            var fetchInfo = new FetchInfo(biggerBox, resolution, CRS);

            if (DataSource is null)
                yield break;

#pragma warning disable VSTHRD002 // Allow use of .Result for test purposes
            foreach (var feature in DataSource.GetFeaturesAsync(fetchInfo).ToListAsync().Result)
                yield return feature;
#pragma warning restore VSTHRD002 // 
        }

        public override MRect? Extent => DataSource?.GetExtent();

        IProvider? ILayerDataSource<IProvider>.DataSource => throw new NotImplementedException();
    }
}

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
    public class TestLayer : BaseLayer, ILayerDataSource<IProvider<IFeature>>
    {
        public IProvider<IFeature>? DataSource { get; set; }
        public string? CRS { get; set; }
        public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
        {
            if (box == null) { return new List<IFeature>(); }

            var biggerBox = box.Grow(
                    SymbolStyle.DefaultWidth * 2 * resolution,
                    SymbolStyle.DefaultHeight * 2 * resolution);
            var fetchInfo = new FetchInfo(biggerBox, resolution, CRS);

            return DataSource?.GetFeatures(fetchInfo) ?? Array.Empty<IFeature>();
        }

        public override MRect? Extent => DataSource?.GetExtent();

        IProvider<IFeature>? ILayerDataSource<IProvider<IFeature>>.DataSource => throw new NotImplementedException();

        public override void RefreshData(FetchInfo fetchInfo)
        {

        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Wpf.Editing.Layers
{
    public class VertexOnlyLayer : BaseLayer
    {
        private readonly ILayer _source;

        public override BoundingBox Envelope => _source.Envelope;

        public VertexOnlyLayer(ILayer source)
        {
            _source = source;
            _source.DataChanged += (sender, args) => OnDataChanged(args);
            Style = new SymbolStyle {SymbolScale = 0.5};
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var features = _source.GetFeaturesInView(box, resolution).ToList();
            foreach (var feature in features)
            {
                if (feature.Geometry is Point || feature.Geometry is MultiPoint) continue; // Points with a vertex on top confuse me
                foreach (var vertices in feature.Geometry.MainVertices())
                {
                    yield return new Feature { Geometry = vertices };
                }
            }
        }

        public override void RefreshData(BoundingBox extent, double resolution, bool majorChange)
        {
            OnDataChanged(new DataChangedEventArgs());
        }
    }
}

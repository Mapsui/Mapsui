﻿using System.Collections.Generic;
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
        private readonly WritableLayer _source;

        public override MRect Envelope => _source.Envelope;

        public VertexOnlyLayer(WritableLayer source)
        {
            _source = source;
            _source.DataChanged += (_, args) => OnDataChanged(args);
            Style = new SymbolStyle { SymbolScale = 0.5 };
        }

        public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
        {
            var features = _source.GetFeatures(box, resolution).Cast<IGeometryFeature>().ToList();
            foreach (var feature in features)
            {
                if (feature.Geometry is Point || feature.Geometry is MultiPoint) continue; // Points with a vertex on top confuse me
                foreach (var vertices in feature.Geometry.MainVertices())
                {
                    yield return new Feature { Geometry = vertices };
                }
            }
        }

        public override void RefreshData(FetchInfo fetchInfo)
        {
            OnDataChanged(new DataChangedEventArgs());
        }
    }
}

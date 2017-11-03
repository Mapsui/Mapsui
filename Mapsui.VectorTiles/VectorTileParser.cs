using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile;
using GeoJSON.Net.Feature;
using Mapbox.Vector.Tile;
using Mapsui.Fetcher;
using Mapsui.Styles;
using Mapsui.VectorTiles.Extensions;
using Feature = Mapsui.Providers.Feature;

namespace Mapsui.VectorTiles
{
    
    public class VectorTileParser : ITileParser
    {
        private readonly Random _random = new Random(4967443);

        public IEnumerable<Feature> Parse(TileInfo tileInfo, byte[] tileData)
        {
            var layerInfos = Mapbox.Vector.Tile.VectorTileParser.Parse(new MemoryStream(tileData));
            var featureCollection = layerInfos.Select(i => 
                i.ToGeoJSON(tileInfo.Index.Col, tileInfo.Index.Row, int.Parse(tileInfo.Index.Level)));
            return ToMapsuiFeatures(featureCollection);
        }

        public IEnumerable<Feature> ToMapsuiFeatures(IEnumerable<FeatureCollection> featureCollections)
        {
            var result = new List<Feature>();
            foreach (var featureCollection in featureCollections)
            {
                foreach (var jsonFeature in featureCollection.Features)
                {
                    var geometry = jsonFeature.Geometry.ToMapsui();
                    if (geometry == null) continue; // todo: log
                    var feature = new Feature { Geometry = geometry };
                    feature.Styles.Clear();
                    var color = Color.FromArgb(255, _random.Next(256), _random.Next(256), _random.Next(256));
                    feature.Styles.Add(new VectorStyle { Fill = new Brush(color) });
                    result.Add(feature);
                }
            }
            return result;
        }
    }
}

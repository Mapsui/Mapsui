using Mapsui.Providers;
using System.Collections.Generic;
using Mapsui.Logging;

namespace Mapsui.VectorTiles.Extensions
{
	public static class VectorTileLayerExtensions
	{
		public static IEnumerable<Feature> ToMapsui(this Mapbox.Vector.Tile.VectorTileLayer vectortileLayer, int x, int y, int z)
		{
		    var features = new List<Feature>();

            foreach (var vertorTileFeature in vectortileLayer.VectorTileFeatures)
            {
                var feature = vertorTileFeature.ToMapsui(x,y,z);
                if (feature.Geometry != null)
                {
                    features.Add(feature);
                }
                else
                {
                    Logger.Log(LogLevel.Warning, "Vector tile feature has no geometry");
                }
            }
		    return features;
		}
	}
}


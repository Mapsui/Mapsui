using System;
using BruTile;
using BruTile.Cache;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
#if SILVERLIGHT
using Mapsui.Rendering.Xaml.BitmapRendering;
#endif

namespace Mapsui.UI.Xaml.Layers
{
    public class GroupTileLayer : BaseLayer, ITileLayer
    {
        private IList<TileLayer> _layers = new List<TileLayer>();
        private readonly MemoryCache<Feature> _memoryCache = new MemoryCache<Feature>(200, 300);
        
        public GroupTileLayer(IEnumerable<TileLayer> tileLayers)
        {
            foreach (var tileLayer in tileLayers)
            {
                AddTileLayer(tileLayer);
            }
        }

        public void AddTileLayer(TileLayer layer)
        {
            Layers.Add(layer);
            layer.DataChanged += TileLayerDataChanged;
        }

        private void TileLayerDataChanged(object sender, DataChangedEventArgs e)
        {
            if (Schema == null) return;

            var tiles = new List<MemoryStream>();

            foreach (var tileLayer in Layers)
            {
                if (!tileLayer.Enabled) continue;

                var tile = tileLayer.MemoryCache.Find(e.TileInfo.Index);
                if (tile != null) tiles.Add(((IRaster) tile.Geometry).Data);
            }

            if (tiles.Count == 0) return;
            if (tiles.Count == 1)
            {
                AddBitmapToCache(e, tiles.First()); // If there is 1 tile then omit the rasterization to gain performance.
            }
            else
            {
                var tileWidth = Schema.GetTileWidth(e.TileInfo.Index.Level);
                var tileHeight = Schema.GetTileHeight(e.TileInfo.Index.Level); 
#if SILVERLIGHT
                RunOnUIThread(() => AddBitmapToCache(e, CombineBitmaps(tiles, tileWidth, tileHeight)));
#else
                AddBitmapToCache(e, CombineBitmaps(tiles, tileWidth, tileHeight));
#endif
            }
        }

#if SILVERLIGHT
        private void RunOnUIThread(Action method)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(method);
        }
#endif

        private void AddBitmapToCache(DataChangedEventArgs e, MemoryStream bitmap)
        {
            if (bitmap != null)
                MemoryCache.Add(e.TileInfo.Index,
                    new Feature
                    {
                        Geometry = new Raster(bitmap, e.TileInfo.Extent.ToBoundingBox()),
                        Styles = new List<IStyle> {new VectorStyle()}
                    });
            OnDataChanged(e);
        }
        
        private static MemoryStream CombineBitmaps(IList<MemoryStream> tiles, int width, int height)
        {
            // Eventually the registered renderer should be used to combine the bitmaps. 
            // The GroupTileLayer should be moved to Mapsui core.

            if (tiles.Count == 0) return null;

            if (tiles.Count == 1) return tiles.First(); // If there is 1 tile omit the rasterization to gain performance.

            var canvas = new Canvas();
            foreach (MemoryStream tile in tiles)
            {
                var bitmapImage = new BitmapImage();
#if SILVERLIGHT
                tile.Position = 0;
                var copyOfTile = new MemoryStream(tile.ToArray());
                bitmapImage.SetSource(copyOfTile);
#else
                tile.Position = 0;
                var copyOfTile = new MemoryStream(tile.ToArray()); 
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = copyOfTile;
                bitmapImage.EndInit();
#endif
                var image = new Image {Source = bitmapImage};
                canvas.Children.Add(image);

            }

#if SILVERLIGHT
            return BitmapConverter.ToBitmapStream(canvas, width, height);
#else
            return Rendering.Xaml.BitmapRendering.BitmapConverter.ToBitmapStream(canvas, width, height);
#endif

        }

        public override void AbortFetch()
        {
            foreach (var tileLayer in Layers)
            {
                tileLayer.AbortFetch();
            }
        }

        public override void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            foreach (var tileLayer in Layers)
            {
                tileLayer.ViewChanged(majorChange, extent, resolution);
            }

            if (Schema == null) return;
            var infos = Schema.GetTileInfos(extent.ToExtent(), BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution));
            foreach (var tileInfo in infos)
            {
                if (_memoryCache.Find(tileInfo.Index) == null)
                {
                    TileLayerDataChanged(this, new DataChangedEventArgs(null, false, tileInfo, Name));
                }
            }
        }

        public ITileSchema Schema
        {
            get
            {
                if (Layers.Count > 0) return Layers[0].Schema;
                return null;
            }
        }
        
        public override BoundingBox Envelope
        {
            get
            {
                BoundingBox box = null;
                foreach (var tileLayer in Layers)
                {
                    if (box == null)
                    {
                        if (tileLayer.Envelope != null)
                            box = new BoundingBox(tileLayer.Envelope);
                    }
                    else
                    {
                        box.Join(tileLayer.Envelope);
                    }
                }
                return box;
            }
        }
        
        public MemoryCache<Feature> MemoryCache
        {
            get { return _memoryCache; }
        }

        public IList<TileLayer> Layers
        {
            get { return _layers; }
            set { _layers = value; }
        }

        public override void ClearCache()
        {
            AbortFetch();
            _memoryCache.Clear();
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var dictionary = new Dictionary<TileIndex, IFeature>();

            if (Schema == null) return dictionary.Values;

            var levelId = BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution);
            RenderGetStrategy.GetRecursive(dictionary, Schema, _memoryCache, box.ToExtent(), levelId);
            
            var sortedDictionary = (from entry in dictionary orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            return sortedDictionary.Values;
        }
    }
}

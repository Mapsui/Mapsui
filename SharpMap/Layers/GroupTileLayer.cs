using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BruTile;
using BruTile.Cache;
using SharpMap.Fetcher;
using SharpMap.Geometries;
using SharpMap.Providers;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    public class GroupTileLayer : BaseLayer, ITileLayer, IAsyncDataFetcher
    {
        private IList<TileLayer> layers = new List<TileLayer>();
        private readonly MemoryCache<Feature> memoryCache = new MemoryCache<Feature>(100, 200);
        
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
                if (tile != null) tiles.Add(((IRaster)tile.Geometry).Data);
            }

#if SILVERLIGHT
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
#endif
                     var bitmap = CombineBitmaps(tiles, Schema.Width, Schema.Height);
                     if (bitmap != null) MemoryCache.Add(e.TileInfo.Index, new Feature { Geometry = new Raster(bitmap, e.TileInfo.Extent.ToBoundingBox()), Style = new VectorStyle()});
                     if (DataChanged != null) DataChanged(sender, e);
#if SILVERLIGHT
                });
#endif
        }

        private MemoryStream CombineBitmaps(IList<MemoryStream> tiles, int width, int height)
        {
            if (tiles.Count == 0) return null;

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
            var writeableBitmap = new WriteableBitmap(width, height);
            writeableBitmap.Render(canvas, null);
            writeableBitmap.Invalidate();
            return ConvertToBitmapStream(writeableBitmap);
#else
            canvas.Arrange(new System.Windows.Rect(0, 0, width, height));
            
            var renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, new System.Windows.Media.PixelFormat());
            renderTargetBitmap.Render(canvas);
            var bitmap = new PngBitmapEncoder();
            bitmap.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            var bitmapStream = new MemoryStream();
            bitmap.Save(bitmapStream);
            return bitmapStream;
#endif

        }

#if SILVERLIGHT

        public static MemoryStream ConvertToBitmapStream(WriteableBitmap bitmap)
        {
            var stream = new MemoryStream();

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            var ei = new HackingSilverlightLibrary.EditableImage(width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int pixel = bitmap.Pixels[(i * width) + j];
                    ei.SetPixel(j, i,
                                (byte)((pixel >> 16) & 0xFF),
                                (byte)((pixel >> 8) & 0xFF),
                                (byte)(pixel & 0xFF),
                                (byte)((pixel >> 24) & 0xFF)
                        );
                }
            }
            Stream png = ei.GetStream();
            int len = (int)png.Length;
            byte[] bytes = new byte[len];
            png.Read(bytes, 0, len);
            stream.Write(bytes, 0, len);

            return stream;
        }
#endif

        public void AbortFetch()
        {
            foreach (var tileLayer in Layers)
            {
                tileLayer.AbortFetch();
            }
        }

        public void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            foreach (var tileLayer in Layers)
            {
                tileLayer.ViewChanged(changeEnd, extent, resolution);
            }

            if (Schema == null) return;
            var infos = Schema.GetTilesInView(extent.ToExtent(), BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution));
            foreach (var tileInfo in infos)
            {
                if (memoryCache.Find(tileInfo.Index) == null)
                {
                    TileLayerDataChanged(this, new DataChangedEventArgs(null, false, tileInfo, LayerName));
                }
            }
        }

        public event DataChangedEventHandler DataChanged;

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
            get { return memoryCache; }
        }

        public IList<TileLayer> Layers
        {
            get { return layers; }
            set { layers = value; }
        }

        public void ClearCache()
        {
            AbortFetch();
            memoryCache.Clear();
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var dictionary = new Dictionary<TileIndex, IFeature>();

            if (Schema == null) return dictionary.Values;

            TileLayer.GetRecursive(dictionary, Schema, memoryCache, box.ToExtent(), BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution));
            var sortedDictionary = (from entry in dictionary orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            return sortedDictionary.Values;
        }
    }
}

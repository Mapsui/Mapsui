using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Rendering.XamlRendering
{
    public static class BitmapRenderer
    {
        private static readonly MemoryCache<System.Windows.Shapes.Path> NativeCache = new MemoryCache<System.Windows.Shapes.Path>(150, 200);

        public static void Render(WriteableBitmap bitmap, IViewport viewport, Map map)
        {
            foreach (var layer in map.Layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= viewport.Resolution &&
                    layer.MaxVisible >= viewport.Resolution)
                {
                    RenderLayer(bitmap, viewport, layer);
                }
            }
        }

        private static void RenderLayer(WriteableBitmap bitmap, IViewport viewport, ILayer layer)
        {
            if (layer is ITileLayer)
            {
                var tileLayer = layer as ITileLayer;
                RenderTile(bitmap, tileLayer.Schema, viewport, tileLayer.MemoryCache);
            }
        }

        private static void RenderTile(WriteableBitmap bitmap, ITileSchema schema, IViewport viewport, MemoryCache<Feature> memoryCache)
        {
            var levelId = BruTile.Utilities.GetNearestLevel(schema.Resolutions, viewport.Resolution);
            var tiles = schema.GetTilesInView(viewport.Extent.ToExtent(), levelId);

            foreach (TileInfo tile in tiles)
            {
                var p = NativeCache.Find(tile.Index);
                if (p != null)
                {
                    bitmap.Render(p, null);
                    continue;
                }

                var image = memoryCache.Find(tile.Index);

                if (image != null)
                {
                    Rect dest = WorldToView(tile.Extent, viewport);
                    dest = GeometryRenderer.RoundToPixel(dest);

                    //See here the clumsy way to write a bitmap in SL/WPF
                    var path = new System.Windows.Shapes.Path();
                    path.Data = new RectangleGeometry { Rect = dest };
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(((IRaster)image.Geometry).Data);
                    path.Fill = new ImageBrush { ImageSource = bitmapImage };
                    path.CacheMode = new BitmapCache();
                    bitmap.Render(path, null);
                }
            }
        }

        private static Rect WorldToView(Extent extent, IViewport viewport)
        {
            var min = viewport.WorldToScreen(extent.MinX, extent.MinY);
            var max = viewport.WorldToScreen(extent.MaxX, extent.MaxY);
            return new Rect(min.X, max.Y, max.X - min.X, min.Y - max.Y);
        }
    }
}

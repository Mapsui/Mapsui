using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BruTile;
using BruTile.Cache;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;

namespace SilverlightRendering
{
    public static class BitmapRenderer
    {
        public static MemoryCache<System.Windows.Shapes.Path> nativeCache = new MemoryCache<System.Windows.Shapes.Path>(150, 200);

        public static void Render(WriteableBitmap bitmap, IView view, Map map)
        {
            foreach (var layer in map.Layers)
            {
                if (layer.Enabled &&
                    layer.MinVisible <= view.Resolution &&
                    layer.MaxVisible >= view.Resolution)
                {
                    RenderLayer(bitmap, view, layer);
                }
            }
        }

        private static void RenderLayer(WriteableBitmap bitmap, IView view, ILayer layer)
        {
            if (layer is ITileLayer)
            {
                var tileLayer = layer as ITileLayer;
                RenderTile(bitmap, tileLayer.Schema, view, tileLayer.MemoryCache);
            }
        }

        private static void RenderTile(WriteableBitmap bitmap, ITileSchema schema, IView view, MemoryCache<Feature> memoryCache)
        {
            int level = BruTile.Utilities.GetNearestLevel(schema.Resolutions, view.Resolution);
            var tiles = schema.GetTilesInView(view.Extent.ToExtent(), level);

            foreach (TileInfo tile in tiles)
            {
                var p = nativeCache.Find(tile.Index);
                if (p != null)
                {
                    bitmap.Render(p, null);
                    continue;
                }

                var image = memoryCache.Find(tile.Index);

                if (image != null)
                {
                    Rect dest = WorldToView(tile.Extent, view);
                    dest = GeometryRenderer.RoundToPixel(dest);

                    //See here the clumsy way to write a bitmap in SL/WPF
                    var path = new System.Windows.Shapes.Path();
                    path.Data = new RectangleGeometry() { Rect = dest };
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(((IRaster)image.Geometry).Data);
                    path.Fill = new ImageBrush() { ImageSource = bitmapImage };
                    path.CacheMode = new BitmapCache();
                    bitmap.Render(path, null);
                }
            }
        }

        private static Rect WorldToView(Extent extent, IView view)
        {
            SharpMap.Geometries.Point min = view.WorldToView(extent.MinX, extent.MinY);
            SharpMap.Geometries.Point max = view.WorldToView(extent.MaxX, extent.MaxY);
            return new Rect(min.X, max.Y, max.X - min.X, min.Y - max.Y);
        }
    }
}

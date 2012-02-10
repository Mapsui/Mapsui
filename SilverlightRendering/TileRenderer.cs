// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BruTile;
using BruTile.Cache;
using SharpMap;
using SharpMap.Geometries;
using Path = System.Windows.Shapes.Path;
using SharpMap.Providers;

namespace SilverlightRendering
{
    class TileRenderer
    {
        readonly ITileCache<Rectangle> images = new MemoryCache<Rectangle>(200, 300);

        public void Render(Canvas canvas, ITileSchema schema, IView view, MemoryCache<Feature> memoryCache, double opacity)
        {
            // TODO:
            // Rewrite the way tiles are fetched. Currently drawing and fetching is combined. 
            // Instead all tiles needed for drawing should be fetched with ILayer.GetFeatureInView. 
            // A complication is that higher level tiles (that replace missing
            // tiles at lower levels) need a clip extent. Perhaps the clip can be determine while
            // rendering.
            // Step one would be to split DrawRecursive in a GetRecurcive and DrawTiles

            canvas.Opacity = opacity;
            if (schema == null) return;
            CollapseAll(canvas);
            int level = Utilities.GetNearestLevel(schema.Resolutions, view.Resolution);
            DrawRecursive(canvas, schema, view, memoryCache, view.Extent.ToExtent(), level);
            RemoveCollapsed(canvas);
        }

        private static void CollapseAll(Canvas canvas)
        {
            foreach (UIElement element in canvas.Children)
            {
                if (element is Path) element.Visibility = Visibility.Collapsed;
                else if (element is Rectangle) element.Visibility = Visibility.Collapsed;
            }
        }

        private void DrawRecursive(Canvas canvas, ITileSchema schema, IView view, MemoryCache<Feature> memoryCache, Extent extent, int level)
        {
            IList<TileInfo> tiles = schema.GetTilesInView(extent, level);

            foreach (TileInfo tile in tiles)
            {
                var feature = memoryCache.Find(tile.Index);
                if (feature == null)
                {
                    if (level > 0) DrawRecursive(canvas, schema, view, memoryCache, tile.Extent.Intersect(extent), level - 1);
                }
                else
                {
                    var image = ((Tile)feature.Geometry).Data;
                    Rect dest = WorldToMap(tile.Extent, view);
                    double opacity = DrawImage(canvas, image, dest, tile, memoryCache);
                    if ((opacity < 1) && (level > 0)) DrawRecursive(canvas, schema, view, memoryCache, tile.Extent.Intersect(extent), level - 1);
                }
            }
        }

        private static Rect WorldToMap(Extent extent, IView view)
        {
            SharpMap.Geometries.Point min = view.WorldToView(extent.MinX, extent.MinY);
            SharpMap.Geometries.Point max = view.WorldToView(extent.MaxX, extent.MaxY);
            return new Rect(min.X, max.Y, max.X - min.X, min.Y - max.Y);
        }

        private double DrawImage(Canvas canvas, MemoryStream memoryStream, Rect dest, TileInfo tile, MemoryCache<Feature> memoryCache)
        {
            try
            {
                Rectangle rectangle = images.Find(tile.Index);

                if ((rectangle == null)
                    || (rectangle.Tag == null)
                    || ((int)rectangle.Tag != memoryStream.GetHashCode())) //checking hashcode to assure the tile is not modified
                {
                    rectangle = new Rectangle();
#if SILVERLIGHT
                    rectangle.CacheMode = new BitmapCache();
#endif
                    var source = new BitmapImage();
#if SILVERLIGHT
                    source.SetSource(memoryStream);
#else
                    source.BeginInit();
                    source.StreamSource = memoryStream;
                    source.EndInit();
#endif
                    rectangle.Tag = memoryStream.GetHashCode();

                    var brush = new ImageBrush();
                    brush.ImageSource = source;
                    brush.Stretch = Stretch.Fill;
                    rectangle.Fill = brush;
                    images.Add(tile.Index, rectangle);
                    Panel.SetZIndex(rectangle, int.Parse(tile.Index.LevelId));

//!!!#if !SILVERLIGHT
                    //Note: animation of opacity of tiles used to be a nice feature but
                    //is currently complicated because of the new bitmap buffer architecture
                    //update: removed bitmapbuffer again, not sure where it will end up.
                    rectangle.Opacity = 0;
                    Animate(rectangle, "Opacity", 0, 1, 600, (s, e) => {});
//!!!!#endif
                }

                if (!canvas.Children.Contains(rectangle))
                    canvas.Children.Add(rectangle);

                var destRounded = GeometryRenderer.RoundToPixel(dest);

                Canvas.SetLeft(rectangle, destRounded.X);
                Canvas.SetTop(rectangle, destRounded.Y);
                rectangle.Width = destRounded.Width;
                rectangle.Height = destRounded.Height;

                rectangle.Arrange(new Rect(0, 0, rectangle.Width, rectangle.Height));

                rectangle.Visibility = Visibility.Visible;
                return rectangle.Opacity;

            }
            catch (Exception ex)
            {
                // todo report error
                Console.WriteLine(ex.Message);
                memoryCache.Remove(tile.Index);
                return 0; // tile was not added and should thus be treated as invisible
            }
        }

        public static void Animate(DependencyObject target, string property, double from, double to, int duration, EventHandler completed)
        {
            var animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new TimeSpan(0, 0, 0, 0, duration);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            
            var storyBoard = new Storyboard();
            storyBoard.Children.Add(animation);
            storyBoard.Completed += completed;
            storyBoard.Begin();
        }

        private static void RemoveCollapsed(Canvas canvas)
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement element = canvas.Children[i];
                if (((element is Rectangle) || (element is Path)) && (element.Visibility == Visibility.Collapsed))
                {
                    canvas.Children.Remove(element);
                }
            }
        }
    }
}

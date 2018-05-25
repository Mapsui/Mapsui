using Itinero;
using Itinero.LocalGeo;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Reminiscence;
using Reminiscence.Collections;

namespace Mapsui.Samples.Common.Maps
{
    class ItineroRoutingSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Widgets.Add(new Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Top });
            map.Widgets.Add(new Widgets.Zoom.ZoomInOutWidget(map) { MarginX = 20, MarginY = 40 });
            return map;
        }

        //private ILayer LayerRouteW(Route route)
        //{
            //List<> p = new System.Collections.Generic.List<>();
            //foreach (Coordinate coordinate in route.Shape)
            //{
            //    var spherical = SphericalMercator.FromLonLat(coordinate.Longitude, coordinate.Latitude);
            //    p.Add(new Point(spherical.X, spherical.Y));
            //}
            //LineString ls = new LineString(p);
            //Feature f = new Feature
            //{
            //    Geometry = ls,
            //    ["Name"] = "Line 1",
            //    Styles = new List { new VectorStyle { Line = new Pen(Color.Blue, 6) } }
            //};
            //return new MemoryLayer
            //{
            //    Name = "Route",
            //    DataSource = new MemoryProvider(f),
            //    Style = null
            //};
        //}
    }
}

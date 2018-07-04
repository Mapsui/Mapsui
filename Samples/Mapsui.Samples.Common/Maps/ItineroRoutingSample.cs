using System.Collections.Generic;
using System.IO;
using Itinero;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;

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
            map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Top });
            map.Widgets.Add(new Widgets.Zoom.ZoomInOutWidget { MarginX = 20, MarginY = 40 });
            return map;
        }

        private void DoStuff()
        {
            var routerDb = new RouterDb();
            using (var stream = new FileInfo(@"/Resources/luxembourg-latest.osm.pbf").OpenRead())
            {
                //!!!routerDb.LoadOsmData(stream, Vehicle.Car); // create the network for cars only.
            }

            // create a router.
            var router = new Router(routerDb);

            // get a profile.
            var profile = Vehicle.Car.Fastest(); // the default OSM car profile.

            // create a routerpoint from a location.
            // snaps the given location to the nearest routable edge.
            var start = router.Resolve(profile, 51.26797020271655f, 4.801905155181885f);
            var end = router.Resolve(profile, 51.26797020271655f, 4.801905155181885f);

            // calculate a route.
            var route = router.Calculate(profile, start, end);
        }

        private ILayer LayerRouteW(Route route)
        {
            List<Point> p = new List<Point>();
            foreach (Coordinate coordinate in route.Shape)
            {
                var spherical = SphericalMercator.FromLonLat(coordinate.Longitude, coordinate.Latitude);
                p.Add(new Point(spherical.X, spherical.Y));
            }
            LineString ls = new LineString(p);
            Feature f = new Feature
            {
                Geometry = ls,
                ["Name"] = "Line 1",
                Styles = new List<IStyle> { new VectorStyle { Line = new Pen(Color.Blue, 6) } }
            };
            return new MemoryLayer
            {
                Name = "Route",
                DataSource = new MemoryProvider(f),
                Style = null
            };
        }
    }
}

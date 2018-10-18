using System;
using System.Net;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Providers.Wfs.Utilities;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Desktop
{
    public class WfsSample // not working: ISample
    {
        public string Name => "6 WFS Sample";
        public string Category => "Desktop";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            try
            {
                // WARNING
                // This sample needs the GeoServer WFS running on your local machine. 
                // It uses the GeoServer default sample data. Installing and starting it is all you need to do
                // http://docs.codehaus.org/display/GEOS/Download

                // Sample by Peter Robineau

                const string getCapabilitiesUri = "http://localhost:8080/geoserver/wfs";
                const string serviceUri = "http://localhost:8080/geoserver/wfs";

                var map = new Map();
                map.Layers.Add(CreateStatesLayer(getCapabilitiesUri));
                map.Layers.Add(CreateStatesWithAdvancedFilter(getCapabilitiesUri));
                map.Layers.Add(CreateStatesWithFilterLayer(serviceUri));
                map.Layers.Add(CreateRoadsLayer(getCapabilitiesUri));
                map.Layers.Add(CreateLandmarksLayer(getCapabilitiesUri));
                map.Layers.Add(CreatePoiLayer(getCapabilitiesUri));
                map.Layers.Add(CreateLabelLayer(getCapabilitiesUri));
                return map;

                // todo: create sample for featureTypeInfo:
                // var featureTypeInfo = new WfsFeatureTypeInfo(serviceUri, "topp", null, "states", "the_geom");
                // this does not work:           var statesAndHouseholdsProvider = new WFSProvider(featureTypeInfo, WFSProvider.WFSVersionEnum.WFS_1_1_0);

                // todo create sample for provider.MultiGeometries = false
            }
            catch (WebException ex)
            {
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                if ((ex.Message.Contains("(502) Bad Gateway")) ||
                    (ex.Message.Contains("Unable to connect to the remote server")))
                {
                    throw new Exception(
                        "The Wfs sample threw an exception. You probably need to install the GeoServer WFS to your local machine. You can get it from here: http://docs.codehaus.org/display/GEOS/Download. The exception message was: " +
                        ex.Message);
                }
                throw;
            }
        }

        private static ILayer CreateStatesLayer(string getCapabilitiesUri)
        {
            var statesProvider = CreateStatesProvider(getCapabilitiesUri);

            return new Layer("States")
            {
                Style = new VectorStyle {Fill = new Brush {Color = Color.Red}},
                DataSource = statesProvider
            };
        }

        private static WFSProvider CreateStatesProvider(string getCapabilitiesUri)
        {
            var statesProvider = new WFSProvider(getCapabilitiesUri, "topp", "states",
                WFSProvider.WFSVersionEnum.WFS_1_0_0)
            {
                QuickGeometries = false,
                GetFeatureGetRequest = true,
            };
            return statesProvider;
        }

        private static ILayer CreateStatesWithAdvancedFilter(string getCapabilitiesUri)
        {
            var statesAndHouseholdsProvider = new WFSProvider(getCapabilitiesUri, "topp", "states",
                WFSProvider.WFSVersionEnum.WFS_1_1_0)
            {
                OgcFilter = CreateStatesAndHouseholdsFilter(),
                QuickGeometries = true
            };

            return new Layer("SelectedStatesAndHousholds")
            {
                Style = new VectorStyle {Fill = new Brush {Color = Color.Green}},
                DataSource = statesAndHouseholdsProvider
            };
        }

        private static Layer CreateStatesWithFilterLayer(string serviceUri)
        {
            var newStarProvider = new WFSProvider(serviceUri, "topp", "http://www.openplans.org/topp", "states",
                "the_geom",
                GeometryTypeEnum.MultiSurfacePropertyType, WFSProvider.WFSVersionEnum.WFS_1_1_0)
            {
                OgcFilter = new PropertyIsLikeFilter_FE1_1_0("STATE_NAME", "New*")
            };

            return new Layer("New*")
            {
                Style = new VectorStyle {Fill = new Brush {Color = Color.Yellow}},
                DataSource = newStarProvider
            };
        }

        private static ILayer CreateLandmarksLayer(string getCapabilitiesUri)
        {
            var landmarksProvider = new WFSProvider(getCapabilitiesUri, "tiger", "poly_landmarks",
                WFSProvider.WFSVersionEnum.WFS_1_0_0)
            {
                QuickGeometries = true
            };

            return new Layer("Landmarks")
            {
                Style = new VectorStyle {Fill = new Brush {Color = Color.Blue}},
                DataSource = landmarksProvider
            };
        }

        private static Layer CreateRoadsLayer(string getCapabilitiesUri)
        {
            var roadsProvider = new WFSProvider(getCapabilitiesUri, "tiger", "tiger_roads",
                WFSProvider.WFSVersionEnum.WFS_1_0_0)
            {
                QuickGeometries = true,
                MultiGeometries = false
            };
            return new Layer("Roads") {DataSource = roadsProvider};
        }

        private static ILayer CreatePoiLayer(string getCapabilitiesUri)
        {
            return new Layer("Poi")
            {
                DataSource = new WFSProvider(getCapabilitiesUri, "tiger", "poi",
                    WFSProvider.WFSVersionEnum.WFS_1_0_0)
                {
                    QuickGeometries = true
                }
            };
        }

        private static ILayer CreateLabelLayer(string getCapabilitiesUri)
        {
            // Labels
            // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
            // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
            // If a label property is set, the quick geometry option is automatically set to 'false'.
            const string labelField = "STATE_NAME";
            var provider = CreateStatesProvider(getCapabilitiesUri);
            provider.Label = labelField;

            return new Layer("labels")
            {
                DataSource = provider,
                MaxVisible = 90,
                Style = new LabelStyle
                {
                    CollisionDetection = false,
                    ForeColor = Color.Black,
                    Font = new Font {FontFamily = "GenericSerif", Size = 10},
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    LabelColumn = labelField
                }
            };
        }

        private static OGCFilterCollection CreateStatesAndHouseholdsFilter()
        {
            var californiaAndVermont = new OGCFilterCollection();
            californiaAndVermont.AddFilter(new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "California"));
            californiaAndVermont.AddFilter(new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "Vermont"));
            californiaAndVermont.Junctor = OGCFilterCollection.JunctorEnum.Or;
            IFilter householdSizeFilter = new PropertyIsBetweenFilter_FE1_1_0("HOUSHOLD", "600000", "4000000");
            var stateAndStatesAndHouseholdsFilter = new OGCFilterCollection();
            stateAndStatesAndHouseholdsFilter.AddFilter(householdSizeFilter);
            stateAndStatesAndHouseholdsFilter.AddFilter(californiaAndVermont);
            stateAndStatesAndHouseholdsFilter.Junctor = OGCFilterCollection.JunctorEnum.Or;
            return stateAndStatesAndHouseholdsFilter;
        }
    }
}
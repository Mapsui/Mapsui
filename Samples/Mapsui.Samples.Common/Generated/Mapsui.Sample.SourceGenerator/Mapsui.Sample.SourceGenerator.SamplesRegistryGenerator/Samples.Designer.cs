using System;
namespace Mapsui.Samples.Common
{
    public static class Samples
    {
        public static void Register() 
        {
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Styles.SvgSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.DataFormats.ShapefileSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Editing.EditingSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Geometries.PointsSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Geometries.PolygonGeometrySample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Geometries.LineStringSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Geometries.VariousSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Navigation.KeepCenterInMapSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Performance.ShapefileTileSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Projection.ShapefileProjectionSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Styles.AtlasSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Styles.SelectionStyleSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Styles.SymbolsSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.Demo.BingSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.DataFormats.WmsProjectionTilingSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.DataFormats.GeoJsonSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.DataFormats.GeoTiffSample());
            Mapsui.Samples.Common.AllSamples.Register(new Mapsui.Samples.Common.Maps.DataFormats.MapTilerSample());

        }
    }
}
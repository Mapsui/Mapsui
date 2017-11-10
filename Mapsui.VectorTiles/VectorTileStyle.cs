namespace Mapsui.VectorTiles
{
    /// <summary>
    ///  Generated with paste special in Visual Studion
    /// </summary>
    public class Rootobject
    {
        public int version { get; set; }
        public string name { get; set; }
        public Metadata metadata { get; set; }
        public float[] center { get; set; }
        public float zoom { get; set; }
        public int bearing { get; set; }
        public int pitch { get; set; }
        public Sources sources { get; set; }
        public string glyphs { get; set; }
        public Layer[] layers { get; set; }
        public string id { get; set; }
    }

    public class Metadata
    {
        public bool mapboxautocomposite { get; set; }
        public string mapboxtype { get; set; }
        public string maputnikrenderer { get; set; }
        public string openmaptilesversion { get; set; }
        public string openmaptilesmapboxowner { get; set; }
        public string openmaptilesmapboxsourceurl { get; set; }
    }

    public class Sources
    {
        public Openmaptiles openmaptiles { get; set; }
    }

    public class Openmaptiles
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Layer
    {
        public string id { get; set; }
        public string type { get; set; }
        public Paint paint { get; set; }
        public string source { get; set; }
        public string sourcelayer { get; set; }
        public object[] filter { get; set; }
        public Layout layout { get; set; }
        public int minzoom { get; set; }
        public Metadata1 metadata { get; set; }
        public int maxzoom { get; set; }
    }

    public class Paint
    {
        public string backgroundcolor { get; set; }
        public string fillcolor { get; set; }
        public object fillopacity { get; set; }
        public string linecolor { get; set; }
        public float[] linedasharray { get; set; }
        public LineWidth linewidth { get; set; }
        public object lineopacity { get; set; }
        public FillOutlineColor filloutlinecolor { get; set; }
        public bool fillantialias { get; set; }
        public string textcolor { get; set; }
        public int lineoffset { get; set; }
        public LineGapWidth linegapwidth { get; set; }
        public int texthalowidth { get; set; }
        public string texthalocolor { get; set; }
        public int texthaloblur { get; set; }
    }

    public class LineWidth
    {
        public float _base { get; set; }
        public float[][] stops { get; set; }
    }

    public class FillOutlineColor
    {
        public object[][] stops { get; set; }
    }

    public class LineGapWidth
    {
        public float _base { get; set; }
        public float[][] stops { get; set; }
    }

    public class Layout
    {
        public string visibility { get; set; }
        public string linecap { get; set; }
        public string linejoin { get; set; }
        public string textfield { get; set; }
        public object textsize { get; set; }
        public string[] textfont { get; set; }
        public float[] textoffset { get; set; }
        public int iconsize { get; set; }
        public string textanchor { get; set; }
        public int textmaxwidth { get; set; }
        public string symbolplacement { get; set; }
        public string texttransform { get; set; }
        public float textletterspacing { get; set; }
        public string textrotationalignment { get; set; }
    }

    public class Metadata1
    {
        public string mapboxgroup { get; set; }
    }
}
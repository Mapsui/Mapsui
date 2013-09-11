using System.Globalization;
using BruTile;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BruTile.Web;

namespace WindowsFormsSample
{
    public partial class Form1 : Form
    {
        readonly Bitmap buffer;

        //a list of resolutions in which the tiles are stored
        readonly double[] resolutions = new[] { 
        156543.033900000, 78271.516950000, 39135.758475000, 19567.879237500, 9783.939618750, 
        4891.969809375, 2445.984904688, 1222.992452344, 611.496226172, 305.748113086, 
        152.874056543, 76.437028271, 38.218514136, 19.109257068, 9.554628534, 4.777314267,
        2.388657133, 1.194328567, 0.597164283};

        public Form1()
        {
            InitializeComponent();

            buffer = new Bitmap(Width, Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(buffer, 0, 0);
        }

        private void Form1Load(object sender, EventArgs e)
        {
            var transform = new Transform(new PointF(629816f, 6805085f), 1222.992452344f, Width, Height);

            var schema = CreateTileSchema();
            var tiles = schema.GetTilesInView(transform.Extent, Utilities.GetNearestLevel(schema.Resolutions, transform.Resolution));

            var requestBuilder = new TmsRequest(new Uri("http://a.tile.openstreetmap.org"), "png");

            var graphics = Graphics.FromImage(buffer);
            foreach (var tile in tiles)
            {
                var url = requestBuilder.GetUri(tile);
                byte[] bytes = RequestHelper.FetchImage(url);
                var bitmap = new Bitmap(new MemoryStream(bytes));
                graphics.DrawImage(bitmap, transform.WorldToView(tile.Extent.MinX, tile.Extent.MinY, tile.Extent.MaxX, tile.Extent.MaxY));
            }

            Invalidate();
        }

        private ITileSchema CreateTileSchema()
        {
            var schema = new TileSchema();
            schema.Name = "OpenStreetMap";
            int i = 0;
            foreach (var resolution in resolutions)
            {
                schema.Resolutions[i] = new Resolution {UnitsPerPixel = resolution, Id = i++.ToString(CultureInfo.InvariantCulture)};
            }
        
            schema.OriginX = -20037508.342789;
            schema.OriginY = 20037508.342789;
            schema.Axis = AxisDirection.InvertedY;
            schema.Extent = new Extent(-20037508.342789, -20037508.342789, 20037508.342789, 20037508.342789);
            schema.Height = 256;
            schema.Width = 256;
            schema.Format = "png";
            schema.Srs = "EPSG:900913";
            return schema;
        }



    }
}

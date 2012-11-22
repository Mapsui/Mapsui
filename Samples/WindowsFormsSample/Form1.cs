using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BruTile;

namespace WindowsFormsSample
{
  public partial class Form1 : Form
  {
    Bitmap buffer; 

    //a list of resolutions in which the tiles are stored
    double[] resolutions = new double[] { 
        156543.033900000, 78271.516950000, 39135.758475000, 19567.879237500, 9783.939618750, 
        4891.969809375, 2445.984904688, 1222.992452344, 611.496226172, 305.748113086, 
        152.874056543, 76.437028271, 38.218514136, 19.109257068, 9.554628534, 4.777314267,
        2.388657133, 1.194328567, 0.597164283};
 

    public Form1()
    {
      InitializeComponent();

      buffer = new Bitmap(this.Width, this.Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.DrawImage(buffer, 0, 0);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      Transform transform = new Transform(new PointF(629816f, 6805085f), 1222.992452344f, this.Width, this.Height);
      
      ITileSchema schema = CreateTileSchema();
      IList<TileInfo> tiles = Tile.GetTiles(schema, transform.Extent, transform.Resolution);

      IRequestBuilder requestBuilder = new RequestTms(new Uri("http://a.tile.openstreetmap.org"), "png");

      Graphics graphics = Graphics.FromImage(buffer);
      foreach (TileInfo tile in tiles)
      {
        Uri url = requestBuilder.GetUrl(tile);
        byte[] bytes = ImageRequest.GetImageFromServer(url);
        Bitmap bitmap = new Bitmap(new MemoryStream(bytes));
        graphics.DrawImage(bitmap, transform.WorldToMap(tile.Extent.MinX, tile.Extent.MinY, tile.Extent.MaxX, tile.Extent.MaxY));
      }

      this.Invalidate();
    }
        
    private ITileSchema CreateTileSchema()
    {
      TileSchema schema = new TileSchema();
      schema.Name = "OpenStreetMap";
      foreach (float resolution in resolutions) schema.Resolutions.Add(resolution);
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

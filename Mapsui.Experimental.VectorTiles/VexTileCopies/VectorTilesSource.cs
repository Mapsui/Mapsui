using System;
using NLog;
using System.Threading.Tasks;
using VexTile.Common.Data;
using VexTile.Common.Sources;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.GlobalMercator;
namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public sealed class VectorTilesSource : IVectorTileSource, IBaseTileSource
{
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    private readonly GlobalMercatorImplementation _gmt = new();

    private readonly ITileDataSource _sharedDataSource;

    public GeoExtent? Bounds { get; private set; }

    public CoordinatePair? Center { get; private set; }

    public int MinZoom { get; private set; }

    public int MaxZoom { get; private set; }

    public string? Name { get; private set; }

    public string? Description { get; private set; }

    public string? MbTilesVersion { get; private set; }

    public VectorTilesSource(ITileDataSource dataSource)
    {
        _sharedDataSource = dataSource ?? throw new ArgumentNullException("dataSource");
        LoadMetadata();
    }

    private void LoadMetadata()
    {
        try
        {
            foreach (IMetaData metaDatum in _sharedDataSource.GetMetaData())
            {
                switch (metaDatum.Name.ToLower())
                {
                    case "bounds":
                        {
                            string[] array = metaDatum.Value.Split(',');
                            Bounds = new GeoExtent
                            {
                                West = Convert.ToDouble(array[0]),
                                South = Convert.ToDouble(array[1]),
                                East = Convert.ToDouble(array[2]),
                                North = Convert.ToDouble(array[3])
                            };
                            break;
                        }
                    case "center":
                        {
                            string[] array = metaDatum.Value.Split(',');
                            Center = new CoordinatePair
                            {
                                X = Convert.ToDouble(array[0]),
                                Y = Convert.ToDouble(array[1])
                            };
                            break;
                        }
                    case "minzoom":
                        MinZoom = Convert.ToInt32(metaDatum.Value);
                        break;
                    case "maxzoom":
                        MaxZoom = Convert.ToInt32(metaDatum.Value);
                        break;
                    case "name":
                        Name = metaDatum.Value;
                        break;
                    case "description":
                        Description = metaDatum.Value;
                        break;
                    case "version":
                        MbTilesVersion = metaDatum.Value;
                        break;
                }
            }
        }
        catch (Exception)
        {
            throw new MemberAccessException("Could not load Mbtiles source file");
        }
    }

    private byte[]? GetRawTile(int x, int y, int zoom)
    {
        try
        {
            ITile? tile = _sharedDataSource.GetTile(x, y, zoom);
            if (tile != null)
            {
                return tile.TileData;
            }
        }
        catch
        {
            throw new MemberAccessException("Could not load tile from Mbtiles");
        }

        return null;
    }

    public async Task<VectorTile?> GetVectorTileAsync(int x, int y, int zoom)
    {
        Rect extent = new Rect(0.0, 0.0, 1.0, 1.0);
        bool overZoomed = false;
        if (zoom > MaxZoom)
        {
            GeoExtent geoExtent = _gmt.TileLatLonBounds(x, y, zoom);
            CoordinatePair coordinatePair = new CoordinatePair
            {
                X = geoExtent.East,
                Y = geoExtent.North
            };
            CoordinatePair obj = new CoordinatePair
            {
                X = geoExtent.West,
                Y = geoExtent.North
            };
            CoordinatePair coordinatePair2 = new CoordinatePair
            {
                X = geoExtent.East,
                Y = geoExtent.South
            };
            CoordinatePair coordinatePair3 = new CoordinatePair
            {
                X = geoExtent.West,
                Y = geoExtent.South
            };
            CoordinatePair coordinatePair4 = new CoordinatePair
            {
                X = (coordinatePair.X + coordinatePair3.X) / 2.0,
                Y = (coordinatePair.Y + coordinatePair3.Y) / 2.0
            };
            TileAddress tileAddress = _gmt.LatLonToTile(coordinatePair4.Y, coordinatePair4.X, MaxZoom);
            GeoExtent geoExtent2 = _gmt.TileLatLonBounds(tileAddress.X, tileAddress.Y, MaxZoom);
            double x2 = Utils.ConvertRange(obj.X, geoExtent2.West, geoExtent2.East, 0.0, 1.0);
            double y2 = Utils.ConvertRange(obj.Y, geoExtent2.North, geoExtent2.South, 0.0, 1.0);
            double x3 = Utils.ConvertRange(coordinatePair2.X, geoExtent2.West, geoExtent2.East, 0.0, 1.0);
            double y3 = Utils.ConvertRange(coordinatePair2.Y, geoExtent2.North, geoExtent2.South, 0.0, 1.0);
            extent = new Rect(new Point(x2, y2), new Point(x3, y3));
            x = tileAddress.X;
            y = tileAddress.Y;
            zoom = MaxZoom;
            overZoomed = true;
        }

        try
        {
            VectorTile vectorTile = await GetCachedVectorTileAsync(x, y, zoom);
            if (vectorTile != null)
            {
                vectorTile.IsOverZoomed = overZoomed;
                vectorTile.ApplyExtentInPlace(extent);
                return vectorTile;
            }
        }
        catch (Exception value)
        {
            _log.Error(value);
        }

        return null;
    }

    private async Task<VectorTile?> GetCachedVectorTileAsync(int x, int y, int zoom)
    {
        byte[]? rawTile = GetRawTile(x, y, zoom);
        if (rawTile != null)
        {
            return await new PbfTileSource(rawTile).GetTileAsync();
        }

        return null;
    }

    public Task<byte[]> GetTileAsync(int x, int y, int zoom)
    {
        byte[]? rawTile = GetRawTile(x, y, zoom);
        if (rawTile == null)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        return Task.FromResult(rawTile);
    }
}

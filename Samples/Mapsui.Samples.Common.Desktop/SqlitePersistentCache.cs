using System;
using System.CodeDom.Compiler;
using System.IO;
using BruTile;
using BruTile.Cache;
using SQLite;

namespace Mapsui.Samples.Common.Desktop;

public class SqlitePersistentCache : IPersistentCache<byte[]>
{
    private readonly string _file;

    public SqlitePersistentCache(string name)
    {
        _file = Path.Combine(Path.GetTempPath(), name + ".sqlite");
        InitDb();
    }

    private void InitDb()
    {
        using var connection = CreateConnection();
        try
        {
            var test = connection.Table<Tile>().FirstOrDefault();
        }
        catch (SQLiteException)
        {
            // Table does not exist so i initialize it
            var command = connection.CreateCommand(@"CREATE TABLE Tile (
                Level INTEGER NOT NULL,
                Col INTEGER NOT NULL,
                Row INTEGER NOT NULL,
                Data BLOB,
                PRIMARY KEY (Level, Col, Row)
                );");
            command.ExecuteNonQuery();
        }
    }

    public void Add(TileIndex index, byte[] tile)
    {
        using var connection = CreateConnection();
        var data = new Tile {
            Level = index.Level,
            Col = index.Col,
            Row = index.Row,
            Data = tile,
        };
        connection.Insert(data);
    }

    public void Remove(TileIndex index)
    {
        using var connection = CreateConnection();
        connection.Table<Tile>().Delete(f => f.Level == index.Level && f.Col == index.Col && f.Row == index.Row);
    }

    public byte[]? Find(TileIndex index)
    {
        using var connection = CreateConnection();
        var tile = connection.Table<Tile>().FirstOrDefault(f => f.Level == index.Level && f.Col == index.Col && f.Row == index.Row);
        return tile?.Data;
    }

    private SQLiteConnection CreateConnection()
    {
        return new SQLiteConnection(_file);
    }
}
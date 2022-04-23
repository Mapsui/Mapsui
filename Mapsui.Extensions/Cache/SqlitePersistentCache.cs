using System;
using System.IO;
using BruTile;
using BruTile.Cache;
using Mapsui.Cache;
using SQLite;

namespace Mapsui.Extensions.Cache;

public class SqlitePersistentCache : IPersistentCache<byte[]>, IUrlPersistentCache
{
    private readonly string _file;
    private readonly TimeSpan _cacheExpireTime;

    public SqlitePersistentCache(string name, TimeSpan? cacheExpireTime = null, string? folder = null)
    {
        folder ??= Path.GetTempPath();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _file = Path.Combine(folder, name + ".sqlite");
        _cacheExpireTime = cacheExpireTime ?? TimeSpan.Zero;
        InitDb();
    }

    private void InitDb()
    {
        using var connection = CreateConnection();
        try
        {
            var test = connection.Table<Tile>().FirstOrDefault();
            if (test != null && test.Created == DateTime.MinValue)
            {
                var today = DateTime.Today;
                var command = connection.CreateCommand(@$"Alter TABLE Tile 
                Add Created DateTime NOT NULL Default ('{today.Year}{today.Month:00}{today.Date:00}');");
                command.ExecuteNonQuery();
            }
        }
        catch (SQLiteException)
        {
            // Table does not exist so i initialize it
            var command = connection.CreateCommand(@"CREATE TABLE Tile (
                Level INTEGER NOT NULL,
                Col INTEGER NOT NULL,
                Row INTEGER NOT NULL,
                Created DateTime NOT NULL,
                Data BLOB,
                PRIMARY KEY (Level, Col, Row)
                );");
            command.ExecuteNonQuery();
        }

        try
        {
            var test = connection.Table<UrlCache>().FirstOrDefault();
        }
        catch (SQLiteException)
        {
            // Table does not exist so i initialize it
            var command = connection.CreateCommand(@"CREATE TABLE UrlCache (
                Url TEXT NOT NULL,                
                Created DateTime NOT NULL,
                Data BLOB,
                PRIMARY KEY (Url)
                );");
            command.ExecuteNonQuery();
        }
    }

    public void Add(TileIndex index, byte[] tile)
    {
        using var connection = CreateConnection();
        var data = new Tile
        {
            Level = index.Level,
            Col = index.Col,
            Row = index.Row,
            Created = DateTime.Now,
            Data = tile,
        };
        connection.Insert(data);
    }

    public void Remove(TileIndex index)
    {
        using var connection = CreateConnection();
        connection.Table<Tile>().Delete(f => f.Level == index.Level && f.Col == index.Col && f.Row == index.Row);
    }

    public byte[] Find(TileIndex index)
    {
        using var connection = CreateConnection();
        var tile = connection.Table<Tile>().FirstOrDefault(f => f.Level == index.Level && f.Col == index.Col && f.Row == index.Row);
        if (_cacheExpireTime != TimeSpan.Zero)
        {
            if (tile.Created.Add(_cacheExpireTime) < DateTime.Now)
            {
                // expired
                Remove(index);
                return null!;
            }
        }
        return tile?.Data!;
    }

    public void Add(string url, byte[] tile)
    {
        using var connection = CreateConnection();
        var data = new UrlCache() {
            Url = url,
            Created = DateTime.Now,
            Data = tile,
        };
        connection.Insert(data);
    }

    public void Remove(string url)
    {
        using var connection = CreateConnection();
        connection.Table<UrlCache>().Delete(f => f.Url == url);
    }

    public byte[]? Find(string url)
    {
        using var connection = CreateConnection();
        var tile = connection.Table<UrlCache>().FirstOrDefault(f => f.Url == url);
        if (_cacheExpireTime != TimeSpan.Zero)
        {
            if (tile.Created.Add(_cacheExpireTime) < DateTime.Now)
            {
                // expired
                Remove(url);
                return null;
            }
        }
        return tile?.Data;
    }

    private SQLiteConnection CreateConnection()
    {
        return new SQLiteConnection(_file);
    }
}
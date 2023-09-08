﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using BruTile;
using BruTile.Cache;
using Mapsui.Cache;
using Mapsui.Logging;
using SQLite;

#if NETSTANDARD2_0
using BrotliSharpLib;
#endif

namespace Mapsui.Extensions.Cache;

public class SqlitePersistentCache : IPersistentCache<byte[]>, IUrlPersistentCache
{
    private readonly string _file;
    private readonly TimeSpan _cacheExpireTime;
    private const string NoCompression = "no";
    private const string BrotliCompression = "br";
    private bool _compress;

    public SqlitePersistentCache(string name, TimeSpan? cacheExpireTime = null, string? folder = null, bool compress = true)
    {
        folder ??= Path.GetTempPath();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _file = Path.Combine(folder, name + ".sqlite");
        _cacheExpireTime = cacheExpireTime ?? TimeSpan.Zero;
        _compress = compress;
        InitDb();
    }

    private void InitDb()
    {
        using var connection = CreateConnection();
        try
        {
            if (!TableExists(connection, nameof(Tile)))
            {
                // Table does not exist so i initialize it
                var command = connection.CreateCommand(@"CREATE TABLE Tile (
                Level INTEGER NOT NULL,
                Col INTEGER NOT NULL,
                Row INTEGER NOT NULL,
                Created DateTime NOT NULL,
                Compression VARCHAR(2) NOT NULL,
                Data BLOB,
                PRIMARY KEY (Level, Col, Row)
                );");
                command.ExecuteNonQuery();
            }

            if (!ColumnExists(connection, nameof(Tile), nameof(Tile.Created)))
            {
                var today = DateTime.Today;
                var command = connection.CreateCommand(@$"Alter TABLE Tile 
                Add Created DateTime NOT NULL Default ('{today.Year}{today.Month:00}{today.Date:00}');");
                command.ExecuteNonQuery();
            }

            if (!ColumnExists(connection, nameof(Tile), nameof(Tile.Compression)))
            {
                var command = connection.CreateCommand(@$"Alter TABLE Tile 
                Add Compression VARCHAR(2) NOT NULL Default ('{NoCompression}');");
                command.ExecuteNonQuery();
            }

            if (!TableExists(connection, nameof(UrlCache)))
            {
                var command = connection.CreateCommand(@"CREATE TABLE UrlCache (
                Url TEXT NOT NULL,                
                Created DateTime NOT NULL,
                Compression VARCHAR(2) NOT NULL,
                Data BLOB,
                PRIMARY KEY (Url)
                );");
                command.ExecuteNonQuery();
            }

            if (!ColumnExists(connection, nameof(UrlCache), nameof(UrlCache.Compression)))
            {
                var command = connection.CreateCommand(@$"Alter TABLE UrlCache 
                Add Compression VARCHAR(2) NOT NULL Default ('{NoCompression}');");
                command.ExecuteNonQuery();
            }
        }
        catch (SQLiteException ex)
        {
            Logger.Log(LogLevel.Warning, ex.Message, ex);
        }
    }

    private bool TableExists(SQLiteConnection connection, string table)
    {
        var tableExists = @$"SELECT name FROM sqlite_master
        WHERE type='table' AND name = '{table}'";
        var command = connection.CreateCommand(tableExists);
        var existingTable = command.ExecuteScalar<string>();
        if (string.IsNullOrEmpty(existingTable))
        {
            return false;
        }

        return true;
    }

    private bool ColumnExists(SQLiteConnection connection, string table, string column)
    {
        var tableExists = @$"SELECT             
        p.name as column_name
        FROM 
            sqlite_master AS m
        JOIN 
            pragma_table_info(m.name) AS p
        WHERE m.name = '{table}' AND p.name = '{column}'";
        var command = connection.CreateCommand(tableExists);
        var existigColumn = command.ExecuteScalar<string>();
        if (string.IsNullOrEmpty(existigColumn))
        {
            return false;
        }

        return true;
    }

    public void Add(TileIndex index, byte[] tile)
    {
        var compress = Compress(tile);

        using var connection = CreateConnection();
        var data = new Tile
        {
            Level = index.Level,
            Col = index.Col,
            Row = index.Row,
            Created = DateTime.Now,
            Data = compress.data,
            Compression = compress.Compression,
        };

        connection.Insert(data);
    }

    public void Remove(TileIndex index)
    {
        using var connection = CreateConnection();
        connection.Table<Tile>().Delete(f => f.Level == index.Level && f.Col == index.Col && f.Row == index.Row);
    }

    // Interface Definition in ITileCache is wrong TODO Fix interface in Brutile
#pragma warning disable CS8766
    public byte[]? Find(TileIndex index)
#pragma warning restore CS8766
    {
        using var connection = CreateConnection();
        var tile = connection.Table<Tile>().FirstOrDefault(f => f.Level == index.Level && f.Col == index.Col && f.Row == index.Row);
        if (_cacheExpireTime != TimeSpan.Zero)
        {
            if (tile.Created.Add(_cacheExpireTime) < DateTime.Now)
            {
                // expired
                Remove(index);
                return null;
            }
        }

        var result = Decompress(tile?.Data, tile?.Compression);

        return result;
    }

    public void Add(string url, byte[] tile)
    {
        var compress = Compress(tile);

        using var connection = CreateConnection();
        var data = new UrlCache
        {
            Url = url,
            Created = DateTime.Now,
            Data = compress.data,
            Compression = compress.Compression,
        };
        connection.InsertOrReplace(data);
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

        return Decompress(tile?.Data, tile?.Compression);
    }

    [return: NotNullIfNotNull("bytes")]
    private (byte[]? data, string Compression) Compress(byte[]? bytes)
    {
        if (bytes == null)
            return (null, NoCompression);

        if (_compress)
        {
            try
            {
                using var outputStream = new MemoryStream();
#if NETSTANDARD2_0
                using (var compressStream = new BrotliStream(outputStream, CompressionMode.Compress))
#else
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.Fastest))
#endif
                {
                    compressStream.Write(bytes, 0, bytes.Length);
                }

                var result = outputStream.ToArray();

                if (result.Length < bytes.Length)
                {
                    return (result, BrotliCompression);
                }
            }
            catch (PlatformNotSupportedException)
            {
                // Ignore error and save uncompressed
                // and disable compression
                _compress = false;
            }
            catch (Exception ex)
            {
                // in wasm this seems to throw an exception
                Logger.Log(LogLevel.Error, ex.Message);
            }
        }

        return (bytes, NoCompression);
    }
    
    private byte[]? Decompress(byte[]? bytes, string? compression)
    {
        if (bytes == null || string.IsNullOrEmpty(compression) || string.Equals(compression, NoCompression, StringComparison.InvariantCultureIgnoreCase))
            return bytes;

        switch (compression!.ToLower())
        {
            case BrotliCompression:
                {
                    try
                    {
                        using var inputStream = new MemoryStream(bytes);
                        using var outputStream = new MemoryStream();
                        using (var decompressStream = new BrotliStream(inputStream, CompressionMode.Decompress))
                        {
                            decompressStream.CopyTo(outputStream);
                        }

                        return outputStream.ToArray();
                    }
                    catch (PlatformNotSupportedException)
                    {
                        // Ignore error and save uncompressed
                        // and disable compression
                        _compress = false;
                    }
                    catch (Exception ex)
                    {
                        // in wasm this seems to throw an exception
                        Logger.Log(LogLevel.Error, ex.Message);
                    }

                    return null;
                }
            default:
                throw new NotImplementedException(compression);

        }
    }

    private SQLiteConnection CreateConnection()
    {
        return new SQLiteConnection(_file);
    }

    public void Clear()
    {
        File.Exists(_file);
        {
            File.Delete(_file);
            // initialize the database again
            InitDb();
        }
    }
}

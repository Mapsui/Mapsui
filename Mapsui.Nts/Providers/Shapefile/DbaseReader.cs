// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using Mapsui.Nts.Providers.Shapefile.Indexing;

#pragma warning disable SYSLIB0001

namespace Mapsui.Nts.Providers.Shapefile;

internal sealed class DbaseReader : IDisposable
{
    // Note: Good stuff on DBase format: http://www.clicketyclick.dk/databases/xbase/format/

    private struct DbaseField
    {
        public string ColumnName;
        public Type DataType;
        public int Decimals;
        public int Length;
        // ReSharper disable once NotAccessedField.Local
        public int Address;
    }

    private DateTime _lastUpdate;
    private int _numberOfRecords;
    private short _headerLength;
    private short _recordLength;
    private readonly string _filename;
    private DbaseField[]? _dbaseColumns;
    private FileStream? _fs;
    private BinaryReader? _br;
    private bool _headerIsParsed;

    public DbaseReader(string filename)
    {
        if (!File.Exists(filename))
            throw new FileNotFoundException($"Could not find file \"{filename}\"");
        _filename = filename;
        _headerIsParsed = false;
    }

    private bool _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set => _isOpen = value;
    }

    public void Open()
    {
        _fs?.Dispose();
        _fs = new FileStream(_filename, FileMode.Open, FileAccess.Read);
        _br?.Dispose();
        _br = new BinaryReader(_fs);
        _isOpen = true;
        if (!_headerIsParsed) ParseDbfHeader(); // Don't read the header if it's already parsed
    }

    public void Close()
    {
        _br?.Close();
        _fs?.Close();
        _isOpen = false;
    }

    public void Dispose()
    {
        if (_isOpen)
            Close();
        _br?.Dispose();
        _br = null;
        _fs?.Dispose();
        _fs = null;
    }

    /// <summary>
    /// Indexes a DBF column in a binary tree [NOT COMPLETE]
    /// </summary>
    /// <typeparam name="T">datatype to be indexed</typeparam>
    /// <param name="columnId">Column to index</param>
    /// <returns></returns>
    public BinaryTree<T, uint> CreateDbfIndex<T>(int columnId) where T : IComparable<T?>
    {
        var tree = new BinaryTree<T, uint>();
        for (uint i = 0; i < (_numberOfRecords > 10000 ? 10000 : _numberOfRecords); i++)
            tree.Add(new BinaryTree<T, uint>.ItemValue((T)GetValue(i, columnId), i));
        return tree;
    }

    /// <summary>
    /// Gets the date this file was last updated.
    /// </summary>
    public DateTime LastUpdate => _lastUpdate;

    private void ParseDbfHeader()
    {
        if (_br!.ReadByte() != 0x03)
            throw new NotSupportedException("Unsupported DBF Type");

        _lastUpdate = new DateTime(_br.ReadByte() + 1900, _br.ReadByte(), _br.ReadByte());
        //Read the last update date
        _numberOfRecords = _br.ReadInt32(); // read number of records.
        _headerLength = _br.ReadInt16(); // read length of header structure.
        _recordLength = _br.ReadInt16(); // read length of a record
        _fs!.Seek(29, SeekOrigin.Begin); //Seek to encoding flag
        _fileEncoding = GetDbaseLanguageDriver(_br.ReadByte()); //Read and parse Language driver
        _fs.Seek(32, SeekOrigin.Begin); //Move past the reserved bytes

        var numberOfColumns = (_headerLength - 31) / 32; // calculate the number of DataColumns in the header
        _dbaseColumns = new DbaseField[numberOfColumns];
        for (var i = 0; i < _dbaseColumns.Length; i++)
        {
            _dbaseColumns[i] = new DbaseField
            {
                ColumnName = Encoding.UTF7.GetString(_br.ReadBytes(11)).Replace("\0", "").Trim()
            };
            var fieldtype = _br.ReadChar();
            switch (fieldtype)
            {
                case 'L':
                    _dbaseColumns[i].DataType = typeof(bool);
                    break;
                case 'C':
                    _dbaseColumns[i].DataType = typeof(string);
                    break;
                case 'D':
                    _dbaseColumns[i].DataType = typeof(DateTime);
                    break;
                case 'N':
                    _dbaseColumns[i].DataType = typeof(double);
                    break;
                case 'F':
                    _dbaseColumns[i].DataType = typeof(float);
                    break;
                case 'B':
                    _dbaseColumns[i].DataType = typeof(byte[]);
                    break;
                default:
                    throw new NotSupportedException("Invalid or unknown DBase field type '" + fieldtype +
                                                     "' in column '" + _dbaseColumns[i].ColumnName + "'");
            }
            _dbaseColumns[i].Address = _br.ReadInt32();

            int length = _br.ReadByte();
            if (length < 0) length = length + 256;
            _dbaseColumns[i].Length = length;
            _dbaseColumns[i].Decimals = _br.ReadByte();
            //If the double-type doesn't have any decimals, make the type an integer
            if (_dbaseColumns[i].Decimals == 0 && _dbaseColumns[i].DataType == typeof(double))
                if (_dbaseColumns[i].Length <= 2)
                    _dbaseColumns[i].DataType = typeof(short);
                else if (_dbaseColumns[i].Length <= 4)
                    _dbaseColumns[i].DataType = typeof(int);
                else
                    _dbaseColumns[i].DataType = typeof(long);
            _fs.Seek(_fs.Position + 14, 0);
        }
        _headerIsParsed = true;
    }

    // ReSharper disable once CyclomaticComplexity // It's a switch statement!
    private static Encoding GetDbaseLanguageDriver(byte dbasecode)
    {
        switch (dbasecode)
        {
            case 0x01:
                return Encoding.GetEncoding(437); //DOS USA code page 437 
            case 0x02:
                return Encoding.GetEncoding(850); // DOS Multilingual code page 850 
            case 0x03:
                return Encoding.GetEncoding(1252); // Windows ANSI code page 1252 
            case 0x04:
                return Encoding.GetEncoding(10000); // Standard Macintosh 
            case 0x08:
                return Encoding.GetEncoding(865); // Danish OEM
            case 0x09:
                return Encoding.GetEncoding(437); // Dutch OEM
            case 0x0A:
                return Encoding.GetEncoding(850); // Dutch OEM Secondary codepage
            case 0x0B:
                return Encoding.GetEncoding(437); // Finnish OEM
            case 0x0D:
                return Encoding.GetEncoding(437); // French OEM
            case 0x0E:
                return Encoding.GetEncoding(850); // French OEM Secondary codepage
            case 0x0F:
                return Encoding.GetEncoding(437); // German OEM
            case 0x10:
                return Encoding.GetEncoding(850); // German OEM Secondary codepage
            case 0x11:
                return Encoding.GetEncoding(437); // Italian OEM
            case 0x12:
                return Encoding.GetEncoding(850); // Italian OEM Secondary codepage
            case 0x13:
                return Encoding.GetEncoding(932); // Japanese Shift-JIS
            case 0x14:
                return Encoding.GetEncoding(850); // Spanish OEM secondary codepage
            case 0x15:
                return Encoding.GetEncoding(437); // Swedish OEM
            case 0x16:
                return Encoding.GetEncoding(850); // Swedish OEM secondary codepage
            case 0x17:
                return Encoding.GetEncoding(865); // Norwegian OEM
            case 0x18:
                return Encoding.GetEncoding(437); // Spanish OEM
            case 0x19:
                return Encoding.GetEncoding(437); // English OEM (Britain)
            case 0x1A:
                return Encoding.GetEncoding(850); // English OEM (Britain) secondary codepage
            case 0x1B:
                return Encoding.GetEncoding(437); // English OEM (U.S.)
            case 0x1C:
                return Encoding.GetEncoding(863); // French OEM (Canada)
            case 0x1D:
                return Encoding.GetEncoding(850); // French OEM secondary codepage
            case 0x1F:
                return Encoding.GetEncoding(852); // Czech OEM
            case 0x22:
                return Encoding.GetEncoding(852); // Hungarian OEM
            case 0x23:
                return Encoding.GetEncoding(852); // Polish OEM
            case 0x24:
                return Encoding.GetEncoding(860); // Portuguese OEM
            case 0x25:
                return Encoding.GetEncoding(850); // Portuguese OEM secondary codepage
            case 0x26:
                return Encoding.GetEncoding(866); // Russian OEM
            case 0x37:
                return Encoding.GetEncoding(850); // English OEM (U.S.) secondary codepage
            case 0x40:
                return Encoding.GetEncoding(852); // Romanian OEM
            case 0x4D:
                return Encoding.GetEncoding(936); // Chinese GBK (PRC)
            case 0x4E:
                return Encoding.GetEncoding(949); // Korean (ANSI/OEM)
            case 0x4F:
                return Encoding.GetEncoding(950); // Chinese Big5 (Taiwan)
            case 0x50:
                return Encoding.GetEncoding(874); // Thai (ANSI/OEM)
            case 0x57:
                return Encoding.GetEncoding(1252); // ANSI
            case 0x58:
                return Encoding.GetEncoding(1252); // Western European ANSI
            case 0x59:
                return Encoding.GetEncoding(1252); // Spanish ANSI
            case 0x64:
                return Encoding.GetEncoding(852); // Eastern European MS–DOS
            case 0x65:
                return Encoding.GetEncoding(866); // Russian MS–DOS
            case 0x66:
                return Encoding.GetEncoding(865); // Nordic MS–DOS
            case 0x67:
                return Encoding.GetEncoding(861); // Icelandic MS–DOS
            case 0x68:
                return Encoding.GetEncoding(895); // Kamenicky (Czech) MS-DOS 
            case 0x69:
                return Encoding.GetEncoding(620); // Mazovia (Polish) MS-DOS 
            case 0x6A:
                return Encoding.GetEncoding(737); // Greek MS–DOS (437G)
            case 0x6B:
                return Encoding.GetEncoding(857); // Turkish MS–DOS
            case 0x6C:
                return Encoding.GetEncoding(863); // French–Canadian MS–DOS
            case 0x78:
                return Encoding.GetEncoding(950); // Taiwan Big 5
            case 0x79:
                return Encoding.GetEncoding(949); // Hangul (Wansung)
            case 0x7A:
                return Encoding.GetEncoding(936); // PRC GBK
            case 0x7B:
                return Encoding.GetEncoding(932); // Japanese Shift-JIS
            case 0x7C:
                return Encoding.GetEncoding(874); // Thai Windows/MS–DOS
            case 0x7D:
                return Encoding.GetEncoding(1255); // Hebrew Windows 
            case 0x7E:
                return Encoding.GetEncoding(1256); // Arabic Windows 
            case 0x86:
                return Encoding.GetEncoding(737); // Greek OEM
            case 0x87:
                return Encoding.GetEncoding(852); // Slovenian OEM
            case 0x88:
                return Encoding.GetEncoding(857); // Turkish OEM
            case 0x96:
                return Encoding.GetEncoding(10007); // Russian Macintosh 
            case 0x97:
                return Encoding.GetEncoding(10029); // Eastern European Macintosh 
            case 0x98:
                return Encoding.GetEncoding(10006); // Greek Macintosh 
            case 0xC8:
                return Encoding.GetEncoding(1250); // Eastern European Windows
            case 0xC9:
                return Encoding.GetEncoding(1251); // Russian Windows
            case 0xCA:
                return Encoding.GetEncoding(1254); // Turkish Windows
            case 0xCB:
                return Encoding.GetEncoding(1253); // Greek Windows
            case 0xCC:
                return Encoding.GetEncoding(1257); // Baltic Windows
            default:
                return Encoding.UTF7;
        }
    }

    /// <summary>
    /// Returns a DataTable that describes the column metadata of the DBase file.
    /// </summary>
    /// <returns>A DataTable that describes the column metadata.</returns>
    public DataTable GetSchemaTable()
    {
        var tab = new DataTable();
        // all of common, non "base-table" fields implemented
        tab.Columns.Add("ColumnName", typeof(string));
        tab.Columns.Add("ColumnSize", typeof(int));
        tab.Columns.Add("ColumnOrdinal", typeof(int));
        tab.Columns.Add("NumericPrecision", typeof(short));
        tab.Columns.Add("NumericScale", typeof(short));
        tab.Columns.Add("DataType", typeof(Type));
        tab.Columns.Add("AllowDBNull", typeof(bool));
        tab.Columns.Add("IsReadOnly", typeof(bool));
        tab.Columns.Add("IsUnique", typeof(bool));
        tab.Columns.Add("IsRowVersion", typeof(bool));
        tab.Columns.Add("IsKey", typeof(bool));
        tab.Columns.Add("IsAutoIncrement", typeof(bool));
        tab.Columns.Add("IsLong", typeof(bool));

        if (_dbaseColumns != null)
        {
            foreach (var dbf in _dbaseColumns)
                tab.Columns.Add(dbf.ColumnName, dbf.DataType);

            for (var i = 0; i < _dbaseColumns.Length; i++)
            {
                var r = tab.NewRow();
                r["ColumnName"] = _dbaseColumns[i].ColumnName;
                r["ColumnSize"] = _dbaseColumns[i].Length;
                r["ColumnOrdinal"] = i;
                r["NumericPrecision"] = _dbaseColumns[i].Decimals;
                r["NumericScale"] = 0;
                r["DataType"] = _dbaseColumns[i].DataType;
                r["AllowDBNull"] = true;
                r["IsReadOnly"] = true;
                r["IsUnique"] = false;
                r["IsRowVersion"] = false;
                r["IsKey"] = false;
                r["IsAutoIncrement"] = false;
                r["IsLong"] = false;

                // specializations, if ID is unique
                //if (_ColumnNames[i] == "ID")
                //	r["IsUnique"] = true;

                tab.Rows.Add(r);
            }
        }

        return tab;
    }

    internal object GetValue(uint oid, int colid)
    {
        if (!_isOpen)
            throw new ApplicationException("An attempt was made to read from a closed DBF file");
        if (oid >= _numberOfRecords)
            throw new ArgumentException("Invalid DataRow requested at index " + oid.ToString(CultureInfo.InvariantCulture));
        if (colid >= _dbaseColumns!.Length || colid < 0)
            throw new ArgumentException("Column index out of range");

        _fs!.Seek(_headerLength + oid * _recordLength, 0);
        for (var i = 0; i < colid; i++)
            _br!.BaseStream.Seek(_dbaseColumns[i].Length, SeekOrigin.Current);

        return ReadDbfValue(_dbaseColumns[colid]);
    }

    private Encoding? _encoding;
    private Encoding _fileEncoding = Encoding.UTF7;

    /// <summary>
    /// Gets or sets the <see cref="System.Text.Encoding"/> used for parsing strings from the DBase DBF file.
    /// </summary>
    /// <remarks>
    /// If the encoding type isn't set, the dbase driver will try to determine the correct <see cref="System.Text.Encoding"/>.
    /// </remarks>
    public Encoding? Encoding
    {
        get => _encoding;
        set => _encoding = value;
    }

    /// <summary>
    /// Gets the feature at the specified Object ID
    /// </summary>
    /// <param name="oid"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    internal GeometryFeature? GetFeature(uint oid, IEnumerable<GeometryFeature> table)
    {
        if (oid >= _numberOfRecords)
            throw new ArgumentException("Invalid DataRow requested at index " + oid.ToString(CultureInfo.InvariantCulture));
        _fs!.Seek(_headerLength + oid * _recordLength, 0);

        var dr = new GeometryFeature();

        if (_br!.ReadChar() == '*') return null; // is record marked deleted?


        foreach (var dbf in _dbaseColumns!)
            dr[dbf.ColumnName] = ReadDbfValue(dbf);
        return dr;
    }

    private object ReadDbfValue(DbaseField dbf)
    {
        switch (dbf.DataType.ToString())
        {
            case "System.String":
                if (_encoding == null)
                    return _fileEncoding.GetString(_br!.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                return _encoding.GetString(_br!.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
            case "System.Double":
                var temp = Encoding.UTF7.GetString(_br!.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                if (double.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out var dbl))
                    return dbl;
                return DBNull.Value;
            case "System.Int16":
                var temp16 = Encoding.UTF7.GetString(_br!.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                if (short.TryParse(temp16, NumberStyles.Float, CultureInfo.InvariantCulture, out var i16))
                    return i16;
                return DBNull.Value;
            case "System.Int32":
                var temp32 = Encoding.UTF7.GetString(_br!.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                if (int.TryParse(temp32, NumberStyles.Float, CultureInfo.InvariantCulture, out var i32))
                    return i32;
                return DBNull.Value;
            case "System.Int64":
                var temp64 = Encoding.UTF7.GetString(_br!.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                if (long.TryParse(temp64, NumberStyles.Float, CultureInfo.InvariantCulture, out var i64))
                    return i64;
                return DBNull.Value;
            case "System.Single":
                var temp4 = Encoding.UTF8.GetString(_br!.ReadBytes(dbf.Length));
                if (float.TryParse(temp4, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    return f;
                return DBNull.Value;
            case "System.Boolean":
                var tempChar = _br!.ReadChar();
                return tempChar == 'T' || tempChar == 't' || tempChar == 'Y' || tempChar == 'y';
            case "System.DateTime":
                // Mono has not yet implemented DateTime.TryParseExact
                if (DateTime.TryParseExact(Encoding.UTF7.GetString(_br!.ReadBytes(8)),
                        "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    return date;
                return DBNull.Value;
            default:
                throw new NotSupportedException("Cannot parse DBase field '" + dbf.ColumnName + "' of type '" +
                                                 dbf.DataType + "'");
        }
    }
}

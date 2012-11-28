// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// Note:
// Good stuff on DBase format: http://www.clicketyclick.dk/databases/xbase/format/

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using Mapsui.Providers;
using Mapsui.Utilities.Indexing;

namespace Mapsui.Data.Providers
{
    internal class DbaseReader : IDisposable
    {
        private struct DbaseField
        {
            public int Address;
            public string ColumnName;
            public Type DataType;
            public int Decimals;
            public int Length;
        }

        private DateTime _lastUpdate;
        private int _NumberOfRecords;
        private Int16 _HeaderLength;
        private Int16 _RecordLength;
        private string _filename;
        private DbaseField[] DbaseColumns;
        private FileStream fs;
        private BinaryReader br;
        private bool HeaderIsParsed;

        public DbaseReader(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(String.Format("Could not find file \"{0}\"", filename));
            _filename = filename;
            HeaderIsParsed = false;
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; }
        }

        public void Open()
        {
            fs = new FileStream(_filename, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);
            _isOpen = true;
            if (!HeaderIsParsed) //Don't read the header if it's already parsed
                ParseDbfHeader(_filename);
        }

        public void Close()
        {
            br.Close();
            fs.Close();
            _isOpen = false;
        }

        public void Dispose()
        {
            if (_isOpen)
                Close();
            br = null;
            fs = null;
        }

        // Binary Tree not working yet on Mono 
        // see bug: http://bugzilla.ximian.com/show_bug.cgi?id=78502
#if !MONO
        /// <summary>
        /// Indexes a DBF column in a binary tree [NOT COMPLETE]
        /// </summary>
        /// <typeparam name="T">datatype to be indexed</typeparam>
        /// <param name="ColumnId">Column to index</param>
        /// <returns></returns>
        public BinaryTree<T, UInt32> CreateDbfIndex<T>(int ColumnId) where T : IComparable<T>
        {
            BinaryTree<T, UInt32> tree = new BinaryTree<T, uint>();
            for (uint i = 0; i < ((_NumberOfRecords > 10000) ? 10000 : _NumberOfRecords); i++)
                tree.Add(new BinaryTree<T, uint>.ItemValue((T) GetValue(i, ColumnId), i));
            return tree;
        }
#endif
        /*
		/// <summary>
		/// Creates an index on the columns for faster searching [EXPERIMENTAL - Requires Lucene dependencies]
		/// </summary>
		/// <returns></returns>
		public string CreateLuceneIndex()
		{
			string dir = this._filename + ".idx";
			if (!System.IO.Directory.Exists(dir))
				System.IO.Directory.CreateDirectory(dir);
			Lucene.Net.Index.IndexWriter iw = new Lucene.Net.Index.IndexWriter(dir,new Lucene.Net.Analysis.Standard.StandardAnalyzer(),true);

			for (uint i = 0; i < this._NumberOfRecords; i++)
			{
				FeatureDataRow dr = GetFeature(i,this.NewTable);
				Lucene.Net.Documents.Document doc = new Lucene.Net.Documents.Document();
				// Add the object-id as a field, so that index can be maintained.
				// This field is not stored with document, it is indexed, but it is not
	            // tokenized prior to indexing.
				//doc.Add(Lucene.Net.Documents.Field.UnIndexed("SharpMap_oid", i.ToString())); //Add OID index

				foreach(System.Data.DataColumn col in dr.Table.Columns) //Add and index values from DBF
				{
					if(col.DataType.Equals(typeof(string)))
						// Add the contents as a valued Text field so it will get tokenized and indexed.
						doc.Add(Lucene.Net.Documents.Field.UnStored(col.ColumnName,(string)dr[col]));
					else
						doc.Add(Lucene.Net.Documents.Field.UnStored(col.ColumnName, dr[col].ToString()));
				}
				iw.AddDocument(doc);
			}
			iw.Optimize();
			iw.Close();
			return this._filename + ".idx";
		}
		*/

        /// <summary>
        /// Gets the date this file was last updated.
        /// </summary>
        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
        }

        private void ParseDbfHeader(string filename)
        {
            if (br.ReadByte() != 0x03)
                throw new NotSupportedException("Unsupported DBF Type");

            _lastUpdate = new DateTime((int) br.ReadByte() + 1900, (int) br.ReadByte(), (int) br.ReadByte());
                //Read the last update date
            _NumberOfRecords = br.ReadInt32(); // read number of records.
            _HeaderLength = br.ReadInt16(); // read length of header structure.
            _RecordLength = br.ReadInt16(); // read length of a record
            fs.Seek(29, SeekOrigin.Begin); //Seek to encoding flag
            _FileEncoding = GetDbaseLanguageDriver(br.ReadByte()); //Read and parse Language driver
            fs.Seek(32, SeekOrigin.Begin); //Move past the reserved bytes

            int NumberOfColumns = (_HeaderLength - 31)/32; // calculate the number of DataColumns in the header
            DbaseColumns = new DbaseField[NumberOfColumns];
            for (int i = 0; i < DbaseColumns.Length; i++)
            {
                DbaseColumns[i] = new DbaseField();
                DbaseColumns[i].ColumnName = Encoding.UTF7.GetString((br.ReadBytes(11))).Replace("\0", "").Trim();
                char fieldtype = br.ReadChar();
                switch (fieldtype)
                {
                    case 'L':
                        DbaseColumns[i].DataType = typeof (bool);
                        break;
                    case 'C':
                        DbaseColumns[i].DataType = typeof (string);
                        break;
                    case 'D':
                        DbaseColumns[i].DataType = typeof (DateTime);
                        break;
                    case 'N':
                        DbaseColumns[i].DataType = typeof (double);
                        break;
                    case 'F':
                        DbaseColumns[i].DataType = typeof (float);
                        break;
                    case 'B':
                        DbaseColumns[i].DataType = typeof (byte[]);
                        break;
                    default:
                        throw (new NotSupportedException("Invalid or unknown DBase field type '" + fieldtype +
                                                         "' in column '" + DbaseColumns[i].ColumnName + "'"));
                }
                DbaseColumns[i].Address = br.ReadInt32();

                int Length = (int) br.ReadByte();
                if (Length < 0) Length = Length + 256;
                DbaseColumns[i].Length = Length;
                DbaseColumns[i].Decimals = (int) br.ReadByte();
                //If the double-type doesn't have any decimals, make the type an integer
                if (DbaseColumns[i].Decimals == 0 && DbaseColumns[i].DataType == typeof (double))
                    if (DbaseColumns[i].Length <= 2)
                        DbaseColumns[i].DataType = typeof (Int16);
                    else if (DbaseColumns[i].Length <= 4)
                        DbaseColumns[i].DataType = typeof (Int32);
                    else
                        DbaseColumns[i].DataType = typeof (Int64);
                fs.Seek(fs.Position + 14, 0);
            }
            HeaderIsParsed = true;
            CreateBaseTable();
        }

        private Encoding GetDbaseLanguageDriver(byte dbasecode)
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
            DataTable tab = new DataTable();
            // all of common, non "base-table" fields implemented
            tab.Columns.Add("ColumnName", typeof (String));
            tab.Columns.Add("ColumnSize", typeof (Int32));
            tab.Columns.Add("ColumnOrdinal", typeof (Int32));
            tab.Columns.Add("NumericPrecision", typeof (Int16));
            tab.Columns.Add("NumericScale", typeof (Int16));
            tab.Columns.Add("DataType", typeof (Type));
            tab.Columns.Add("AllowDBNull", typeof (bool));
            tab.Columns.Add("IsReadOnly", typeof (bool));
            tab.Columns.Add("IsUnique", typeof (bool));
            tab.Columns.Add("IsRowVersion", typeof (bool));
            tab.Columns.Add("IsKey", typeof (bool));
            tab.Columns.Add("IsAutoIncrement", typeof (bool));
            tab.Columns.Add("IsLong", typeof (bool));

            foreach (DbaseField dbf in DbaseColumns)
                tab.Columns.Add(dbf.ColumnName, dbf.DataType);

            for (int i = 0; i < DbaseColumns.Length; i++)
            {
                DataRow r = tab.NewRow();
                r["ColumnName"] = DbaseColumns[i].ColumnName;
                r["ColumnSize"] = DbaseColumns[i].Length;
                r["ColumnOrdinal"] = i;
                r["NumericPrecision"] = DbaseColumns[i].Decimals;
                r["NumericScale"] = 0;
                r["DataType"] = DbaseColumns[i].DataType;
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

            return tab;
        }


        private FeatureDataTable baseTable;

        private void CreateBaseTable()
        {
            baseTable = new FeatureDataTable();
            foreach (DbaseField dbf in DbaseColumns)
                baseTable.Columns.Add(dbf.ColumnName, dbf.DataType);
        }

        internal FeatureDataTable NewTable
        {
            get { return baseTable.Clone(); }
        }

        internal object GetValue(uint oid, int colid)
        {
            if (!_isOpen)
                throw (new ApplicationException("An attempt was made to read from a closed DBF file"));
            if (oid >= _NumberOfRecords)
                throw (new ArgumentException("Invalid DataRow requested at index " + oid.ToString()));
            if (colid >= DbaseColumns.Length || colid < 0)
                throw ((new ArgumentException("Column index out of range")));

            fs.Seek(_HeaderLength + oid*_RecordLength, 0);
            for (int i = 0; i < colid; i++)
                br.BaseStream.Seek(DbaseColumns[i].Length, SeekOrigin.Current);

            return ReadDbfValue(DbaseColumns[colid]);
        }

        private Encoding _Encoding;
        private Encoding _FileEncoding;

        /// <summary>
        /// Gets or sets the <see cref="System.Text.Encoding"/> used for parsing strings from the DBase DBF file.
        /// </summary>
        /// <remarks>
        /// If the encoding type isn't set, the dbase driver will try to determine the correct <see cref="System.Text.Encoding"/>.
        /// </remarks>
        public Encoding Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }


        /// <summary>
        /// Gets the feature at the specified Object ID
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        internal IFeature GetFeature(uint oid, IFeatures table)
        {
            if (!_isOpen)
                throw (new ApplicationException("An attempt was made to read from a closed DBF file"));
            if (oid >= _NumberOfRecords)
                throw (new ArgumentException("Invalid DataRow requested at index " + oid.ToString()));
            fs.Seek(_HeaderLength + oid * _RecordLength, 0);

            IFeature dr = table.New();

            if (br.ReadChar() == '*') //is record marked deleted?
                return null;

            for (int i = 0; i < DbaseColumns.Length; i++)
            {
                DbaseField dbf = DbaseColumns[i];
                dr[dbf.ColumnName] = ReadDbfValue(dbf);
            }
            return dr;
        }

        private object ReadDbfValue(DbaseField dbf)
        {
            switch (dbf.DataType.ToString())
            {
                case "System.String":
                    if (_Encoding == null)
                        return _FileEncoding.GetString(br.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                    else
                        return _Encoding.GetString(br.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                case "System.Double":
                    string temp = Encoding.UTF7.GetString(br.ReadBytes(dbf.Length)).Replace("\0", "").Trim();
                    double dbl = 0;
                    if (double.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out dbl))
                        return dbl;
                    else
                        return DBNull.Value;
                case "System.Int16":
                    string temp16 = Encoding.UTF7.GetString((br.ReadBytes(dbf.Length))).Replace("\0", "").Trim();
                    Int16 i16 = 0;
                    if (Int16.TryParse(temp16, NumberStyles.Float, CultureInfo.InvariantCulture, out i16))
                        return i16;
                    else
                        return DBNull.Value;
                case "System.Int32":
                    string temp32 = Encoding.UTF7.GetString((br.ReadBytes(dbf.Length))).Replace("\0", "").Trim();
                    Int32 i32 = 0;
                    if (Int32.TryParse(temp32, NumberStyles.Float, CultureInfo.InvariantCulture, out i32))
                        return i32;
                    else
                        return DBNull.Value;
                case "System.Int64":
                    string temp64 = Encoding.UTF7.GetString((br.ReadBytes(dbf.Length))).Replace("\0", "").Trim();
                    Int64 i64 = 0;
                    if (Int64.TryParse(temp64, NumberStyles.Float, CultureInfo.InvariantCulture, out i64))
                        return i64;
                    else
                        return DBNull.Value;
                case "System.Single":
                    string temp4 = Encoding.UTF8.GetString((br.ReadBytes(dbf.Length)));
                    float f = 0;
                    if (float.TryParse(temp4, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                        return f;
                    else
                        return DBNull.Value;
                case "System.Boolean":
                    char tempChar = br.ReadChar();
                    return ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'));
                case "System.DateTime":
                    DateTime date;
                    // Mono has not yet implemented DateTime.TryParseExact
#if !MONO
                    if (DateTime.TryParseExact(Encoding.UTF7.GetString((br.ReadBytes(8))),
                                               "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        return date;
                    else
                        return DBNull.Value;
#else
					try 
					{
						return date = DateTime.ParseExact ( System.Text.Encoding.UTF7.GetString((br.ReadBytes(8))), 	
						"yyyyMMdd", Mapsui.Map.numberFormat_EnUS, System.Globalization.DateTimeStyles.None );
					}
					catch ( Exception e )
					{
						return DBNull.Value;
					}
#endif
                default:
                    throw (new NotSupportedException("Cannot parse DBase field '" + dbf.ColumnName + "' of type '" +
                                                     dbf.DataType.ToString() + "'"));
            }
        }
    }
}
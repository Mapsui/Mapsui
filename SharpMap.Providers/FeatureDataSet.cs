// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using SharpMap.Geometries;
using SharpMap.Providers;
using SharpMap.Styles;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents an in-memory cache of spatial data. The FeatureDataSet is an extension of System.Data.DataSet
    /// </summary>
    [Serializable()]
    public class FeatureDataSet : DataSet
    {
        private FeatureTableCollection _FeatureTables;

        /// <summary>
        /// Initializes a new instance of the FeatureDataSet class.
        /// </summary>
        public FeatureDataSet()
        {
            InitClass();
            CollectionChangeEventHandler schemaChangedHandler = new CollectionChangeEventHandler(SchemaChanged);
            //this.Tables.CollectionChanged += schemaChangedHandler;
            Relations.CollectionChanged += schemaChangedHandler;
            InitClass();
        }

        /// <summary>
        /// nitializes a new instance of the FeatureDataSet class.
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">streaming context</param>
        protected FeatureDataSet(SerializationInfo info, StreamingContext context)
        {
            string strSchema = ((string) (info.GetValue("XmlSchema", typeof (string))));
            if ((strSchema != null))
            {
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(new XmlTextReader(new StringReader(strSchema)));
                if ((ds.Tables["FeatureTable"] != null))
                {
                    Tables.Add(new FeatureDataTable(ds.Tables["FeatureTable"]));
                }
                DataSetName = ds.DataSetName;
                Prefix = ds.Prefix;
                Namespace = ds.Namespace;
                Locale = ds.Locale;
                CaseSensitive = ds.CaseSensitive;
                EnforceConstraints = ds.EnforceConstraints;
                Merge(ds, false, MissingSchemaAction.Add);
            }
            else
            {
                InitClass();
            }
            GetSerializationData(info, context);
            CollectionChangeEventHandler schemaChangedHandler = SchemaChanged;
            //this.Tables.CollectionChanged += schemaChangedHandler;
            Relations.CollectionChanged += schemaChangedHandler;
        }

        /// <summary>
        /// Gets the collection of tables contained in the FeatureDataSet
        /// </summary>
        public new FeatureTableCollection Tables
        {
            get { return _FeatureTables; }
        }

        /// <summary>
        /// Copies the structure of the FeatureDataSet, including all FeatureDataTable schemas, relations, and constraints. Does not copy any data. 
        /// </summary>
        /// <returns></returns>
        public new FeatureDataSet Clone()
        {
            FeatureDataSet cln = ((FeatureDataSet) (base.Clone()));
            return cln;
        }

        /// <summary>
        /// Gets a value indicating whether Tables property should be persisted.
        /// </summary>
        /// <returns></returns>
        protected override bool ShouldSerializeTables()
        {
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether Relations property should be persisted.
        /// </summary>
        /// <returns></returns>
        protected override bool ShouldSerializeRelations()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        protected override void ReadXmlSerializable(XmlReader reader)
        {
            Reset();
            DataSet ds = new DataSet();
            ds.ReadXml(reader);
            //if ((ds.Tables["FeatureTable"] != null))
            //{
            //    this.Tables.Add(new FeatureDataTable(ds.Tables["FeatureTable"]));
            //}
            DataSetName = ds.DataSetName;
            Prefix = ds.Prefix;
            Namespace = ds.Namespace;
            Locale = ds.Locale;
            CaseSensitive = ds.CaseSensitive;
            EnforceConstraints = ds.EnforceConstraints;
            Merge(ds, false, MissingSchemaAction.Add);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override XmlSchema GetSchemaSerializable()
        {
            MemoryStream stream = new MemoryStream();
            WriteXmlSchema(new XmlTextWriter(stream, null));
            stream.Position = 0;
            return XmlSchema.Read(new XmlTextReader(stream), null);
        }


        private void InitClass()
        {
            _FeatureTables = new FeatureTableCollection();
            //this.DataSetName = "FeatureDataSet";
            Prefix = "";
            Namespace = "http://tempuri.org/FeatureDataSet.xsd";
            Locale = new CultureInfo("en-US");
            CaseSensitive = false;
            EnforceConstraints = true;
        }

        private bool ShouldSerializeFeatureTable()
        {
            return false;
        }

        private void SchemaChanged(object sender, CollectionChangeEventArgs e)
        {
            if ((e.Action == CollectionChangeAction.Remove))
            {
                //this.InitVars();
            }
        }
    }

    /// <summary>
    /// Represents the method that will handle the RowChanging, RowChanged, RowDeleting, and RowDeleted events of a FeatureDataTable. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FeatureDataRowChangeEventHandler(object sender, FeatureDataRowChangeEventArgs e);

    /// <summary>
    /// Represents one feature table of in-memory spatial data. 
    /// </summary>
    [DebuggerStepThrough()]
    [Serializable()]
    public class FeatureDataTable : DataTable, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the FeatureDataTable class with no arguments.
        /// </summary>
        public FeatureDataTable() 
        {
            InitClass();
        }

        /// <summary>
        /// Intitalizes a new instance of the FeatureDataTable class with the specified table name.
        /// </summary>
        /// <param name="table"></param>
        public FeatureDataTable(DataTable table)
            : base(table.TableName)
        {
            if (table.DataSet != null)
            {
                if ((table.CaseSensitive != table.DataSet.CaseSensitive))
                {
                    CaseSensitive = table.CaseSensitive;
                }
                if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
                {
                    Locale = table.Locale;
                }
                if ((table.Namespace != table.DataSet.Namespace))
                {
                    Namespace = table.Namespace;
                }
            }

            Prefix = table.Prefix;
            MinimumCapacity = table.MinimumCapacity;
            DisplayExpression = table.DisplayExpression;
        }

        /// <summary>
        /// Gets the number of rows in the table
        /// </summary>
        [Browsable(false)]
        public int Count
        {
            get { return Rows.Count; }
        }

        /// <summary>
        /// Gets the feature data row at the specified index
        /// </summary>
        /// <param name="index">row index</param>
        /// <returns>FeatureDataRow</returns>
        public FeatureDataRow this[int index]
        {
            get { return (FeatureDataRow) Rows[index]; }
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator for enumering the rows of the FeatureDataTable
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return base.Rows.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Occurs after a FeatureDataRow has been changed successfully. 
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowChanged;

        /// <summary>
        /// Occurs when a FeatureDataRow is changing. 
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowChanging;

        /// <summary>
        /// Occurs after a row in the table has been deleted.
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowDeleted;

        /// <summary>
        /// Occurs before a row in the table is about to be deleted.
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowDeleting;

        /// <summary>
        /// Adds a row to the FeatureDataTable
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(FeatureDataRow row)
        {
            Rows.Add(row);
        }

        /// <summary>
        /// Clones the structure of the FeatureDataTable, including all FeatureDataTable schemas and constraints. 
        /// </summary>
        /// <returns></returns>
        public new FeatureDataTable Clone()
        {
            FeatureDataTable cln = ((FeatureDataTable) (base.Clone()));
            cln.InitVars();
            return cln;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DataTable CreateInstance()
        {
            return new FeatureDataTable();
        }

        internal void InitVars()
        {
            //this.columnFeatureGeometry = this.Columns["FeatureGeometry"];
        }

        private void InitClass()
        {
            //this.columnFeatureGeometry = new DataColumn("FeatureGeometry", typeof(SharpMap.Geometries.Geometry), null, System.Data.MappingType.Element);
            //this.Columns.Add(this.columnFeatureGeometry);
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table.
        /// </summary>
        /// <returns></returns>
        public new FeatureDataRow NewRow()
        {
            return (FeatureDataRow) base.NewRow();
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table, based on a datarow builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new FeatureDataRow(builder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Type GetRowType()
        {
            return typeof (FeatureDataRow);
        }

        /// <summary>
        /// Raises the FeatureDataRowChanged event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanged(DataRowChangeEventArgs e)
        {
            base.OnRowChanged(e);
            if ((FeatureDataRowChanged != null))
            {
                FeatureDataRowChanged(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowChanging event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanging(DataRowChangeEventArgs e)
        {
            base.OnRowChanging(e);
            if ((FeatureDataRowChanging != null))
            {
                FeatureDataRowChanging(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleted event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleted(DataRowChangeEventArgs e)
        {
            base.OnRowDeleted(e);
            if ((FeatureDataRowDeleted != null))
            {
                FeatureDataRowDeleted(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleting event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleting(DataRowChangeEventArgs e)
        {
            base.OnRowDeleting(e);
            if ((FeatureDataRowDeleting != null))
            {
                FeatureDataRowDeleting(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow) (e.Row)), e.Action));
            }
        }

        ///// <summary>
        ///// Gets the collection of rows that belong to this table.
        ///// </summary>
        //public new DataRowCollection Rows
        //{
        //    get { throw (new NotSupportedException()); }
        //    set { throw (new NotSupportedException()); }
        //}

        /// <summary>
        /// Removes the row from the table
        /// </summary>
        /// <param name="row">Row to remove</param>
        public void RemoveRow(FeatureDataRow row)
        {
            Rows.Remove(row);
        }


        public IEnumerable<IFeature> Items
        {
            get
            {
                foreach (FeatureDataRow row in base.Rows)
                {
                    yield return row;
                }
            }
        }

        //Not sure if the New and Add methods should be here. 
        //Maybe FeatureDataSet should just have some 
        //readonly IFeatureCollection inteface. PDD.
        public IFeature New()
        {
            return (FeatureDataRow)base.NewRow();
        }

        public void Add(IFeature row)
        {
            Rows.Add(row);
        }
    }

    /// <summary>
    /// Represents the collection of tables for the FeatureDataSet.
    /// </summary>
    [Serializable()]
    public class FeatureTableCollection : List<FeatureDataTable>
    {
    }

    /// <summary>
    /// Represents a row of data in a FeatureDataTable.
    /// </summary>
    [DebuggerStepThrough()]
    [Serializable()]
    public class FeatureDataRow : DataRow, IFeature
    {
        internal FeatureDataRow(DataRowBuilder rb) : base(rb)
        {
            Styles = new Collection<IStyle>();
        }

        /// <summary>
        /// The geometry of the current feature
        /// </summary>
        public IGeometry Geometry { get; set; }

        public object RenderedGeometry { get; set; }

        public ICollection<IStyle> Styles { get; set; }

        /// <summary>
        /// Returns true of the geometry is null
        /// </summary>
        /// <returns></returns>
        public bool IsFeatureGeometryNull()
        {
            return Geometry == null;
        }

        /// <summary>
        /// Sets the geometry column to null
        /// </summary>
        public void SetFeatureGeometryNull()
        {
            Geometry = null;
        }

        public IEnumerable<string> Fields
        {
            get
            {
                throw new NotImplementedException();
                //it should be something like this. But I am not sure this gets the column name:
                //foreach (var column in this.Table.Columns) yield return column.ToString(); 
            }
        }

    }

    /// <summary>
    /// Occurs after a FeatureDataRow has been changed successfully.
    /// </summary>
    [DebuggerStepThrough()]
    public class FeatureDataRowChangeEventArgs : EventArgs
    {
        private DataRowAction eventAction;
        private FeatureDataRow eventRow;

        /// <summary>
        /// Initializes a new instance of the FeatureDataRowChangeEventArgs class.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="action"></param>
        public FeatureDataRowChangeEventArgs(FeatureDataRow row, DataRowAction action)
        {
            eventRow = row;
            eventAction = action;
        }

        /// <summary>
        /// Gets the row upon which an action has occurred.
        /// </summary>
        public FeatureDataRow Row
        {
            get { return eventRow; }
        }

        /// <summary>
        /// Gets the action that has occurred on a FeatureDataRow.
        /// </summary>
        public DataRowAction Action
        {
            get { return eventAction; }
        }

    }
}
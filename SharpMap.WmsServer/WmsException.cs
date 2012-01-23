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

using System.Web;

namespace SharpMap.Web.Wms
{
    /// <summary>
    /// Class for throwing WMS exceptions to client
    /// </summary>
    internal class WmsException
    {
        internal static void ThrowWmsException(string Message)
        {
            ThrowWmsException(WmsExceptionCode.NotApplicable, Message);
        }

        internal static void ThrowWmsException(WmsExceptionCode code, string Message)
        {
            HttpResponse Response = HttpContext.Current.Response;
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n");
            Response.Write(
                "<ServiceExceptionReport version=\"1.3.0\" xmlns=\"http://www.opengis.net/ogc\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.opengis.net/ogc http://schemas.opengis.net/wms/1.3.0/exceptions_1_3_0.xsd\">\n");
            Response.Write("<ServiceException");
            if (code != WmsExceptionCode.NotApplicable)
                Response.Write(" code=\"" + code + "\"");
            Response.Write(">" + Message + "</ServiceException>\n");
            Response.Write("</ServiceExceptionReport>");
            Response.End();
        }

        #region Nested type: WmsExceptionCode

        /// <summary>
        /// WMS Exception codes
        /// </summary>
        internal enum WmsExceptionCode
        {
            /// <summary>
            /// Request contains a Format not offered by the server.
            /// </summary>
            InvalidFormat,
            /// <summary>
            /// Request contains a CRS not offered by the server for one or more of the
            /// Layers in the request.
            /// </summary>
            InvalidCRS,
            /// <summary>
            /// GetMap request is for a Layer not offered by the server, or GetFeatureInfo
            /// request is for a Layer not shown on the map.
            /// </summary>
            LayerNotDefined,
            /// <summary>
            /// Request is for a Layer in a Style not offered by the server.
            /// </summary>
            StyleNotDefined,
            /// <summary>
            /// GetFeatureInfo request is applied to a Layer which is not declared queryable.
            /// </summary>
            LayerNotQueryable,
            /// <summary>
            /// GetFeatureInfo request contains invalid X or Y value.
            /// </summary>
            InvalidPoint,
            /// <summary>
            /// Value of (optional) UpdateSequence parameter in GetCapabilities request is
            /// equal to current value of service metadata update sequence number.
            /// </summary>
            CurrentUpdateSequence,
            /// <summary>
            /// Value of (optional) UpdateSequence parameter in GetCapabilities request is
            /// greater than current value of service metadata update sequence number.
            /// </summary>
            InvalidUpdateSequence,
            /// <summary>
            /// Request does not include a sample dimension value, and the server did not
            /// declare a default value for that dimension.
            /// </summary>
            MissingDimensionValue,
            /// <summary>
            /// Request contains an invalid sample dimension value.
            /// </summary>
            InvalidDimensionValue,
            /// <summary>
            /// Request is for an optional operation that is not supported by the server.
            /// </summary>
            OperationNotSupported,
            /// <summary>
            /// No error code
            /// </summary>
            NotApplicable
        }

        #endregion

        //private static System.Xml.Schema.XmlSchema GetExceptionSchema()
        //{
        //    //Get XML Schema
        //    System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Web.Wms.Schemas._1._3._0.exceptions_1_3_0.xsd");
        //    System.Xml.Schema.XmlSchema schema = System.Xml.Schema.XmlSchema.Read(stream, null);
        //    stream.Close();
        //    return schema;
        //}
    }
}
// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Mapsui.Projections;

// ReSharper disable InconsistentNaming

namespace Mapsui.Providers.Wfs.Utilities
{
    public class WFS_1_1_0_TextResources : WFS_1_1_0_XPathTextResources, IWFS_TextResources
    {

        ////////////////////////////////////////////////////////////////////////
        // HTTP Configuration                                                 //                      
        // POST & GET                                                         //
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// This method returns the query string for 'GetCapabilities'.
        /// </summary>
        public string GetCapabilitiesRequest()
        {
            return "?SERVICE=WFS&Version=1.1.0&REQUEST=GetCapabilities";
        }

        /// <summary>
        /// This method returns the query string for 'DescribeFeatureType'.
        /// </summary>
        /// <param name="featureTypeName">The name of the featuretype to query</param>
        public string DescribeFeatureTypeRequest(string featureTypeName)
        {
            return "?SERVICE=WFS&Version=1.1.0&REQUEST=DescribeFeatureType&TYPENAME=" + featureTypeName +
                   "&NAMESPACE=xmlns(app=http://www.deegree.org/app)";
        }

        /// <summary>
        /// This method returns the query string for 'GetFeature'.
        /// </summary>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelProperties"></param>
        /// <param name="boundingBox">The bounding box of the query</param>
        /// <param name="filter">An instance implementing <see cref="IFilter"/></param>
        public string GetFeatureGETRequest(WfsFeatureTypeInfo featureTypeInfo, List<string>? labelProperties,
            MRect? boundingBox, IFilter? filter)
        {
            var qualification = string.IsNullOrEmpty(featureTypeInfo.Prefix)
                ? string.Empty
                : featureTypeInfo.Prefix + ":";

            var paramBuilder = new StringBuilder();

            paramBuilder.Append("?SERVICE=WFS&Version=1.1.0&REQUEST=GetFeature&TYPENAME=");
            paramBuilder.Append(HttpUtility.UrlEncode(qualification + featureTypeInfo.Name));
            paramBuilder.Append("&srsName=");
            paramBuilder.Append(HttpUtility.UrlEncode(CrsHelper.EpsgPrefix + featureTypeInfo.SRID));

            if (filter != null || boundingBox != null)
            {
                paramBuilder.Append("&FILTER=");

                using var sWriter = new StringWriter();
                using var xWriter = new XmlTextWriter(sWriter);
                {
                    AppendGml3Filter(xWriter, featureTypeInfo, boundingBox, filter, qualification);
                }
                paramBuilder.Append(HttpUtility.UrlEncode(sWriter.ToString()));
            }

            if (!string.IsNullOrEmpty(featureTypeInfo.Prefix))
            {
                paramBuilder.Append("&NAMESPACE=xmlns(" + HttpUtility.UrlEncode(featureTypeInfo.Prefix) + "=" +
                                    HttpUtility.UrlEncode(featureTypeInfo.FeatureTypeNamespace) + ")");
            }

            return paramBuilder.ToString();
        }

        /// <summary>
        /// This method returns the POST request for 'GetFeature'.
        /// </summary>
        /// <param name="featureTypeInfo">A <see cref="WfsFeatureTypeInfo"/> instance providing metadata of the featuretype to query</param>
        /// <param name="labelProperties">A list of properties necessary for label rendering</param>
        /// <param name="boundingBox">The bounding box of the query</param>
        /// <param name="filter">An instance implementing <see cref="IFilter"/></param>
        public byte[] GetFeaturePOSTRequest(WfsFeatureTypeInfo featureTypeInfo, List<string>? labelProperties,
            MRect? boundingBox, IFilter? filter)
        {
            var qualification = string.IsNullOrEmpty(featureTypeInfo.Prefix)
                                       ? string.Empty
                                       : featureTypeInfo.Prefix + ":";

            using (var sWriter = new StringWriter())
            {
                using (var xWriter = new XmlTextWriter(sWriter))
                {
                    xWriter.Namespaces = true;
                    xWriter.WriteStartElement("GetFeature", NSWFS);
                    xWriter.WriteAttributeString("service", "WFS");
                    xWriter.WriteAttributeString("version", "1.1.0");
                    if (!string.IsNullOrEmpty(featureTypeInfo.Prefix) &&
                        !string.IsNullOrEmpty(featureTypeInfo.FeatureTypeNamespace))
                        xWriter.WriteAttributeString("xmlns:" + featureTypeInfo.Prefix,
                                                     featureTypeInfo.FeatureTypeNamespace);
                    //added by PDD to get it to work for degree default sample
                    xWriter.WriteStartElement("Query", NSWFS);
                    xWriter.WriteAttributeString("typeName", qualification + featureTypeInfo.Name);
                    xWriter.WriteAttributeString("srsName", CrsHelper.EpsgPrefix + featureTypeInfo.SRID);
                    xWriter.WriteElementString("PropertyName", qualification + featureTypeInfo.Geometry.GeometryName);
                    if (labelProperties != null)
                        foreach (var labelProperty in labelProperties.Where(labelProperty =>
                                     !string.IsNullOrWhiteSpace(labelProperty)))
                        {
                            xWriter.WriteElementString("PropertyName", qualification + labelProperty);
                        }

                    AppendGml3Filter(xWriter, featureTypeInfo, boundingBox, filter, qualification);

                    xWriter.WriteEndElement();
                    xWriter.WriteEndElement();
                    xWriter.Flush();
                    return Encoding.UTF8.GetBytes(sWriter.ToString());
                }
            }
        }

        private void AppendGml3Filter(XmlTextWriter xWriter, WfsFeatureTypeInfo featureTypeInfo, MRect? boundingBox,
            IFilter? filter, string qualification)
        {
            xWriter.WriteStartElement("Filter", NSOGC);
            if (filter != null && boundingBox != null) xWriter.WriteStartElement("And");
            if (boundingBox != null)
            {
                xWriter.WriteStartElement("BBOX");
                if (!string.IsNullOrEmpty(featureTypeInfo.Prefix) &&
                    !string.IsNullOrEmpty(featureTypeInfo.FeatureTypeNamespace))
                    xWriter.WriteElementString("PropertyName",
                        qualification + featureTypeInfo.Geometry.GeometryName);
                //added qualification to get it to work for degree default sample
                else
                    xWriter.WriteElementString("PropertyName", featureTypeInfo.Geometry.GeometryName);
                xWriter.WriteStartElement("gml", "Envelope", NSGML);
                xWriter.WriteAttributeString("srsName",
                    "http://www.opengis.net/gml/srs/epsg.xml#" + featureTypeInfo.SRID);
                xWriter.WriteElementString("lowerCorner", NSGML,
                    XmlConvert.ToString(boundingBox.Left) + " " +
                    XmlConvert.ToString(boundingBox.Bottom));
                xWriter.WriteElementString("upperCorner", NSGML,
                    XmlConvert.ToString(boundingBox.Right) + " " +
                    XmlConvert.ToString(boundingBox.Top));
                xWriter.WriteEndElement();
                xWriter.WriteEndElement();
            }

            if (filter != null) xWriter.WriteRaw(filter.Encode());
            if (filter != null && boundingBox != null) xWriter.WriteEndElement();
            xWriter.WriteEndElement();
        }
    }
}
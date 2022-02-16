// Copyright (c) The Mapsui authors.
// The Mapsui authors licened this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (peter.robineau@gmx.at)

using System.Collections;
using System.IO;
using System.Xml.XPath;
using Mapsui.Providers.Wfs.Utilities;

namespace Mapsui.Providers.Wfs.Xml
{
    /// <summary>
    /// XPathQueryManager interface
    /// </summary>
    public interface IXPathQueryManager
    {
        void AddNamespace(string prefix, string ns);
        XPathExpression? Compile(string xPath);
        IXPathQueryManager Clone();
        XPathNodeIterator? GetIterator(XPathExpression? xPath);
        XPathNodeIterator? GetIterator(XPathExpression xPath, DictionaryEntry[] queryParameters);
        string? GetValueFromNode(XPathExpression? xPath, DictionaryEntry[]? queryParameters = null);
        IXPathQueryManager? GetXPathQueryManagerInContext(XPathExpression? xPath, DictionaryEntry[]? queryParameters = null);
        void ResetNamespaces();
        void SetDocumentToParse(Stream documentStream);
        void SetDocumentToParse(HttpClientUtil httpClientUtil);
    }
}
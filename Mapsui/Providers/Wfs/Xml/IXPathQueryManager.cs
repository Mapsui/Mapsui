// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

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
        XPathExpression Compile(string xPath);
        IXPathQueryManager Clone();
        string GetValueFromNode(XPathExpression xPath, DictionaryEntry[] queryParameters = null);
        IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath, DictionaryEntry[] queryParameters = null);
        void ResetNamespaces();
        void SetDocumentToParse(Stream documentStream);
        void SetDocumentToParse(HttpClientUtil httpClientUtil);
    }
}
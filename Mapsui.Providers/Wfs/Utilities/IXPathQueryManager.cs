// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// XPathQueryManager interface
    /// </summary>
    public interface IXPathQueryManager
    {
        void AddNamespace(string prefix, string ns);
        XPathExpression Compile(string xPath);
        IXPathQueryManager Clone();
        XPathNodeIterator GetIterator(XPathExpression xPath);
        XPathNodeIterator GetIterator(XPathExpression xPath, DictionaryEntry[] queryParameters);
        string GetValueFromNode(XPathExpression xPath);
        string GetValueFromNode(XPathExpression xPath, DictionaryEntry[] queryParameters);
        List<string> GetValuesFromNodes(XPathExpression xPath);
        List<string> GetValuesFromNodes(XPathExpression xPath, DictionaryEntry[] queryParameters);
        IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath);
        IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath, DictionaryEntry[] queryParameters);
        bool GetContextOfNextNode();
        bool GetContextOfNode(uint index);
        void ResetNamespaces();
        void ResetNavigator();
        void SetDocumentToParse(Stream documentStream);
        void SetDocumentToParse(byte[] document);
        void SetDocumentToParse(HttpClientUtil httpClientUtil);
        void SetDocumentToParse(string fileName);
    }
}
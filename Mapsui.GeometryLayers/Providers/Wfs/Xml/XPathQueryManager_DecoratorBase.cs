// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections;
using System.IO;
using System.Xml.XPath;
using Mapsui.Providers.Wfs.Utilities;

namespace Mapsui.Providers.Wfs.Xml
{
    /// <summary>
    /// This class should be the base class of all decorators for classes
    /// implementing <see cref="IXPathQueryManager"/>.
    /// </summary>
    public abstract class XPathQueryManagerDecoratorBase
    {

        protected IXPathQueryManager XPathQueryManager;



        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="xPathQueryManager">An instance implementing <see cref="IXPathQueryManager"/> to operate on</param>
        protected XPathQueryManagerDecoratorBase(IXPathQueryManager xPathQueryManager)
        {
            XPathQueryManager = xPathQueryManager;
        }



        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="prefix">A namespace prefix</param>
        /// <param name="ns">A namespace URI</param>
        public virtual void AddNamespace(string prefix, string ns)
        {
            XPathQueryManager.AddNamespace(prefix, ns);
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="xPath">An XPath string</param>
        /// <returns>A compiled XPath expression</returns>
        public virtual XPathExpression? Compile(string xPath)
        {
            return XPathQueryManager.Compile(xPath);
        }

        /// <summary>
        /// This method must be implemented specifically in each decorator.
        /// </summary>
        public abstract IXPathQueryManager Clone();

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        public virtual XPathNodeIterator? GetIterator(XPathExpression? xPath)
        {
            return XPathQueryManager.GetIterator(xPath);
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public virtual XPathNodeIterator? GetIterator(XPathExpression xPath, DictionaryEntry[] queryParameters)
        {
            return XPathQueryManager.GetIterator(xPath, queryParameters);
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public virtual string? GetValueFromNode(XPathExpression? xPath, DictionaryEntry[]? queryParameters = null)
        {
            if (xPath == null)
                return null;
            if (queryParameters == null) return XPathQueryManager.GetValueFromNode(xPath);
            return XPathQueryManager.GetValueFromNode(xPath, queryParameters);
        }

        /// <summary>
        /// This method must be implemented specifically in each decorator.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public abstract IXPathQueryManager? GetXPathQueryManagerInContext(XPathExpression xPath,
                                                                         DictionaryEntry[]? queryParameters = null);

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        public virtual void ResetNamespaces()
        {
            XPathQueryManager.ResetNamespaces();
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="documentStream">A Stream with XML data</param>
        public virtual void SetDocumentToParse(Stream documentStream)
        {
            XPathQueryManager.SetDocumentToParse(documentStream);
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        public virtual void SetDocumentToParse(HttpClientUtil httpClientUtil)
        {
            XPathQueryManager.SetDocumentToParse(httpClientUtil);
        }

    }
}
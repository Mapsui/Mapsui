// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections;
using System.IO;
using System.Xml.XPath;

namespace Mapsui.Providers.Wfs.Utilities
{
    /// <summary>
    /// This class should be the base class of all decorators for classes
    /// implementing <see cref="IXPathQueryManager"/>.
    /// </summary>
    public abstract class XPathQueryManager_DecoratorBase
    {
        #region Fields

        protected IXPathQueryManager _XPathQueryManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="xPathQueryManager">An instance implementing <see cref="IXPathQueryManager"/> to operate on</param>
        protected XPathQueryManager_DecoratorBase(IXPathQueryManager xPathQueryManager)
        {
            _XPathQueryManager = xPathQueryManager;
        }

        #endregion

        #region Public Member

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="prefix">A namespace prefix</param>
        /// <param name="ns">A namespace URI</param>
        public virtual void AddNamespace(string prefix, string ns)
        {
            _XPathQueryManager.AddNamespace(prefix, ns);
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="xPath">An XPath string</param>
        /// <returns>A compiled XPath expression</returns>
        public virtual XPathExpression Compile(string xPath)
        {
            return _XPathQueryManager.Compile(xPath);
        }

        /// <summary>
        /// This method must be implemented specifically in each decorator.
        /// </summary>
        public abstract IXPathQueryManager Clone();


        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public virtual string GetValueFromNode(XPathExpression xPath, DictionaryEntry[] queryParameters = null)
        {
            if (queryParameters == null) return _XPathQueryManager.GetValueFromNode(xPath);
            else return _XPathQueryManager.GetValueFromNode(xPath, queryParameters);
        }

        /// <summary>
        /// This method must be implemented specifically in each decorator.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public abstract IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath,
                                                                         DictionaryEntry[] queryParameters = null);

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        public virtual void ResetNamespaces()
        {
            _XPathQueryManager.ResetNamespaces();
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="documentStream">A Stream with XML data</param>
        public virtual void SetDocumentToParse(Stream documentStream)
        {
            _XPathQueryManager.SetDocumentToParse(documentStream);
        }

        /// <summary>
        /// This method invokes the corresponding method of the inherent <see cref="IXPathQueryManager"/> instance.
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        public virtual void SetDocumentToParse(HttpClientUtil httpClientUtil)
        {
            _XPathQueryManager.SetDocumentToParse(httpClientUtil);
        }

        #endregion
    }
}
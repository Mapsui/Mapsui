// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Mapsui.Providers.Wfs.Xml
{
    /// <summary>
    /// This class is a decorator for classes implementing <see cref="IXPathQueryManager"/>.
    /// It stores compiled XPath expressions for re-use.
    /// </summary>
    public class XPathQueryManagerCompiledExpressionsDecorator
        : XPathQueryManagerDecoratorBase, IXPathQueryManager
    {
        
        private static readonly Dictionary<string, XPathExpression> CompiledExpressions =
            new Dictionary<string, XPathExpression>();

        private static readonly NameTable NameTable = new NameTable();

        
        
        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManagerCompiledExpressionsDecorator"/> class.
        /// </summary>
        /// <param name="xPathQueryManager">An instance implementing <see cref="IXPathQueryManager"/> to operate on</param>
        public XPathQueryManagerCompiledExpressionsDecorator(IXPathQueryManager xPathQueryManager)
            : base(xPathQueryManager)
        {
        }

        
        
        /// <summary>
        /// This method compiles an XPath string, if not already saved.  
        /// Otherwise it returns the available XPath compilation. 
        /// </summary>
        /// <param name="xPath">The XPath string</param>
        /// <returns>A compiled XPath expression</returns>
        public override XPathExpression Compile(string xPath)
        {
            XPathExpression expr;
            // Compare pointers instead of literal values
            if (ReferenceEquals(xPath, NameTable.Get(xPath)))
                return CompiledExpressions[xPath];

            NameTable.Add(xPath);
            CompiledExpressions.Add(xPath, (expr = XPathQueryManager.Compile(xPath)));
            return expr;
        }

        /// <summary>
        /// This method returns a clone of the current instance.
        /// The cloned instance operates on the same (read-only) XPathDocument instance.
        /// </summary>
        public override IXPathQueryManager Clone()
        {
            return new XPathQueryManagerCompiledExpressionsDecorator(XPathQueryManager.Clone());
        }



        /// <summary>
        /// This method returns an instance of <see cref="XPathQueryManagerCompiledExpressionsDecorator"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public override IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath,
                                                                         DictionaryEntry[] queryParameters = null)
        {
            IXPathQueryManager xPathQueryManager = (queryParameters == null)
                                                       ? XPathQueryManager.GetXPathQueryManagerInContext(xPath)
                                                       : XPathQueryManager.GetXPathQueryManagerInContext(xPath,
                                                                                                          queryParameters);


            if (xPathQueryManager == null) return null;
            return new XPathQueryManagerCompiledExpressionsDecorator(xPathQueryManager);
        }

            }
}

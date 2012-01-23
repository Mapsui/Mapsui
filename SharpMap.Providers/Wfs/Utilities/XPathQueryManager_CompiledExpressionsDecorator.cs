// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class is a decorator for classes implementing <see cref="IXPathQueryManager"/>.
    /// It stores compiled XPath expressions for re-use.
    /// </summary>
    public class XPathQueryManager_CompiledExpressionsDecorator
        : XPathQueryManager_Decorator, IXPathQueryManager
    {
        #region Fields

        private static readonly Dictionary<string, XPathExpression> _CompiledExpressions =
            new Dictionary<string, XPathExpression>();

        private static readonly NameTable _NameTable = new NameTable();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager_CompiledExpressionsDecorator"/> class.
        /// </summary>
        /// <param name="xPathQueryManager">An instance implementing <see cref="IXPathQueryManager"/> to operate on</param>
        public XPathQueryManager_CompiledExpressionsDecorator(IXPathQueryManager xPathQueryManager)
            : base(xPathQueryManager)
        {
        }

        #endregion

        #region IXPathQueryManager Member

        //// <summary>
        /// This method compiles an XPath string, if not already saved.  
        /// Otherwise it returns the available XPath compilation. 
        /// </summary>
        /// <param name="xPath">The XPath string</param>
        /// <returns>A compiled XPath expression</returns>
        public override XPathExpression Compile(string xPath)
        {
            XPathExpression expr;
            // Compare pointers instead of literal values
            if (ReferenceEquals(xPath, _NameTable.Get(xPath)))
                return _CompiledExpressions[xPath];
            else
            {
                _NameTable.Add(xPath);
                _CompiledExpressions.Add(xPath, (expr = _XPathQueryManager.Compile(xPath)));
                return expr;
            }
        }

        /// <summary>
        /// This method returns a clone of the current instance.
        /// The cloned instance operates on the same (read-only) XPathDocument instance.
        /// </summary>
        public override IXPathQueryManager Clone()
        {
            return new XPathQueryManager_CompiledExpressionsDecorator(_XPathQueryManager.Clone());
        }

        /// <summary>
        /// This method returns an instance of <see cref="XPathQueryManager_CompiledExpressionsDecorator"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        public override IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath)
        {
            IXPathQueryManager xPathQueryManager = _XPathQueryManager.GetXPathQueryManagerInContext(xPath);
            if (xPathQueryManager == null) return null;
            return new XPathQueryManager_CompiledExpressionsDecorator(xPathQueryManager);
        }

        /// <summary>
        /// This method returns an instance of <see cref="XPathQueryManager_CompiledExpressionsDecorator"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public override IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath,
                                                                         DictionaryEntry[] queryParameters)
        {
            IXPathQueryManager xPathQueryManager = _XPathQueryManager.GetXPathQueryManagerInContext(xPath,
                                                                                                    queryParameters);
            if (xPathQueryManager == null) return null;
            return new XPathQueryManager_CompiledExpressionsDecorator(xPathQueryManager);
        }

        /// <summary>
        /// This method moves the current instance of <see cref="XPathQueryManager_CompiledExpressionsDecorator"/> 
        /// to the context of the next node a previously handed over XPath expression addresses.
        /// </summary>
        public override bool GetContextOfNextNode()
        {
            return _XPathQueryManager.GetContextOfNextNode();
        }

        /// <summary>
        /// This method moves the current instance of <see cref="XPathQueryManager_CompiledExpressionsDecorator"/> 
        /// to the context of node[index] of current position.
        /// </summary>
        /// <param name="index">The index of the node to search</param>
        public override bool GetContextOfNode(uint index)
        {
            return _XPathQueryManager.GetContextOfNode(index);
        }

        #endregion
    }
}
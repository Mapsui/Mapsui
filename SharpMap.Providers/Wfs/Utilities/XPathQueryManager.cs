// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class provides an easy-to-use interface for complex (parameterized) XPath queries.  
    /// </summary>
    public class XPathQueryManager : IXPathQueryManager
    {
        #region Fields

        private int _NavDiff = -1;
        private CustomQueryContext _ParamContext;
        private XPathNodeIterator _XIter;
        private XPathNavigator _XNav;
        private XPathDocument _XPathDoc;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
        /// </summary>
        public XPathQueryManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
        /// </summary>
        /// <param name="documentStream">A Stream with XML data</param>
        public XPathQueryManager(Stream documentStream)
        {
            SetDocumentToParse(documentStream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
        /// </summary>
        /// <param name="document">A byte array with XML data</param>
        public XPathQueryManager(byte[] document)
        {
            SetDocumentToParse(document);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
        /// </summary>
        /// <param name="xPathDoc">An XmlDocument instance</param>
        public XPathQueryManager(XPathDocument xPathDoc)
        {
            SetDocumentToParse(xPathDoc);
            _ParamContext = new CustomQueryContext(new NameTable());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class. 
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        public XPathQueryManager(HttpClientUtil httpClientUtil)
        {
            SetDocumentToParse(httpClientUtil);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class. 
        /// </summary>
        /// <param name="fileName"></param>
        public XPathQueryManager(string fileName)
        {
            SetDocumentToParse(fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
        /// </summary>
        /// <param name="xPathDoc">An XmlDocument instance</param>
        /// <param name="paramContext">A <see cref="XPathQueryManager.CustomQueryContext"/> instance for parameterized XPath expressions</param>
        private XPathQueryManager(XPathDocument xPathDoc, XPathNavigator xNav, CustomQueryContext paramContext)
        {
            _XNav = xNav.Clone();
            SetDocumentToParse(xPathDoc);
            InitializeCustomContext(paramContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
        /// </summary>
        /// <param name="xPathDoc">An XmlDocument instance</param>
        /// <param name="xIter">An XPathNodeIterator instance</param
        /// <param name="paramContext">A <see cref="XPathQueryManager.CustomQueryContext"/> instance for parameterized XPath expressions</param>
        private XPathQueryManager(XPathDocument xPathDoc, XPathNodeIterator xIter, CustomQueryContext paramContext)
            : this(xPathDoc)
        {
            if (xIter != null)
                _XNav = xIter.Current;
            InitializeCustomContext(paramContext);
        }

        #endregion

        #region IXPathQueryManager Member

        /// <summary>
        /// This method adds a namespace for XPath queries.
        /// </summary>
        /// <param name="prefix">The namespace prefix</param>
        /// <param name="ns">The namespace URI</param>
        public void AddNamespace(string prefix, string ns)
        {
            _ParamContext.AddNamespace(prefix, ns);
        }

        /// <summary>
        /// This method compiles an XPath string.
        /// </summary>
        /// <param name="xPath">The XPath string</param>
        /// <returns>A compiled XPath expression</returns>
        public XPathExpression Compile(string xPath)
        {
            return _XNav.Compile(xPath);
        }

        /// <summary>
        /// This method returns a clone of the current instance.
        /// The cloned instance operates on the same (read-only) XmlDocument instance.
        /// </summary>
        public IXPathQueryManager Clone()
        {
            return new XPathQueryManager(_XPathDoc, _XNav, _ParamContext);
        }

        /// <summary>
        /// This method returns an XPathNodeIterator instance positioned at the nodes 
        /// the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        public XPathNodeIterator GetIterator(XPathExpression xPath)
        {
            findXPath(xPath);
            return _XIter.Clone();
        }

        /// <summary>
        /// This method returns an XPathNodeIterator instance positioned at the nodes 
        /// the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public XPathNodeIterator GetIterator(XPathExpression xPath, DictionaryEntry[] queryParameters)
        {
            _ParamContext.AddParam(queryParameters);
            return GetIterator(xPath);
        }

        /// <summary>
        /// This method returns the value of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        public string GetValueFromNode(XPathExpression xPath)
        {
            string result = null;
            findXPath(xPath);
            if (_XIter.MoveNext())
                result = _XIter.Current.Value;
            return result;
        }

        /// <summary>
        /// This method returns the value of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public string GetValueFromNode(XPathExpression xPath, DictionaryEntry[] queryParameters)
        {
            _ParamContext.AddParam(queryParameters);
            return GetValueFromNode(xPath);
        }

        /// <summary>
        /// This method returns a collection of the values of all nodes the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        public List<string> GetValuesFromNodes(XPathExpression xPath)
        {
            List<string> valuesList = new List<string>();
            findXPath(xPath);
            while (_XIter.MoveNext())
                valuesList.Add(_XIter.Current.ToString());
            return valuesList;
        }

        /// <summary>
        /// This method returns a collection of the values of all nodes the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public List<string> GetValuesFromNodes(XPathExpression xPath, DictionaryEntry[] queryParameters)
        {
            _ParamContext.AddParam(queryParameters);
            return GetValuesFromNodes(xPath);
        }

        /// <summary>
        /// This method returns an instance of <see cref="XPathQueryManager"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        public IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath)
        {
            findXPath(xPath);
            if (_XIter.MoveNext())
                return new XPathQueryManager(_XPathDoc, _XIter, _ParamContext);
            return null;
        }

        /// <summary>
        /// This method returns an instance of <see cref="XPathQueryManager"/> 
        /// in the context of the first node the XPath expression addresses.
        /// </summary>
        /// <param name="xPath">The compiled XPath expression</param>
        /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
        public IXPathQueryManager GetXPathQueryManagerInContext(XPathExpression xPath, DictionaryEntry[] queryParameters)
        {
            _ParamContext.AddParam(queryParameters);
            findXPath(xPath);
            if (_XIter.MoveNext())
                return new XPathQueryManager(_XPathDoc, _XIter, _ParamContext);
            return null;
        }

        /// <summary>
        /// This method moves the current instance of <see cref="XPathQueryManager"/> 
        /// to the context of the next node a previously handed over XPath expression addresses.
        /// </summary>
        public bool GetContextOfNextNode()
        {
            return GetContextOfNode((uint) _NavDiff + 1);
        }

        /// <summary>
        /// This method moves the current instance of <see cref="XPathQueryManager"/> 
        /// to the context of node[index] of current position.
        /// </summary>
        /// <param name="index">The index of the node to search</param>
        public bool GetContextOfNode(uint index)
        {
            if (_NavDiff == -1) ++_NavDiff;
            while (_NavDiff < index)
            {
                if (!_XNav.MoveToNext()) break;
                _NavDiff++;
            }
            while (_NavDiff > index)
            {
                _XNav.MoveToPrevious();
                _NavDiff--;
            }
            if (_NavDiff == index)
                return true;
            else
            {
                ResetNavigator();
                return false;
            }
        }

        /// <summary>
        /// This method deletes the current namespace context.
        /// </summary>
        public void ResetNamespaces()
        {
            _ParamContext = null;
        }

        /// <summary>
        /// This method resets the inherent XPathNavigator instance.
        /// </summary>
        public void ResetNavigator()
        {
            GetContextOfNode(0);
            _NavDiff--;
        }

        /// <summary>
        /// Sets a new XML document. 
        /// </summary>
        /// <param name="documentStream">A Stream with XML data</param>
        public void SetDocumentToParse(Stream documentStream)
        {
            initializeXPathObjects(documentStream);
        }

        /// <summary>
        /// Sets a new XML document. 
        /// </summary>
        /// <param name="document">A byte array with XML data</param>
        public void SetDocumentToParse(byte[] document)
        {
            initializeXPathObjects(new MemoryStream(document));
        }

        /// <summary>
        /// Sets a new XML document. 
        /// </summary>
        /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
        public void SetDocumentToParse(HttpClientUtil httpClientUtil)
        {
            try
            {
                initializeXPathObjects(httpClientUtil.GetDataStream());
            }
            finally
            {
                httpClientUtil.Close();
            }
        }

        /// <summary>
        /// Sets a new XmlDocument
        /// </summary>
        /// <param name="fileName"></param>
        public void SetDocumentToParse(string fileName)
        {
            try
            {
                initializeXPathObjects(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured while reading the xml file: " + fileName);
                throw ex;
            }
        }

        #endregion

        #region Private Member

        /// <summary>
        /// Sets a new XML document.
        /// </summary>
        /// <param name="xPathDoc">An XPathDocument instance</param>
        private void SetDocumentToParse(XPathDocument xPathDoc)
        {
            _XPathDoc = xPathDoc;
            if (_XNav == null) _XNav = _XPathDoc.CreateNavigator().Clone();
        }

        /// <summary>
        /// This method does some XPath specific initializations.
        /// </summary>
        private void initializeXPathObjects(Stream xmlStream)
        {
            try
            {
                _XPathDoc = new XPathDocument(xmlStream);
                _XNav = _XPathDoc.CreateNavigator();
                _ParamContext = new CustomQueryContext(new NameTable());
            }
            catch (XmlException ex)
            {
                Trace.TraceError("An XML specific exception occured " +
                                 "while initializing XPathDocument and XPathNavigator in XPathQueryManager: " +
                                 ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occured " +
                                 "while initializing XPathDocument and XPathNavigator in XPathQueryManager: " +
                                 ex.Message);
                throw ex;
            }
            finally
            {
                xmlStream.Dispose();
            }
        }

        /// <summary>
        /// This method sets the inherent XPathNodeIterator instance.
        /// </summary>
        /// <param name="xPath">A compiled XPath expression</param>
        private void findXPath(XPathExpression xPath)
        {
            xPath.SetContext(_ParamContext);
            _XIter = _XNav.Select(xPath);
            InitializeCustomContext(_ParamContext);
        }

        private void InitializeCustomContext(CustomQueryContext paramContext)
        {
            IDictionary<string, string> namespaces = paramContext.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            _ParamContext = new CustomQueryContext((NameTable) paramContext.NameTable);
            _ParamContext.AddNamespace(namespaces);
        }

        #endregion

        #region Nested Types

        #region CustomQueryContext

        /// <summary>
        /// This class represents a custom context for XPath queries.
        /// It is derived from XsltContext.
        /// </summary>
        public class CustomQueryContext : XsltContext
        {
            #region Fields

            private XsltArgumentList _ArgumentList = new XsltArgumentList();

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomQueryContext"/> class.
            /// </summary>
            public CustomQueryContext()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomQueryContext"/> class.
            /// </summary>
            /// <param name="table">A NameTable instance</param>
            public CustomQueryContext(NameTable table)
                : base(table)
            {
            }

            #endregion

            #region Public Member

            /// <summary>
            /// Method from XsltContext.
            /// </summary>
            public override bool Whitespace
            {
                get { return true; }
            }

            /// <summary>
            /// This method adds a list of namespaces to use in the custom context.
            /// </summary>
            /// <param name="namespaces">A list of namespaces</param>
            public void AddNamespace(IDictionary<string, string> namespaces)
            {
                foreach (string ns in namespaces.Keys)
                    AddNamespace(ns, namespaces[ns]);
            }

            /// <summary>
            /// Method from XsltContext.
            /// </summary>
            public override int CompareDocument(string baseUri, string nextbaseUri)
            {
                return 0;
            }

            /// <summary>
            /// Method from XsltContext.
            /// </summary>
            public override bool PreserveWhitespace(XPathNavigator node)
            {
                return true;
            }

            /// <summary>
            /// This method resolves a function appearing in an XPath expression.
            /// </summary>
            /// <param name="prefix">The prefix of the function</param>
            /// <param name="name">The name of the function</param>
            /// <param name="ArgTypes">A list of argument types of the function</param>
            public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
            {
                if (name.Equals(ParamCompare.FunctionName)) return new ParamCompare(ArgTypes, 2, 2);
                if (name.Equals(ParamCompareWithTargetNs.FunctionName))
                    return new ParamCompareWithTargetNs(ArgTypes, 3, 3);
                return null;
            }

            /// <summary>
            /// This method resolves a variable appearing in an XPath expression.
            /// </summary>
            /// <param name="prefix">The prefix of the variable</param>
            /// <param name="name">The name of the variable</param>
            public override IXsltContextVariable ResolveVariable(string prefix, string name)
            {
                object param = GetParam(name);
                if (param != null)
                    return new ParamFunctionVar(name, param);
                return null;
            }

            /// <summary>
            /// This method adds a parameter to the custom context.
            /// </summary>
            /// <param name="name">The name of the parameter</param>
            /// <param name="parameter">The value of the parameter</param>
            public void AddParam(string name, object parameter)
            {
                _ArgumentList.AddParam(name, string.Empty, parameter);
            }

            /// <summary>
            /// This method adds a list of parameters to the custom context.
            /// </summary>
            /// <param name="parameters">A list of parameters</param>
            public void AddParam(DictionaryEntry[] parameters)
            {
                int length = parameters.Length;
                for (int i = 0; i < length; i++)
                    _ArgumentList.AddParam(parameters[i].Key.ToString(),
                                           string.Empty, parameters[i].Value.ToString());
            }

            /// <summary>
            /// This method gets a parameter by name.
            /// </summary>
            /// <param name="name">The name of the parameter</param>
            public object GetParam(string name)
            {
                return _ArgumentList.GetParam(name, string.Empty);
            }

            /// <summary>
            /// This method removes a parameter from the inherent parameter list.
            /// </summary>
            /// <param name="name">The name of the parameter</param>
            public object RemoveParam(string name)
            {
                return _ArgumentList.RemoveParam(name, string.Empty);
            }

            /// <summary>
            /// This methods clears the inherent parameter list.
            /// </summary>
            public void ResetParams()
            {
                _ArgumentList.Clear();
            }

            #endregion
        }

        #endregion

        #region ParamBase

        /// <summary>
        /// This class is the base class of <see cref="ParamCompare"/> and <see cref="ParamCompareWithTargetNs"/>.
        /// </summary>
        public abstract class ParamBase
        {
            #region Fields

            private XPathResultType[] _ArgTypes;
            private int _MaxArgs;
            private int _MinArgs;
            private XPathResultType _ReturnType;

            #endregion

            #region Properties

            /// <summary>
            /// Gets the argument types.
            /// </summary>
            public XPathResultType[] ArgTypes
            {
                get { return _ArgTypes; }
            }

            /// <summary>
            /// Gets the return type.
            /// </summary>
            public XPathResultType ReturnType
            {
                get { return _ReturnType; }
            }

            /// <summary>
            /// Gets the minimum number of arguments allowed.
            /// </summary>
            public int Minargs
            {
                get { return _MinArgs; }
            }

            /// <summary>
            /// Gets the maximum number of arguments allowed.
            /// </summary>
            public int Maxargs
            {
                get { return _MaxArgs; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Protected constructor for the abstract class.
            /// </summary>
            /// <param name="argTypes">The argument types of the function</param>
            /// <param name="returnType">The return type of the function</param>
            /// <param name="minArgs">The minimum number of arguments allowed</param>
            /// <param name="maxArgs">The maximum number of arguments allowed</param>
            protected ParamBase(XPathResultType[] argTypes, XPathResultType returnType,
                                int minArgs, int maxArgs)
            {
                _ArgTypes = argTypes;
                _ReturnType = returnType;
                _MinArgs = minArgs;
                _MaxArgs = maxArgs;
            }

            #endregion
        }

        #endregion

        #region ParamCompare

        /// <summary>
        /// This class performs a string comparison in an XPath expression.
        /// </summary>
        public class ParamCompare : ParamBase, IXsltContextFunction
        {
            #region Fields

            /// <summary>
            /// The name to use when embedding the function in an XPath expression.
            /// </summary>
            public static readonly string FunctionName = "_PARAMCOMP_";

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamCompare"/> class.
            /// </summary>
            /// <param name="argTypes">The argument types of the function</param>
            /// <param name="minArgs">The minimum number of arguments allowed</param>
            /// <param name="maxArgs">The maximum number of arguments allowed</param>
            public ParamCompare(XPathResultType[] argTypes, int minArgs, int maxArgs)
                : base(argTypes, XPathResultType.Boolean, minArgs, maxArgs)
            {
            }

            #endregion

            #region IXsltContextFunction Member

            /// <summary>
            /// This method performs a string comparison.
            /// </summary>
            /// <param name="xsltContext">The Xslt context</param>
            /// <param name="args">The arguments of the function</param>
            /// <param name="docContext">The document context</param>
            /// <returns>A boolean value indicating whether the argument strings are identical</returns>
            public virtual object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
            {
                return resolveNsPrefix(ResolveArgument(args[0]), xsltContext).Equals(
                    resolveNsPrefix(ResolveArgument(args[1]), xsltContext), StringComparison.Ordinal);
            }

            #endregion

            #region Protected Member

            /// <summary>
            /// This method creates a string from an object argument.
            /// In many cases the argument is an XPathNodeIterator that must be resolved.
            /// </summary>
            /// <param name="arg">An argument of the function to be resolved</param>
            protected string ResolveArgument(object arg)
            {
                if (arg is string)
                    return arg.ToString();
                else if (arg is XPathNodeIterator)
                    if (((XPathNodeIterator) arg).MoveNext() == true)
                        return ((XPathNodeIterator) arg).Current.Value;
                return string.Empty;
            }

            #endregion

            #region Private Member

            /// <summary>
            /// This method resolves the prefix of an argument.
            /// If a prefix is found, the corresponding namespace URI is looked up 
            /// and substituted.
            /// </summary>
            /// <param name="args">An argument of the function to be resolved</param>
            /// <param name="xsltContext">The Xslt context for namespace resolving</param>
            private string resolveNsPrefix(string args, XsltContext xsltContext)
            {
                string prefix;
                string ns;
                if (args.Contains(":"))
                {
                    prefix = args.Substring(0, args.IndexOf(":"));
                    if (!string.IsNullOrEmpty((ns = xsltContext.LookupNamespace(prefix))))
                        args = args.Replace(prefix + ":", ns);
                }
                return args;
            }

            #endregion
        }

        #endregion

        #region ParamCompareWithTargetNs

        /// <summary>
        /// This class performs a string comparison in an XPath expression.
        /// It is specifically created for using in XML schema documents.
        /// </summary>
        public class ParamCompareWithTargetNs : ParamCompare
        {
            #region Fields

            /// <summary>
            /// The name to use when embedding the function in an XPath expression.
            /// </summary>
            public static readonly string FunctionName = "_PARAMCOMPWITHTARGETNS_";

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamCompareWithTargetNs"/> class.
            /// </summary>
            /// <param name="argTypes">The argument types of the function</param>
            /// <param name="minArgs">The minimum number of arguments allowed</param>
            /// <param name="maxArgs">The maximum number of arguments allowed</param>
            public ParamCompareWithTargetNs(XPathResultType[] argTypes, int minArgs, int maxArgs)
                : base(argTypes, minArgs, maxArgs)
            {
            }

            #endregion

            #region IXsltContextFunction Member

            /// <summary>
            /// This method performs a string comparison.
            /// </summary>
            /// <param name="xsltContext">The Xslt context</param>
            /// <param name="args">The arguments of the function</param>
            /// <param name="docContext">The document context</param>
            /// <returns>A boolean value indicating whether the argument strings are identical</returns>
            public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
            {
                return ((string) ((string) args[1] + ResolveArgument(args[2]))).Equals(
                    resolveNsPrefix(ResolveArgument(args[0]), (string) args[1], docContext), StringComparison.Ordinal);
            }

            #endregion

            #region Private Member

            /// <summary>
            /// This method resolves the prefix of an argument.
            /// If a prefix is found, the corresponding namespace URI (if existing) is looked up 
            /// and substituted. Otherwise the target namespace is placed first.
            /// </summary>
            /// <param name="args">An argument of the function to be resolved</param>
            /// <param name="xsltContext">The Xslt context for namespace resolving</param>
            private string resolveNsPrefix(string args, string targetNs, XPathNavigator docContext)
            {
                string prefix;
                string ns;
                if (args.Contains(":"))
                {
                    prefix = args.Substring(0, args.IndexOf(":"));
                    if (!string.IsNullOrEmpty((ns = docContext.LookupNamespace(prefix))))
                        return args = args.Replace(prefix + ":", ns);
                    return targetNs + args;
                }
                return targetNs + args;
            }

            #endregion
        }

        #endregion

        #region ParamFunctionVar

        /// <summary>
        /// This class represents a variable in an XPath expression.
        /// </summary>
        public class ParamFunctionVar : IXsltContextVariable
        {
            #region Fields

            private string _Name;
            private object _Param;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ParamFunctionVar"/> class.
            /// </summary>
            /// <param name="name">The name of the variable</param>
            /// <param name="param">The parameter</param>
            public ParamFunctionVar(string name, object param)
            {
                _Name = name;
                _Param = param;
            }

            #endregion

            #region IXsltContextVariable Member

            /// <summary>
            /// Method implementing IXsltContextVariable
            /// </summary>
            public object Evaluate(XsltContext xsltContext)
            {
                return _Param;
            }

            /// <summary>
            /// Method implementing IXsltContextVariable
            /// </summary>
            public bool IsLocal
            {
                get { return true; }
            }

            /// <summary>
            /// Method implementing IXsltContextVariable
            /// </summary>
            public bool IsParam
            {
                get { return true; }
            }

            /// <summary>
            /// Method implementing IXsltContextVariable
            /// </summary>
            public XPathResultType VariableType
            {
                get { return XPathResultType.Any; }
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
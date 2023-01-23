// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mapsui.Logging;
using Mapsui.Providers.Wfs.Utilities;

namespace Mapsui.Providers.Wfs.Xml;

/// <summary>
/// This class provides an easy-to-use interface for complex (parameterized) XPath queries.  
/// </summary>
public class XPathQueryManager : IXPathQueryManager
{

    private CustomQueryContext? _paramContext;
    private XPathNodeIterator? _xIter;
    private XPathNavigator? _xNav;
    private XPathDocument? _xPathDoc;
    private bool _initialized;
    private HttpClientUtil? _httpClientUtil;



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
    public XPathQueryManager(XPathDocument? xPathDoc)
    {
        SetDocumentToParse(xPathDoc);
        _paramContext = new CustomQueryContext(new NameTable());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPathQueryManager"/> class. 
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    public XPathQueryManager(HttpClientUtil httpClientUtil)
    {
        _httpClientUtil = httpClientUtil;
    }

    /// <summary>Init Async</summary>
    /// <returns>Task</returns>
    public async Task InitAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        if (_httpClientUtil != null)
            await SetDocumentToParseAsync(_httpClientUtil);
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
    /// <param name="xNav"></param>
    /// <param name="paramContext">A <see cref="XPathQueryManager.CustomQueryContext"/> instance for parameterized XPath expressions</param>
    private XPathQueryManager(XPathDocument? xPathDoc, XPathNavigator? xNav, CustomQueryContext? paramContext)
    {
        _xNav = xNav?.Clone();
        SetDocumentToParse(xPathDoc);
        InitializeCustomContext(paramContext);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPathQueryManager"/> class.
    /// </summary>
    /// <param name="xPathDoc">An XmlDocument instance</param>
    /// <param name="xIter">An XPathNodeIterator instance</param>
    /// <param name="paramContext">A <see cref="XPathQueryManager.CustomQueryContext"/> instance for parameterized XPath expressions</param>
    private XPathQueryManager(XPathDocument? xPathDoc, XPathNodeIterator? xIter, CustomQueryContext? paramContext)
        : this(xPathDoc)
    {
        if (xIter != null)
            _xNav = xIter.Current;
        InitializeCustomContext(paramContext);
    }



    /// <summary>
    /// This method adds a namespace for XPath queries.
    /// </summary>
    /// <param name="prefix">The namespace prefix</param>
    /// <param name="ns">The namespace URI</param>
    public void AddNamespace(string prefix, string ns)
    {
        if (_paramContext != null)
            _paramContext.AddNamespace(prefix, ns);
    }

    /// <summary>
    /// This method compiles an XPath string.
    /// </summary>
    /// <param name="xPath">The XPath string</param>
    /// <returns>A compiled XPath expression</returns>
    public XPathExpression? Compile(string xPath)
    {
        return _xNav?.Compile(xPath);
    }

    /// <summary>
    /// This method returns a clone of the current instance.
    /// The cloned instance operates on the same (read-only) XmlDocument instance.
    /// </summary>
    public IXPathQueryManager Clone()
    {
        return new XPathQueryManager(_xPathDoc, _xNav, _paramContext);
    }

    /// <summary>
    /// This method returns an XPathNodeIterator instance positioned at the nodes 
    /// the XPath expression addresses.
    /// </summary>
    /// <param name="xPath">The compiled XPath expression</param>
    public XPathNodeIterator? GetIterator(XPathExpression? xPath)
    {
        FindXPath(xPath);
        return _xIter?.Clone();
    }

    /// <summary>
    /// This method returns an XPathNodeIterator instance positioned at the nodes 
    /// the XPath expression addresses.
    /// </summary>
    /// <param name="xPath">The compiled XPath expression</param>
    /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
    public XPathNodeIterator? GetIterator(XPathExpression xPath, DictionaryEntry[] queryParameters)
    {
        if (_paramContext != null)
            _paramContext.AddParam(queryParameters);
        return GetIterator(xPath);
    }

    /// <summary>
    /// This method returns the value of the first node the XPath expression addresses.
    /// </summary>
    /// <param name="xPath">The compiled XPath expression</param>
    /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
    public string? GetValueFromNode(XPathExpression? xPath, DictionaryEntry[]? queryParameters = null)
    {
        if (queryParameters != null && _paramContext != null)
            _paramContext.AddParam(queryParameters);
        string? result = null;
        FindXPath(xPath);
        if (_xIter?.MoveNext() ?? false)
            result = _xIter?.Current?.Value;
        return result;
    }

    /// <summary>
    /// This method returns a collection of the values of all nodes the XPath expression addresses.
    /// </summary>
    /// <param name="xPath">The compiled XPath expression</param>
    public List<string> GetValuesFromNodes(XPathExpression xPath)
    {
        var valuesList = new List<string>();
        FindXPath(xPath);
        while (_xIter?.MoveNext() ?? false)
            if (_xIter.Current != null)
                valuesList.Add(_xIter.Current.ToString());
        return valuesList;
    }

    /// <summary>
    /// This method returns a collection of the values of all nodes the XPath expression addresses.
    /// </summary>
    /// <param name="xPath">The compiled XPath expression</param>
    /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
    public List<string> GetValuesFromNodes(XPathExpression xPath, DictionaryEntry[] queryParameters)
    {
        if (_paramContext != null)
            _paramContext.AddParam(queryParameters);
        return GetValuesFromNodes(xPath);
    }

    /// <summary>
    /// This method returns an instance of <see cref="XPathQueryManager"/> 
    /// in the context of the first node the XPath expression addresses.
    /// </summary>
    /// <param name="xPath">The compiled XPath expression</param>
    /// <param name="queryParameters">Parameters for the compiled XPath expression</param>
    public IXPathQueryManager? GetXPathQueryManagerInContext(XPathExpression? xPath, DictionaryEntry[]? queryParameters = null)
    {
        if (queryParameters != null && _paramContext != null) _paramContext.AddParam(queryParameters);
        FindXPath(xPath);
        if (_xIter?.MoveNext() ?? false)
            return new XPathQueryManager(_xPathDoc, _xIter, _paramContext);
        return null;
    }

    /// <summary>
    /// This method deletes the current namespace context.
    /// </summary>
    public void ResetNamespaces()
    {
        _paramContext = null;
    }

    /// <summary>
    /// Sets a new XML document. 
    /// </summary>
    /// <param name="documentStream">A Stream with XML data</param>
    public void SetDocumentToParse(Stream documentStream)
    {
        InitializeXPathObjects(documentStream);
    }

    /// <summary>
    /// Sets a new XML document. 
    /// </summary>
    /// <param name="document">A byte array with XML data</param>
    public void SetDocumentToParse(byte[] document)
    {
        InitializeXPathObjects(new MemoryStream(document));
    }

    /// <summary>
    /// Sets a new XML document. 
    /// </summary>
    /// <param name="httpClientUtil">A configured <see cref="HttpClientUtil"/> instance for performing web requests</param>
    public async Task SetDocumentToParseAsync(HttpClientUtil httpClientUtil)
    {
        try
        {
            InitializeXPathObjects(await httpClientUtil.GetDataStreamAsync());
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, e.Message, e);
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
            InitializeXPathObjects(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occured while reading the xml file: " + fileName + ". " + ex.Message, ex);
            throw;
        }
    }



    /// <summary>
    /// Sets a new XML document.
    /// </summary>
    /// <param name="xPathDoc">An XPathDocument instance</param>
    private void SetDocumentToParse(XPathDocument? xPathDoc)
    {
        _xPathDoc = xPathDoc;
        _xNav ??= _xPathDoc?.CreateNavigator().Clone();
    }

    /// <summary>
    /// This method does some XPath specific initializations.
    /// </summary>
    private void InitializeXPathObjects(Stream? xmlStream)
    {
        if (xmlStream == null)
            return;

        try
        {
            _xPathDoc = new XPathDocument(xmlStream);
            _xNav = _xPathDoc.CreateNavigator();
            _paramContext = new CustomQueryContext(new NameTable());
        }
        catch (XmlException ex)
        {
            Logger.Log(LogLevel.Error, "An XML specific exception occured " +
                                       "while initializing XPathDocument and XPathNavigator in XPathQueryManager: " +
                                       ex.Message, ex);
            throw;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "An exception occured " +
                                       "while initializing XPathDocument and XPathNavigator in XPathQueryManager: " +
                                       ex.Message, ex);
            throw;
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
    private void FindXPath(XPathExpression? xPath)
    {
        xPath?.SetContext(_paramContext!);
        if (xPath != null)
            _xIter = _xNav?.Select(xPath);
        InitializeCustomContext(_paramContext);
    }

    private void InitializeCustomContext(CustomQueryContext? paramContext)
    {
        if (paramContext == null)
            return;
        var namespaces = paramContext.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
        _paramContext = new CustomQueryContext((NameTable)(paramContext.NameTable ?? new NameTable()));
        if (namespaces != null)
        {
            _paramContext.AddNamespace(namespaces);
        }
    }

    /// <summary>
    /// This class represents a custom context for XPath queries.
    /// It is derived from XsltContext.
    /// </summary>
    public class CustomQueryContext : XsltContext
    {

        private readonly XsltArgumentList _argumentList = new XsltArgumentList();



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



        /// <summary>
        /// Method from XsltContext.
        /// </summary>
        public override bool Whitespace => true;

        /// <summary>
        /// This method adds a list of namespaces to use in the custom context.
        /// </summary>
        /// <param name="namespaces">A list of namespaces</param>
        public void AddNamespace(IDictionary<string, string> namespaces)
        {
            foreach (var ns in namespaces.Keys)
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
        /// <param name="argTypes">A list of argument types of the function</param>
        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
        {
            if (name.Equals(ParamCompare.FunctionName)) return new ParamCompare(argTypes, 2, 2);
            if (name.Equals(ParamCompareWithTargetNs.FunctionName))
                return new ParamCompareWithTargetNs(argTypes, 3, 3);
#pragma warning disable CS8603
            return null; // seems to work
#pragma warning restore CS8603
        }

        /// <summary>
        /// This method resolves a variable appearing in an XPath expression.
        /// </summary>
        /// <param name="prefix">The prefix of the variable</param>
        /// <param name="name">The name of the variable</param>
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            var param = GetParam(name);
            if (param != null)
                return new ParamFunctionVar(param);
#pragma warning disable CS8603
            return null; // seems to work
#pragma warning restore CS8603
        }

        /// <summary>
        /// This method adds a parameter to the custom context.
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="parameter">The value of the parameter</param>
        public void AddParam(string name, object parameter)
        {
            _argumentList.AddParam(name, string.Empty, parameter);
        }

        /// <summary>
        /// This method adds a list of parameters to the custom context.
        /// </summary>
        /// <param name="parameters">A list of parameters</param>
        public void AddParam(DictionaryEntry[] parameters)
        {
            var length = parameters.Length;
            for (var i = 0; i < length; i++)
                _argumentList.AddParam(parameters[i].Key.ToString()!,
                                       string.Empty, parameters[i].Value?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// This method gets a parameter by name.
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        public object? GetParam(string name)
        {
            return _argumentList.GetParam(name, string.Empty);
        }

        /// <summary>
        /// This method removes a parameter from the inherent parameter list.
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        public object? RemoveParam(string name)
        {
            return _argumentList.RemoveParam(name, string.Empty);
        }

        /// <summary>
        /// This methods clears the inherent parameter list.
        /// </summary>
        public void ResetParams()
        {
            _argumentList.Clear();
        }

    }



    /// <summary>
    /// This class is the base class of <see cref="ParamCompare"/> and <see cref="ParamCompareWithTargetNs"/>.
    /// </summary>
    public abstract class ParamBase
    {

        private readonly XPathResultType[] _argTypes;
        private readonly int _maxArgs;
        private readonly int _minArgs;
        private readonly XPathResultType _returnType;



        /// <summary>
        /// Gets the argument types.
        /// </summary>
        public XPathResultType[] ArgTypes => _argTypes;

        /// <summary>
        /// Gets the return type.
        /// </summary>
        public XPathResultType ReturnType => _returnType;

        /// <summary>
        /// Gets the minimum number of arguments allowed.
        /// </summary>
        public int Minargs => _minArgs;

        /// <summary>
        /// Gets the maximum number of arguments allowed.
        /// </summary>
        public int Maxargs => _maxArgs;



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
            _argTypes = argTypes;
            _returnType = returnType;
            _minArgs = minArgs;
            _maxArgs = maxArgs;
        }

    }



    /// <summary>
    /// This class performs a string comparison in an XPath expression.
    /// </summary>
    public class ParamCompare : ParamBase, IXsltContextFunction
    {

        /// <summary>
        /// The name to use when embedding the function in an XPath expression.
        /// </summary>
        public static readonly string FunctionName = "_PARAMCOMP_";



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



        /// <summary>
        /// This method performs a string comparison.
        /// </summary>
        /// <param name="xsltContext">The Xslt context</param>
        /// <param name="args">The arguments of the function</param>
        /// <param name="docContext">The document context</param>
        /// <returns>A boolean value indicating whether the argument strings are identical</returns>
        public virtual object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return ResolveNsPrefix(ResolveArgument(args[0]), xsltContext)?.Equals(
                ResolveNsPrefix(ResolveArgument(args[1]), xsltContext), StringComparison.Ordinal) ?? false;
        }

        /// <summary>
        /// This method creates a string from an object argument.
        /// In many cases the argument is an XPathNodeIterator that must be resolved.
        /// </summary>
        /// <param name="arg">An argument of the function to be resolved</param>
        protected string? ResolveArgument(object arg)
        {
            if (arg is string)
                return arg.ToString();
            var iterator = arg as XPathNodeIterator;
            if (iterator != null)
            {
                if (iterator.MoveNext())
                    return iterator.Current?.Value;
            }
            return string.Empty;
        }



        /// <summary>
        /// This method resolves the prefix of an argument.
        /// If a prefix is found, the corresponding namespace URI is looked up 
        /// and substituted.
        /// </summary>
        /// <param name="args">An argument of the function to be resolved</param>
        /// <param name="xsltContext">The Xslt context for namespace resolving</param>
        private string? ResolveNsPrefix(string? args, XsltContext xsltContext)
        {
            if (args?.Contains(":") ?? false)
            {
                var prefix = args.Substring(0, args.IndexOf(":", StringComparison.Ordinal));
                string ns;
                if (!string.IsNullOrEmpty((ns = xsltContext.LookupNamespace(prefix) ?? string.Empty)))
                    args = args.Replace(prefix + ":", ns);
            }
            return args;
        }

    }



    /// <summary>
    /// This class performs a string comparison in an XPath expression.
    /// It is specifically created for using in XML schema documents.
    /// </summary>
    public class ParamCompareWithTargetNs : ParamCompare
    {

        /// <summary>
        /// The name to use when embedding the function in an XPath expression.
        /// </summary>
        public static readonly new string FunctionName = "_PARAMCOMPWITHTARGETNS_";



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



        /// <summary>
        /// This method performs a string comparison.
        /// </summary>
        /// <param name="xsltContext">The Xslt context</param>
        /// <param name="args">The arguments of the function</param>
        /// <param name="docContext">The document context</param>
        /// <returns>A boolean value indicating whether the argument strings are identical</returns>
        public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return (((string)args[1] + ResolveArgument(args[2]))).Equals(
                resolveNsPrefix(ResolveArgument(args[0]), (string)args[1], docContext), StringComparison.Ordinal);
        }



        /// <summary>
        /// This method resolves the prefix of an argument.
        /// If a prefix is found, the corresponding namespace URI (if existing) is looked up 
        /// and substituted. Otherwise the target namespace is placed first.
        /// </summary>
        /// <param name="args">An argument of the function to be resolved</param>
        /// <param name="targetNs"></param>
        /// <param name="docContext"></param>
        private static string resolveNsPrefix(string? args, string targetNs, XPathNavigator docContext)
        {
            if (args?.Contains(":") ?? false)
            {
                var prefix = args.Substring(0, args.IndexOf(":", StringComparison.Ordinal));
                string ns;
                if (!string.IsNullOrEmpty((ns = docContext.LookupNamespace(prefix) ?? string.Empty)))
                    return args.Replace(prefix + ":", ns);
                return targetNs + args;
            }
            return targetNs + args;
        }

    }



    /// <summary>
    /// This class represents a variable in an XPath expression.
    /// </summary>
    public class ParamFunctionVar : IXsltContextVariable
    {

        private readonly object _param;



        /// <summary>
        /// Initializes a new instance of the <see cref="ParamFunctionVar"/> class.
        /// </summary>
        /// <param name="param">The parameter</param>
        public ParamFunctionVar(object param)
        {
            _param = param;
        }



        /// <summary>
        /// Method implementing IXsltContextVariable
        /// </summary>
        public object Evaluate(XsltContext xsltContext)
        {
            return _param;
        }

        /// <summary>
        /// Method implementing IXsltContextVariable
        /// </summary>
        public bool IsLocal => true;

        /// <summary>
        /// Method implementing IXsltContextVariable
        /// </summary>
        public bool IsParam => true;

        /// <summary>
        /// Method implementing IXsltContextVariable
        /// </summary>
        public XPathResultType VariableType => XPathResultType.Any;

    }


}

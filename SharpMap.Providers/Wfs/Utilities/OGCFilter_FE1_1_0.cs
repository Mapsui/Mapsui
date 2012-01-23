// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections.Generic;
using System.Text;

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// Filter interface
    /// </summary>
    public interface IFilter
    {
        string Encode();
    }

    /// <summary>
    /// This class hosts a collection of instances implementing <see cref="IFilter"/>.
    /// </summary>
    public class OGCFilterCollection : IFilter
    {
        #region Enumerations

        /// <summary>
        /// This enumeration consists of expressions denoting FE logical operators.
        /// </summary>
        public enum JunctorEnum
        {
            And,
            Or
        } ;

        #endregion

        #region Fields and Properties

        private List<IFilter> _Filters;

        private JunctorEnum _Junctor = JunctorEnum.And;

        /// <summary>
        /// Gets and sets a collection of instances implementing <see cref="IFilter"/>.
        /// </summary>
        public List<IFilter> Filters
        {
            get { return _Filters; }
            set { _Filters = value; }
        }

        /// <summary>
        /// Gets and sets the operator for combining the filters.
        /// </summary>
        public JunctorEnum Junctor
        {
            get { return _Junctor; }
            set { _Junctor = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OGCFilterCollection"/> class.
        /// </summary>
        public OGCFilterCollection()
        {
            _Filters = new List<IFilter>();
        }

        #endregion

        #region Public Member

        /// <summary>
        /// This method adds an instance implementing <see cref="IFilter"/>.
        /// </summary>
        /// <param name="filter"></param>
        public void AddFilter(IFilter filter)
        {
            _Filters.Add(filter);
        }

        /// <summary>
        /// This method adds an instance of <see cref="OGCFilterCollection"/>.
        /// </summary>
        /// <param name="filterCollection"></param>
        public void AddFilterCollection(OGCFilterCollection filterCollection)
        {
            if (!ReferenceEquals(filterCollection, this))
                _Filters.Add(filterCollection);
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            StringBuilder filterBuilder = new StringBuilder();
            filterBuilder.Append("<" + _Junctor + ">");
            foreach (IFilter filter in Filters)
                filterBuilder.Append(filter.Encode());
            filterBuilder.Append("</" + _Junctor + ">");
            return filterBuilder.ToString();
        }

        #endregion
    }

    /// <summary>
    /// This class is the base class of all filters.
    /// It stores the filter arguments.
    /// </summary>
    public abstract class OGCFilterBase
    {
        #region Fields

        protected string[] _Args;

        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="args">An array of arguments for the filter</param>
        protected OGCFilterBase(string[] args)
        {
            _Args = args;
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsEqualToFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsEqualToFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsEqualToFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsEqualTo>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                <Literal>" + _Args[1] +
                   @"</Literal>
            </PropertyIsEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsNotEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsNotEqualToFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsNotEqualToFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsNotEqualToFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsNotEqualTo>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                <Literal>" + _Args[1] +
                   @"</Literal>
            </PropertyIsNotEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsLessThanFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsLessThanFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsLessThanFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsLessThanFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsLessThan>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                <Literal>" + _Args[1] +
                   @"</Literal>
            </PropertyIsLessThan>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsGreaterThanFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsGreaterThanFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsGreaterThanFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsGreaterThanFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsGreaterThan>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                <Literal>" + _Args[1] +
                   @"</Literal>
            </PropertyIsGreaterThan>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsLessThanOrEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsLessThanOrEqualToFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsLessThanOrEqualToFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsLessThanOrEqualToFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsLessThanOrEqualTo>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                <Literal>" + _Args[1] +
                   @"</Literal>
            </PropertyIsLessThanOrEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsGreaterThanOrEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsGreaterThanOrEqualToFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsGreaterThanOrEqualToFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsGreaterThanOrEqualToFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsGreaterThanOrEqualTo>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                <Literal>" + _Args[1] +
                   @"</Literal>
            </PropertyIsGreaterThanOrEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsBetweenFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsBetweenFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsBetweenFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsBetweenFilter_FE1_1_0(string propertyName, string lowerBoundary, string upperBoundary)
            : base(new[] {propertyName, lowerBoundary, upperBoundary})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return @"
            <PropertyIsBetween>
                <PropertyName>" + _Args[0] +
                   @"</PropertyName>
                    <LowerBoundary><Literal>" + _Args[1] +
                   @"</Literal></LowerBoundary>
                    <UpperBoundary><Literal>" + _Args[2] +
                   @"</Literal></UpperBoundary>
            </PropertyIsBetween>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsLikeFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsLikeFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyIsLikeFilter_FE1_1_0"/> class.
        /// </summary>
        public PropertyIsLikeFilter_FE1_1_0(string propertyName, string arg)
            : base(new[] {propertyName, arg})
        {
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            return
                @"
            <PropertyIsLike wildCard='*' singleChar='#' escapeChar='!'>
                <PropertyName>" +
                _Args[0] + @"</PropertyName>
                <Literal>" + _Args[1] +
                @"</Literal>
            </PropertyIsLike>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC FeatureIdFilter Version 1.1.0.
    /// </summary>
    public class FeatureIdFilter_FE1_1_0 : OGCFilterBase, IFilter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureIdFilter_FE1_1_0 "/> class.
        /// </summary>
        public FeatureIdFilter_FE1_1_0(string id)
            : base(new[] {id})
        {
        }

        #endregion

        #region IFilter Member

        public string Encode()
        {
            return "<FeatureId fid=" + _Args[0] + "/>";
        }

        #endregion

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
    }
}
// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System.Collections.Generic;
using System.Text;

namespace Mapsui.Providers.Wfs.Utilities
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

        private List<IFilter> _filters;

        private JunctorEnum _junctor = JunctorEnum.And;

        /// <summary>
        /// Gets and sets a collection of instances implementing <see cref="IFilter"/>.
        /// </summary>
        public List<IFilter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        /// <summary>
        /// Gets and sets the operator for combining the filters.
        /// </summary>
        public JunctorEnum Junctor
        {
            get { return _junctor; }
            set { _junctor = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OGCFilterCollection"/> class.
        /// </summary>
        public OGCFilterCollection()
        {
            _filters = new List<IFilter>();
        }

        #endregion

        #region Public Member

        /// <summary>
        /// This method adds an instance implementing <see cref="IFilter"/>.
        /// </summary>
        /// <param name="filter"></param>
        public void AddFilter(IFilter filter)
        {
            _filters.Add(filter);
        }

        /// <summary>
        /// This method adds an instance of <see cref="OGCFilterCollection"/>.
        /// </summary>
        /// <param name="filterCollection"></param>
        public void AddFilterCollection(OGCFilterCollection filterCollection)
        {
            if (!ReferenceEquals(filterCollection, this))
                _filters.Add(filterCollection);
        }

        #endregion

        #region IFilter Member

        /// <summary>
        /// This method encodes the filter in XML.
        /// </summary>
        /// <returns>An XML string</returns>
        public string Encode()
        {
            var filterBuilder = new StringBuilder();
            filterBuilder.Append("<" + _junctor + ">");
            foreach (IFilter filter in Filters)
                filterBuilder.Append(filter.Encode());
            filterBuilder.Append("</" + _junctor + ">");
            return filterBuilder.ToString();
        }

        #endregion
    }

    /// <summary>
    /// This class is the base class of all filters.
    /// It stores the filter arguments.
    /// </summary>
    public abstract class OgcFilterBase
    {
        #region Fields

        protected string[] Args;

        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor for the abstract class.
        /// </summary>
        /// <param name="args">An array of arguments for the filter</param>
        protected OgcFilterBase(string[] args)
        {
            Args = args;
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsEqualToFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                <Literal>" + Args[1] +
                   @"</Literal>
            </PropertyIsEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsNotEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsNotEqualToFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                <Literal>" + Args[1] +
                   @"</Literal>
            </PropertyIsNotEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsLessThanFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsLessThanFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                <Literal>" + Args[1] +
                   @"</Literal>
            </PropertyIsLessThan>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsGreaterThanFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsGreaterThanFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                <Literal>" + Args[1] +
                   @"</Literal>
            </PropertyIsGreaterThan>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsLessThanOrEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsLessThanOrEqualToFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                <Literal>" + Args[1] +
                   @"</Literal>
            </PropertyIsLessThanOrEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsGreaterThanOrEqualToFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsGreaterThanOrEqualToFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                <Literal>" + Args[1] +
                   @"</Literal>
            </PropertyIsGreaterThanOrEqualTo>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsBetweenFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsBetweenFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                <PropertyName>" + Args[0] +
                   @"</PropertyName>
                    <LowerBoundary><Literal>" + Args[1] +
                   @"</Literal></LowerBoundary>
                    <UpperBoundary><Literal>" + Args[2] +
                   @"</Literal></UpperBoundary>
            </PropertyIsBetween>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC PropertyIsLikeFilter Version 1.1.0.
    /// </summary>
    public class PropertyIsLikeFilter_FE1_1_0 : OgcFilterBase, IFilter
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
                Args[0] + @"</PropertyName>
                <Literal>" + Args[1] +
                @"</Literal>
            </PropertyIsLike>";
        }

        #endregion
    }

    /// <summary>
    /// This class provides an interface for creating an OGC FeatureIdFilter Version 1.1.0.
    /// </summary>
    public class FeatureIdFilter_FE1_1_0 : OgcFilterBase, IFilter
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
            return "<FeatureId fid=" + Args[0] + "/>";
        }

        #endregion
    }
}
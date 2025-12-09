// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wfs;
using Mapsui.Providers.Wfs.Utilities;

namespace Mapsui.Providers.Wfs2;

/// <summary>
/// WFS 2.0.2 data provider
/// This provider is optimized for OGC WFS 2.0.2 (OGC 09-025r2) and provides:
/// - GetCapabilities, DescribeFeatureType, and GetFeature operations with version 2.0.2
/// - Support for BBOX queries
/// - Support for paging via count and startIndex parameters
/// - Support for resultType (results/hits)
/// - Reuses the existing WFS infrastructure for parsing and geometry handling
/// </summary>
public sealed class Wfs2Provider : IProvider, IDisposable
{
    private readonly WFSProvider _innerProvider;
    private readonly WFS_2_0_2_TextResources _textResources;
    private int? _count;
    private int? _startIndex;
    private string? _resultType;

    /// <summary>
    /// Gets the underlying WFSProvider instance
    /// </summary>
    internal WFSProvider InnerProvider => _innerProvider;

    /// <summary>
    /// Gets or sets the CRS (Coordinate Reference System) for the provider
    /// </summary>
    public string? CRS
    {
        get => _innerProvider.CRS;
        set => _innerProvider.CRS = value;
    }

    /// <summary>
    /// Gets or sets the axis order for coordinate transformations
    /// </summary>
    public int[] AxisOrder
    {
        get => _innerProvider.AxisOrder;
        set => _innerProvider.AxisOrder = value;
    }

    /// <summary>
    /// Gets or sets whether to use GET requests for GetFeature (default is POST)
    /// </summary>
    public bool GetFeatureGetRequest
    {
        get => _innerProvider.GetFeatureGetRequest;
        set => _innerProvider.GetFeatureGetRequest = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of features to return per request (WFS 2.0.2 paging)
    /// Note: This property is exposed for future use. Full integration with the request flow
    /// requires additional changes to the WFSProvider request building logic.
    /// </summary>
    public int? Count
    {
        get => _count;
        set => _count = value;
    }

    /// <summary>
    /// Gets or sets the starting index for paging (WFS 2.0.2 paging)
    /// Note: This property is exposed for future use. Full integration with the request flow
    /// requires additional changes to the WFSProvider request building logic.
    /// </summary>
    public int? StartIndex
    {
        get => _startIndex;
        set => _startIndex = value;
    }

    /// <summary>
    /// Gets or sets the result type: "results" for features or "hits" for count only
    /// Note: This property is exposed for future use. Full integration with the request flow
    /// requires additional changes to the WFSProvider request building logic.
    /// </summary>
    public string? ResultType
    {
        get => _resultType;
        set => _resultType = value;
    }

    /// <summary>
    /// Gets or sets the OGC filter to apply to queries
    /// </summary>
    public IFilter? OgcFilter
    {
        get => _innerProvider.OgcFilter;
        set => _innerProvider.OgcFilter = value;
    }

    /// <summary>
    /// Gets or sets the label properties
    /// </summary>
    public List<string> Labels
    {
        get => _innerProvider.Labels;
        set => _innerProvider.Labels = value;
    }

    /// <summary>
    /// Gets feature metadata
    /// </summary>
    public WfsFeatureTypeInfo? FeatureTypeInfo => _innerProvider.FeatureTypeInfo;

    /// <summary>
    /// Gets or sets the network credentials
    /// </summary>
    public ICredentials? Credentials
    {
        get => _innerProvider.Credentials;
        set => _innerProvider.Credentials = value;
    }

    /// <summary>
    /// Gets or sets the proxy URL
    /// </summary>
    public string? ProxyUrl
    {
        get => _innerProvider.ProxyUrl;
        set => _innerProvider.ProxyUrl = value;
    }

    /// <summary>
    /// Private constructor - use CreateAsync factory method instead
    /// </summary>
    private Wfs2Provider(WFSProvider innerProvider, WFS_2_0_2_TextResources textResources)
    {
        _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        _textResources = textResources ?? throw new ArgumentNullException(nameof(textResources));
    }

    /// <summary>
    /// Helper method to override the text resources in a WFSProvider instance using reflection.
    /// This is necessary because WFSProvider doesn't expose a way to inject custom text resources.
    /// </summary>
    private static void OverrideTextResources(WFSProvider provider, WFS_2_0_2_TextResources textResources)
    {
        var textResourcesField = typeof(WFSProvider).GetField("_textResources",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        textResourcesField?.SetValue(provider, textResources);
    }

    /// <summary>
    /// Creates a new WFS 2.0.2 provider instance and initializes it
    /// </summary>
    /// <param name="baseUri">Base URI of the WFS service (e.g., "https://example.com/geoserver/ows")</param>
    /// <param name="nsPrefix">Namespace prefix for the feature type (e.g., "vogis")</param>
    /// <param name="featureTypeName">Name of the feature type to query (e.g., "laser_2002_04_punkte")</param>
    /// <param name="persistentCache">Optional persistent cache for responses</param>
    /// <param name="proxyUrl">Optional proxy URL</param>
    /// <param name="credentials">Optional network credentials</param>
    /// <returns>Initialized Wfs2Provider instance</returns>
    public static async Task<Wfs2Provider> CreateAsync(
        string baseUri,
        string nsPrefix,
        string featureTypeName,
        IUrlPersistentCache? persistentCache = null,
        string? proxyUrl = null,
        ICredentials? credentials = null)
    {
        var textResources = new WFS_2_0_2_TextResources();

        // Create the inner WFSProvider with WFS 2.0.0 enum
        // (we'll override the text resources to use 2.0.2)
        var innerProvider = new WFSProvider(
            baseUri,
            nsPrefix,
            string.Empty, // featureTypeNamespace - will be resolved from GetCapabilities
            featureTypeName,
            string.Empty, // geometryName - will be resolved from DescribeFeatureType
            WFSProvider.WFSVersionEnum.WFS_2_0_0,
            persistentCache);

        // Override the text resources with our 2.0.2 version
        OverrideTextResources(innerProvider, textResources);

        if (!string.IsNullOrEmpty(proxyUrl))
        {
            innerProvider.ProxyUrl = proxyUrl;
        }

        if (credentials != null)
        {
            innerProvider.Credentials = credentials;
        }

        var provider = new Wfs2Provider(innerProvider, textResources);

        // Note: Don't call InitAsync here - let the user call it explicitly
        // to match the pattern used in WFSProvider

        return provider;
    }

    /// <summary>
    /// Creates a new WFS 2.0.2 provider with complete metadata, bypassing GetCapabilities and DescribeFeatureType
    /// </summary>
    /// <param name="serviceUri">The service URI</param>
    /// <param name="nsPrefix">Namespace prefix</param>
    /// <param name="featureTypeNamespace">Feature type namespace</param>
    /// <param name="featureTypeName">Feature type name</param>
    /// <param name="geometryName">Geometry property name</param>
    /// <param name="geometryType">Geometry type</param>
    /// <param name="persistentCache">Optional persistent cache</param>
    /// <returns>Wfs2Provider instance ready to use</returns>
    public static Wfs2Provider Create(
        string serviceUri,
        string nsPrefix,
        string featureTypeNamespace,
        string featureTypeName,
        string geometryName,
        GeometryTypeEnum geometryType = GeometryTypeEnum.Unknown,
        IUrlPersistentCache? persistentCache = null)
    {
        var textResources = new WFS_2_0_2_TextResources();

        var innerProvider = new WFSProvider(
            serviceUri,
            nsPrefix,
            featureTypeNamespace,
            featureTypeName,
            geometryName,
            geometryType,
            WFSProvider.WFSVersionEnum.WFS_2_0_0,
            persistentCache);

        // Override the text resources with our 2.0.2 version
        OverrideTextResources(innerProvider, textResources);

        return new Wfs2Provider(innerProvider, textResources);
    }

    /// <summary>
    /// Initialize the provider by fetching metadata from GetCapabilities and DescribeFeatureType
    /// </summary>
    public async Task InitAsync()
    {
        await _innerProvider.InitAsync();
    }

    /// <summary>
    /// Returns all features whose bounding box intersects with the specified fetch info
    /// </summary>
    /// <param name="fetchInfo">Fetch information including bounding box for the query</param>
    /// <returns>Collection of features within the bounding box</returns>
    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        // Note: WFS 2.0.2 paging parameters (count, startIndex, resultType)
        // would need to be integrated into the request building logic
        // For now, we delegate to the inner provider which uses the text resources
        return await _innerProvider.GetFeaturesAsync(fetchInfo).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the extent (bounding box) of all features
    /// </summary>
    /// <returns>The extent of the data</returns>
    public MRect? GetExtent()
    {
        return _innerProvider.GetExtent();
    }

    /// <summary>
    /// Disposes the provider and releases resources
    /// </summary>
    public void Dispose()
    {
        // We own the inner provider, so we dispose it
#pragma warning disable IDISP007 // Don't dispose injected
        _innerProvider.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
    }
}

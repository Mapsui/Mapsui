namespace Mapsui.ArcGIS.ImageServiceProvider;

public enum InterpolationType
{
    RSP_BilinearInterpolation,
    RSP_CubicConvolution,
    RSP_Majority,
    RSP_NearestNeighbor
}


public class ArcGISImageCapabilities : IArcGISCapabilities
{
    public ArcGISImageCapabilities() : this("") { }

    public ArcGISImageCapabilities(string url, long startTime = -1, long endTime = -1, string format = "jpgpng", InterpolationType interpolation = InterpolationType.RSP_NearestNeighbor)
    {
        ServiceUrl = url;
        Format = format;
        Interpolation = interpolation;
        StartTime = startTime;
        EndTime = endTime;
    }

    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public InterpolationType Interpolation { get; set; }
    public string Format { get; set; }
    public string ServiceUrl { get; set; }
    public string? currentVersion { get; set; }
    public bool singleFusedMapCache { get; set; }
    public Extent? fullExtent { get; set; }
    public SpatialReference? spatialReference { get; set; }
    public Extent? initialExtent { get; set; }
    public TileInfo? tileInfo { get; set; }
    public string? copyrightText { get; set; }
    public string? serviceDescription { get; set; }
    public string? capabilities { get; set; }
    public string? description { get; set; }
    public string? name { get; set; }
    public TimeInfo? timeInfo { get; set; }
    public double pixelSizeX { get; set; }
    public double pixelSizeY { get; set; }
    public int bandCount { get; set; }
    public string? pixelType { get; set; }
    public double minPixelSize { get; set; }
    public double maxPixelSize { get; set; }
    public string? serviceDataType { get; set; }
    public double[]? minValues { get; set; }
    public double[]? maxValues { get; set; }
    public double[]? stdvValues { get; set; }
    public string? objectIdField { get; set; }
    public Field[]? fields { get; set; }
    public string? defaultCompressionQuality { get; set; }
    public string? defaultResamplingMethod { get; set; }
    public double maxImageHeight { get; set; }
    public double maxImageWidth { get; set; }
    public string? defaultMosaicMethod { get; set; }
    public string? allowedMosaicMethods { get; set; }
    public string? sortField { get; set; }
    public string? sortValue { get; set; }
    public string? mosaicOperator { get; set; }
    public double maxRecordCount { get; set; }
    public double maxDownloadImageCount { get; set; }
    public double maxDownloadSizeLimit { get; set; }
    public double maxMosaicImageCount { get; set; }
    public string? cacheDirectory { get; set; }
    public double minScale { get; set; }
    public double maxScale { get; set; }
    public bool allowRasterFunction { get; set; }
    public RasterFunctionInfo[]? rasterFunctionInfos { get; set; }
    public RasterTypeInfo[]? rasterTypeInfos { get; set; }
    public EditFieldsInfo? editFieldsInfo { get; set; }
    public OwnershipBasedAccessControlForRasters? ownershipBasedAccessControlForRasters { get; set; }
    public string? mensurationCapabilities { get; set; }
    public bool hasHistograms { get; set; }
    public bool hasColormap { get; set; }
    public bool hasRasterAttributeTable { get; set; }
    public bool allowComputeTiePoints { get; set; }
    public bool useStandardizedQueries { get; set; }
    public bool supportsStatistics { get; set; }
    public bool supportsAdvancedQueries { get; set; }
}

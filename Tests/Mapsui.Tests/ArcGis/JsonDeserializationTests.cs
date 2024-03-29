using System.Text.Json;
using Mapsui.ArcGIS;
using NUnit.Framework;

namespace Mapsui.Tests.ArcGis;

[TestFixture]
public class JsonDeserializationTests
{
    [Test]
    public void DeserializeArcGisCapabilitiesTest()
    {
        var json = """
                   {
                       "advancedQueryCapabilities": {
                           "supportsDistinct": true,
                           "supportsOrderBy": true,
                           "supportsPagination": true,
                           "supportsStatistics": true,
                           "useStandardizedQueries": true
                       },
                       "allowAnalysis": true,
                       "allowComputeTiePoints": false,
                       "allowCopy": true,
                       "allowedMosaicMethods": "ByAttribute,Seamline,NorthWest,Center,LockRaster,Nadir,None",
                       "allowRasterFunction": true,
                       "bandCount": 6,
                       "bandNames": [
                           "Band_1",
                           "Band_2",
                           "Band_3",
                           "Band_4",
                           "Band_5",
                           "Band_6"
                       ],
                       "blockHeight": 256,
                       "blockWidth": 2048,
                       "capabilities": "Catalog,Mensuration,Image,Metadata",
                       "compressionType": "None",
                       "copyrightText": "",
                       "currentVersion": 10.9,
                       "datasetFormat": "AMD",
                       "defaultCompressionQuality": 85,
                       "defaultMosaicMethod": "ByAttribute",
                       "defaultResamplingMethod": "Bilinear",
                       "description": "Multispectral Landsat GLS image service created from the Global Land Survey (GLS) data from epochs 1975, 1990, 2000, 2005 and 2010. GLS datasets are created by the United States Geological Survey (USGS) and the National Aeronautics and Space Administration (NASA), using Landsat images. This service includes imagery from Landsat 4, Landsat 5 TM and Landsat 7 ETM, at 30 meter resolution and Landsat MSS at 60 meter resolution. It can be used for mapping and change detection of agriculture, soils, vegetation health, water-land features and boundary studies. Using on-the-fly processing, the raw DN values are transformed to scaled (0 - 10000) apparent reflectance values and then different service based renderings for band combinations and indices are applied.",
                       "editFieldsInfo": null,
                       "exportTilesAllowed": false,
                       "extent": {
                           "spatialReference": {
                               "latestWkid": 3857,
                               "wkid": 102100
                           },
                           "xmax": 20037507.8427882,
                           "xmin": -20037507.0672,
                           "ymax": 16876499.3966,
                           "ymin": -8572530.6034
                       },
                       "fields": [
                           {
                               "alias": "OBJECTID",
                               "domain": null,
                               "name": "OBJECTID",
                               "type": "esriFieldTypeOID"
                           },
                           {
                               "alias": "Name",
                               "domain": null,
                               "length": 200,
                               "name": "Name",
                               "type": "esriFieldTypeString"
                           },
                           {
                               "alias": "MinPS",
                               "domain": null,
                               "name": "MinPS",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "MaxPS",
                               "domain": null,
                               "name": "MaxPS",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "LowPS",
                               "domain": null,
                               "name": "LowPS",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "HighPS",
                               "domain": null,
                               "name": "HighPS",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "Category",
                               "domain": {
                                   "codedValues": [
                                       {
                                           "code": 0,
                                           "name": "Unknown"
                                       },
                                       {
                                           "code": 1,
                                           "name": "Primary"
                                       },
                                       {
                                           "code": 2,
                                           "name": "Overview"
                                       },
                                       {
                                           "code": 3,
                                           "name": "Unprocessed Overview"
                                       },
                                       {
                                           "code": 4,
                                           "name": "Partial Overview"
                                       },
                                       {
                                           "code": 5,
                                           "name": "Inactive"
                                       },
                                       {
                                           "code": 253,
                                           "name": "Uploaded"
                                       },
                                       {
                                           "code": 254,
                                           "name": "Incomplete"
                                       },
                                       {
                                           "code": 255,
                                           "name": "Custom"
                                       }
                                   ],
                                   "description": "Catalog item categories.",
                                   "mergePolicy": "esriMPTDefaultValue",
                                   "name": "MosaicCatalogItemCategoryDomain",
                                   "splitPolicy": "esriSPTDefaultValue",
                                   "type": "codedValue"
                               },
                               "name": "Category",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "GroupName",
                               "domain": null,
                               "length": 100,
                               "name": "GroupName",
                               "type": "esriFieldTypeString"
                           },
                           {
                               "alias": "ProductName",
                               "domain": null,
                               "length": 100,
                               "name": "ProductName",
                               "type": "esriFieldTypeString"
                           },
                           {
                               "alias": "CenterX",
                               "domain": null,
                               "name": "CenterX",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "CenterY",
                               "domain": null,
                               "name": "CenterY",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "WRS_Path",
                               "domain": null,
                               "name": "WRS_Path",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "WRS_Row",
                               "domain": null,
                               "name": "WRS_Row",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "Best",
                               "domain": null,
                               "name": "Best",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "AcquisitionDate",
                               "domain": null,
                               "length": 8,
                               "name": "AcquisitionDate",
                               "type": "esriFieldTypeDate"
                           },
                           {
                               "alias": "DateUpdated",
                               "domain": null,
                               "length": 8,
                               "name": "DateUpdated",
                               "type": "esriFieldTypeDate"
                           },
                           {
                               "alias": "SunAzimuth",
                               "domain": null,
                               "name": "SunAzimuth",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "SunElevation",
                               "domain": null,
                               "name": "SunElevation",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "CloudCover",
                               "domain": null,
                               "name": "CloudCover",
                               "type": "esriFieldTypeDouble"
                           },
                           {
                               "alias": "PR",
                               "domain": null,
                               "name": "PR",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "Latest",
                               "domain": null,
                               "name": "Latest",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "DayOfYear",
                               "domain": null,
                               "name": "DayOfYear",
                               "type": "esriFieldTypeInteger"
                           },
                           {
                               "alias": "Month",
                               "domain": null,
                               "name": "Month",
                               "type": "esriFieldTypeSmallInteger"
                           },
                           {
                               "alias": "SensorName",
                               "domain": null,
                               "length": 30,
                               "name": "SensorName",
                               "type": "esriFieldTypeString"
                           },
                           {
                               "alias": "dataset_id",
                               "domain": null,
                               "length": 50,
                               "name": "dataset_id",
                               "type": "esriFieldTypeString"
                           },
                           {
                               "alias": "Shape",
                               "domain": null,
                               "name": "Shape",
                               "type": "esriFieldTypeGeometry"
                           }
                       ],
                       "fullExtent": {
                           "spatialReference": {
                               "latestWkid": 3857,
                               "wkid": 102100
                           },
                           "xmax": 20037507.8427882,
                           "xmin": -20037507.0672,
                           "ymax": 16876499.3966,
                           "ymin": -8572530.6034
                       },
                       "hasColormap": false,
                       "hasHistograms": false,
                       "hasMultidimensions": false,
                       "hasRasterAttributeTable": false,
                       "initialExtent": {
                           "spatialReference": {
                               "latestWkid": 3857,
                               "wkid": 102100
                           },
                           "xmax": 20037507.8427882,
                           "xmin": -20037507.0672,
                           "ymax": 16876499.3966,
                           "ymin": -8572530.6034
                       },
                       "maxDownloadImageCount": 20,
                       "maxDownloadSizeLimit": 2048,
                       "maxImageHeight": 4000,
                       "maxImageWidth": 4000,
                       "maxMosaicImageCount": 20,
                       "maxPixelSize": 0,
                       "maxRecordCount": 1000,
                       "maxScale": 0,
                       "maxValues": [
                           10000,
                           10000,
                           10000,
                           10000,
                           10000,
                           10000
                       ],
                       "meanValues": [
                           3500,
                           3500,
                           3500,
                           3500,
                           3500,
                           3500
                       ],
                       "mensurationCapabilities": "Basic",
                       "minPixelSize": 0,
                       "minScale": 0,
                       "minValues": [
                           0,
                           0,
                           0,
                           0,
                           0,
                           0
                       ],
                       "mosaicOperator": "First",
                       "name": "LandsatGLS/MS",
                       "objectIdField": "OBJECTID",
                       "ownershipBasedAccessControlForRasters": null,
                       "pixelSizeX": 29.9999961896375,
                       "pixelSizeY": 30,
                       "pixelType": "S16",
                       "rasterFunctionInfos": [
                           {
                               "description": "Bands shortwave IR-1, near-IR, blue (5, 4, 1 ) with dynamic range adjustment applied on apparent reflectance. Vigorous veg. is bright green, stressed veg. dull green and bare areas as brown.",
                               "help": "",
                               "name": "Agriculture with DRA"
                           },
                           {
                               "description": "Bands near-IR, red, green (4 , 3, 2) with dynamic range adjustment applied on apparent reflectance. Healthy vegetation is bright red while stressed vegetation is dull red.",
                               "help": "",
                               "name": "Color Infrared with DRA"
                           },
                           {
                               "description": "Bands shortwave IR-2, red, blue (6, 3, 1) with dynamic range adjustment applied on apparent reflectance. Vigorous veg. is bright green, stressed veg. dull green and bare areas as brown.",
                               "help": "",
                               "name": "Geology with DRA"
                           },
                           {
                               "description": "Natural Color bands red, green, blue ( 3, 2, 1) displayed with dynamic range adjustment applied on apparent reflectance.",
                               "help": "",
                               "name": "Natural Color with DRA"
                           },
                           {
                               "description": "Bands shortwave-IR2, near-IR, red (6, 4, 3) with dynamic range adjustment applied on apparent reflectance.",
                               "help": "",
                               "name": "Short-wave Infrared with DRA"
                           },
                           {
                               "description": "Bands shortwave IR-1, near-IR, blue (5, 4, 1) with fixed stretched applied on apparent reflectance. Vigorous vegetation is bright green, stressed vegetation dull green and bare areas as brown.",
                               "help": "",
                               "name": "Agriculture"
                           },
                           {
                               "description": "Bands near-infrared, red, green (4 , 3, 2) with fixed stretch applied on apparent reflectance. Healthy vegetation is bright red while stressed vegetation is dull red.",
                               "help": "",
                               "name": "Color Infrared"
                           },
                           {
                               "description": "Bands shortwave IR-2, near-IR, blue (6, 3, 1) with fixed stretched applied on apparent reflectance. Vigorous vegetation is bright green, stressed vegetation dull green and bare areas as brown.",
                               "help": "",
                               "name": "Geology"
                           },
                           {
                               "description": "Natural Color bands red, green, blue(3, 2, 1) displayed with fixed stretch applied on apparent reflectance.",
                               "help": "",
                               "name": "Natural Color"
                           },
                           {
                               "description": "Bands shortwave-IR2, near-IR, red (6, 4, 3) with fixed stretched applied on apparent reflectance.",
                               "help": "",
                               "name": "Short-wave Infrared"
                           },
                           {
                               "description": "Normalized difference vegetation index (NDVI) with color map. Dark green is thick vigorous vegetation and brown represents sparse vegetation.",
                               "help": "",
                               "name": "NDVI Colorized"
                           },
                           {
                               "description": "Normalized Difference Moisture Index with color map. Wetlands and moist areas are blues, and dry areas in deep yellow and brown.",
                               "help": "",
                               "name": "Normalized Difference Moisture Index Colorized"
                           },
                           {
                               "description": "Normalized difference vegetation index (NDVI) computed as (b4 - b3) / (b4 + b3) on apparent reflectance",
                               "help": "",
                               "name": "NDVI Raw"
                           },
                           {
                               "description": "Normalized Burn Ratio (BNBR) computed as (b4 - b6) / (b4 + b6) on apparent reflectance",
                               "help": "",
                               "name": "NBR Raw"
                           },
                           {
                               "description": "A No-Op Function.",
                               "help": "",
                               "name": "None"
                           }
                       ],
                       "rasterTypeInfos": [
                           {
                               "description": "Supports all ArcGIS Raster Datasets",
                               "help": "",
                               "name": "Raster Dataset"
                           }
                       ],
                       "serviceDataType": "esriImageServiceDataTypeGeneric",
                       "serviceDescription": "Multispectral Landsat GLS image service created from the Global Land Survey (GLS) data from epochs 1975, 1990, 2000, 2005 and 2010. GLS datasets are created by the United States Geological Survey (USGS) and the National Aeronautics and Space Administration (NASA), using Landsat images. This service includes imagery from Landsat 4, Landsat 5 TM and Landsat 7 ETM, at 30 meter resolution and Landsat MSS at 60 meter resolution. It can be used for mapping and change detection of agriculture, soils, vegetation health, water-land features and boundary studies. Using on-the-fly processing, the raw DN values are transformed to scaled (0 - 10000) apparent reflectance values and then different service based renderings for band combinations and indices are applied.",
                       "serviceSourceType": "esriImageServiceSourceTypeMosaicDataset",
                       "sortAscending": true,
                       "sortField": "BEST",
                       "sortValue": "0",
                       "spatialReference": {
                           "latestWkid": 3857,
                           "wkid": 102100
                       },
                       "stdvValues": [
                           1250,
                           1250,
                           1250,
                           1250,
                           1250,
                           1250
                       ],
                       "supportsAdvancedQueries": true,
                       "supportsStatistics": true,
                       "timeInfo": {
                           "endTimeField": "AcquisitionDate",
                           "startTimeField": "AcquisitionDate",
                           "timeExtent": [
                               80870400000,
                               1327881600000
                           ],
                           "timeReference": null
                       },
                       "uncompressedSize": 13598271816408,
                       "useStandardizedQueries": true
                   }
                   """;
        Assert.DoesNotThrow(() =>
        {
            var result = JsonSerializer.Deserialize(json, ArcGISContext.Default.ArcGISImageCapabilities);
        });
    }

    [Test]
    public void DeserializeArcGisDynamicCapabilitiesTest()
    {
        var json = """
                   {
                       "archivingInfo": {
                           "supportsHistoricMoment": false
                       },
                       "capabilities": "Map,Query,Data",
                       "cimVersion": "2.9.0",
                       "copyrightText": "",
                       "currentVersion": 10.91,
                       "datesInUnknownTimezone": false,
                       "datumTransformations": [
                           {
                               "geoTransforms": [
                                   {
                                       "latestWkid": 1241,
                                       "name": "NAD_1927_To_NAD_1983_NADCON",
                                       "transformForward": true,
                                       "wkid": 108001
                                   }
                               ]
                           },
                           {
                               "geoTransforms": [
                                   {
                                       "latestWkid": 1241,
                                       "name": "NAD_1927_To_NAD_1983_NADCON",
                                       "transformForward": false,
                                       "wkid": 108001
                                   }
                               ]
                           }
                       ],
                       "description": "The SampleWorldCities map service displays world cities symbolized based on population.",
                       "documentInfo": {
                           "AntialiasingMode": "Fast",
                           "Author": "",
                           "Category": "",
                           "Comments": "The SampleWorldCities map service displays world cities symbolized based on population.",
                           "Keywords": "sample,map,service",
                           "Subject": "The SampleWorldCities map service displays world cities symbolized based on population.",
                           "TextAntialiasingMode": "Force",
                           "Title": "World Cities",
                           "Version": "2.5.0"
                       },
                       "exportTilesAllowed": false,
                       "fullExtent": {
                           "spatialReference": {
                               "falseM": -100000,
                               "falseX": -400,
                               "falseY": -400,
                               "falseZ": -100000,
                               "latestWkid": 4326,
                               "mTolerance": 0.001,
                               "mUnits": 10000,
                               "wkid": 4326,
                               "xyTolerance": 0.0002,
                               "xyUnits": 11258999068426.2,
                               "zTolerance": 0.001,
                               "zUnits": 10000
                           },
                           "xmax": 180.000122070313,
                           "xmin": -180,
                           "ymax": 90.0001220703125,
                           "ymin": -90
                       },
                       "initialExtent": {
                           "spatialReference": {
                               "falseM": -100000,
                               "falseX": -400,
                               "falseY": -400,
                               "falseZ": -100000,
                               "latestWkid": 4326,
                               "mTolerance": 0.001,
                               "mUnits": 10000,
                               "wkid": 4326,
                               "xyTolerance": 0.0002,
                               "xyUnits": 11258999068426.2,
                               "zTolerance": 0.001,
                               "zUnits": 10000
                           },
                           "xmax": 56.8340011675679,
                           "xmin": -86.0071633242112,
                           "ymax": 66.7093364509988,
                           "ymin": -40.2731150927194
                       },
                       "layers": [
                           {
                               "defaultVisibility": true,
                               "geometryType": "esriGeometryPoint",
                               "id": 0,
                               "maxScale": 0,
                               "minScale": 0,
                               "name": "Cities",
                               "parentLayerId": -1,
                               "subLayerIds": null,
                               "supportsDynamicLegends": true,
                               "type": "Feature Layer"
                           },
                           {
                               "defaultVisibility": true,
                               "geometryType": "esriGeometryPolygon",
                               "id": 1,
                               "maxScale": 0,
                               "minScale": 0,
                               "name": "Continent",
                               "parentLayerId": -1,
                               "subLayerIds": null,
                               "supportsDynamicLegends": true,
                               "type": "Feature Layer"
                           },
                           {
                               "defaultVisibility": true,
                               "geometryType": "esriGeometryPolygon",
                               "id": 2,
                               "maxScale": 0,
                               "minScale": 0,
                               "name": "World",
                               "parentLayerId": -1,
                               "subLayerIds": null,
                               "supportsDynamicLegends": true,
                               "type": "Feature Layer"
                           }
                       ],
                       "mapName": "World Cities Population",
                       "maxImageHeight": 4096,
                       "maxImageWidth": 4096,
                       "maxRecordCount": 1000,
                       "maxScale": 0,
                       "minScale": 0,
                       "referenceScale": 0.0,
                       "resampling": false,
                       "serviceDescription": "The SampleWorldCities service is provided so you can quickly and easily preview the functionality of the GIS server. Click the thumbnail image to open in a web application. This sample service is optional and can be deleted.",
                       "singleFusedMapCache": false,
                       "spatialReference": {
                           "falseM": -100000,
                           "falseX": -400,
                           "falseY": -400,
                           "falseZ": -100000,
                           "latestWkid": 4326,
                           "mTolerance": 0.001,
                           "mUnits": 10000,
                           "wkid": 4326,
                           "xyTolerance": 0.0002,
                           "xyUnits": 11258999068426.2,
                           "zTolerance": 0.001,
                           "zUnits": 10000
                       },
                       "supportedExtensions": "KmlServer, WFSServer, WMSServer",
                       "supportedImageFormatTypes": "PNG32,PNG24,PNG,JPG,DIB,TIFF,EMF,PS,PDF,GIF,SVG,SVGZ,BMP",
                       "supportedQueryFormats": "JSON, geoJSON, PBF",
                       "supportsClipping": true,
                       "supportsDatumTransformation": true,
                       "supportsDynamicLayers": true,
                       "supportsQueryDataElements": true,
                       "supportsSpatialFilter": true,
                       "supportsTimeRelation": true,
                       "tables": [
                       ],
                       "units": "esriDecimalDegrees"
                   }
                   """;
        Assert.DoesNotThrow(() =>
        {
            var result = JsonSerializer.Deserialize(json, ArcGISContext.Default.ArcGISDynamicCapabilities);
        });
    }
}

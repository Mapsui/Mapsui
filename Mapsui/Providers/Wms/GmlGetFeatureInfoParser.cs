using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Mapsui.Providers.Wms;

public class GmlGetFeatureInfoParser : IGetFeatureInfoParser
{
    private FeatureInfo? _featureInfo;

    public FeatureInfo ParseWMSResult(string? layerName, Stream result)
    {
        _featureInfo = new FeatureInfo { LayerName = layerName, FeatureInfos = new List<Dictionary<string, string>>() };
        XDocument xdoc;

        try
        {
            xdoc = XDocument.Load(result);
        }
        catch (XmlException e)
        {
            throw new ApplicationException("Bad formatted XML response", e);
        }

        ExtractFeatureInfo(xdoc.Root);

        return _featureInfo;
    }

    private void ExtractFeatureInfo(XElement? root)
    {
        LookExtractMultipleElements(root);

        if (_featureInfo?.FeatureInfos?.Count == 0)
            ExtractFeatures(root);
    }

    private void LookExtractMultipleElements(XElement? layer)
    {
        if (layer == null || !layer.HasElements) return;
        var element = layer.Descendants().FirstOrDefault();

        if (element != null)
        {
            if (layer.Elements(element.Name).Count() == 1)
                LookExtractMultipleElements(element);

            if (layer.Elements(element.Name).Count() > 1)
            {
                ExtractFeatures(layer);
            }
        }
    }

    private void ExtractFeatures(XContainer? layer)
    {
        if (layer == null)
            return;

        foreach (var feature in layer.Elements())
        {
            _featureInfo?.FeatureInfos?.Add(ExtractFeatureElements(feature));
        }
    }

    private Dictionary<string, string> ExtractFeatureElements(XElement layerElement)
    {
        try
        {
            var feature = new Dictionary<string, string>();
            var elements = layerElement.DescendantNodes().OfType<XElement>();

            foreach (var element in elements)
            {
                if (!element.HasElements && !element.Name.LocalName.Equals("coordinates"))
                    feature.Add(element.Name.LocalName, element.Value);
            }

            return feature;
        }
        catch (Exception)
        {
            throw new ApplicationException("Error creating features");
        }
    }
}

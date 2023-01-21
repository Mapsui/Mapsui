using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Mapsui.Providers.Wms;

public class XmlGetFeatureInfoParser : IGetFeatureInfoParser
{
    public FeatureInfo ParseWMSResult(string? layerName, Stream result)
    {
        var featureInfos = new List<Dictionary<string, string>>();
        XDocument xdoc;

        try
        {
            xdoc = XDocument.Load(result);
        }
        catch (XmlException e)
        {
            throw new ArgumentException(e.Message);
        }

        var fields = (from XElement element in xdoc.Descendants()
                      where (element.Name.LocalName.Equals("FIELDS"))
                      select element);

        foreach (var field in fields)
        {
            featureInfos.Add(ExtractFeatureFromField(field));
        }

        var info = new FeatureInfo { LayerName = layerName, FeatureInfos = featureInfos };

        return info;
    }

    private static Dictionary<string, string> ExtractFeatureFromField(XElement featureMember)
    {
        //No layer name is returned from XML
        var featureInfo = new Dictionary<string, string>();

        if (featureMember.HasElements)
        {
            var elements = from ele in featureMember.Descendants() select ele;

            foreach (var element in elements.Where(element => element.Name.LocalName.Equals("FIELD")))
            {
                var name = element.Attribute("name");
                var value = element.Attribute("value");

                if (name != null && value != null)
                    featureInfo.Add(name.Value, value.Value);
            }
        }

        if (featureMember.HasAttributes)
        {
            foreach (var attribute in featureMember.Attributes())
            {
                featureInfo.Add(attribute.Name.ToString(), attribute.Value);
            }
        }

        return featureInfo;
    }
}

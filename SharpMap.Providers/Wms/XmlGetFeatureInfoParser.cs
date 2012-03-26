using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SharpMap.Providers.Wms
{
    public class XmlGetFeatureInfoParser : IGetFeatureInfoParser
    {
        public List<FeatureInfo> ParseWMSResult(Stream result)
        {
            var featureInfos = new List<FeatureInfo>();
            var xdoc = XDocument.Load(new XmlTextReader(result));
            var fields = (from XElement element in xdoc.Descendants()
                          where (element.Name.LocalName.Equals("FIELDS"))
                          select element);

            foreach (var field in fields)
            {
                featureInfos.Add(ExtractFeatureFromField(field));
            }

            return featureInfos;
        }

        private static FeatureInfo ExtractFeatureFromField(XElement featureMember)
        {
            //No layername is resturned from XML
            var featureInfo = new FeatureInfo { LayerName = "" };

            if (featureMember.HasElements)
            {
                var elements = from ele in featureMember.Descendants() select ele;

                foreach (var element in elements.Where(element => element.Name.LocalName.Equals("FIELD")))
                {
                    var name = element.Attribute("name");
                    var value = element.Attribute("value");

                    if (name != null && value != null)
                        featureInfo.Attributes.Add(name.Value, value.Value);
                }
            }

            return featureInfo;
        }
    }
}

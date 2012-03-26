using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SharpMap.Providers.Wms
{
    public class GmlGetFeatureInfoParser : IGetFeatureInfoParser
    {
        public List<FeatureInfo> ParseWMSResult(Stream result)
        {
            var featureInfos = new List<FeatureInfo>();
            var xdoc = XDocument.Load(new XmlTextReader(result));
            var featureMembers = (from XElement element in xdoc.Descendants()
                                  where (element.Name.LocalName.Equals("featureMember"))
                                  select element);

            foreach (var featureMember in featureMembers)
            {
                featureInfos.Add(ExtractFeatureMemberFromElement(featureMember));
            }

            return featureInfos;
        }

        private static FeatureInfo ExtractFeatureMemberFromElement(XElement featureMember)
        {
            var featureInfo = new FeatureInfo();

            if (featureMember.HasElements)
            {
                var featureElement = featureMember.Descendants().FirstOrDefault();
                if (featureElement != null)
                {
                    featureInfo.LayerName = featureElement.Name.LocalName;
                    var elements = from ele in featureElement.Descendants() select ele;
                    foreach (var element in elements.Where(element => !element.Name.LocalName.Equals("geom")))
                    {
                        featureInfo.Attributes.Add(element.Name.LocalName, element.Value);
                    }
                }
            }

            return featureInfo;
        }
    }
}

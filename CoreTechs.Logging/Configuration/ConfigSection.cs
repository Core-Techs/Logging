using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CoreTechs.Logging.Configuration
{
    public class ConfigSection : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var xml = XElement.Parse(section.OuterXml);
            var targetConfigs = xml.Descendants("target");

            var dlc = new TargetConstructor();

            var config = new LoggingConfiguration
            {
                Targets = (from l in targetConfigs
                    select dlc.Construct(l)).ToList()
            };

            return config;
        }
    }
}


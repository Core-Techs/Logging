using System.Configuration;
using System.Xml;
using System.Xml.Linq;

namespace CoreTechs.Logging.Configuration
{
    public class ConfigSection : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var xml = XElement.Parse(section.OuterXml);
            return xml;
        }
    }
}


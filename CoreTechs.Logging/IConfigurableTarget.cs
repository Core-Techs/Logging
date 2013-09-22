using System.Xml.Linq;

namespace CoreTechs.Logging
{
    interface IConfigurableTarget
    {
        void Configure(XElement xml);
    }
}
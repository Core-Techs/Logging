using System.Xml.Linq;

namespace CoreTechs.Logging
{
    public interface IConfigurable
    {
        void Configure(XElement xml);
    }
}
using System.Diagnostics;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("Trace")]
    public class TraceTarget : Target, IConfigurable
    {
        public IEntryConverter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            var fmt = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = fmt.Convert(entry);
            Trace.WriteLine(msg);
        }

        public void Configure(XElement xml)
        {
            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("EntryFormatter", "Formatter"));
        }
    }
}
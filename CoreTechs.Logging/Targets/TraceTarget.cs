using System.Diagnostics;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("Trace")]
    public class TraceTarget : Target
    {
        public IEntryFormatter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            var fmt = EntryFormatter ?? entry.Logger.Config.GetFormatter<string>();
            var msg = fmt.Format(entry);
            Trace.WriteLine(msg);
        }

        public override void Configure(XElement xml)
        {
            EntryFormatter =
                ConstructOrDefault<IEntryFormatter<string>>(xml.GetAttributeValue("EntryFormatter", "Formatter"));
        }
    }
}
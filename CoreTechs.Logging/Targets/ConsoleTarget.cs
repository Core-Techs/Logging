using System;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("Console")]
    public class ConsoleTarget : Target, IConfigurable
    {
        public IEntryConverter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            var formatter = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = formatter.Convert(entry);
            Console.WriteLine(msg);
        }

        public void Configure(XElement xml)
        {
            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("Formatter", "EntryFormatter"));
        }
    }
}
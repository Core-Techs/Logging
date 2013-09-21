using System;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("Console")]
    public class ConsoleTarget : Target
    {
        public IEntryFormatter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            var formatter = EntryFormatter ?? entry.Logger.Config.GetFormatter<string>();
            var msg = formatter.Format(entry);
            Console.WriteLine(msg);
        }

        public override void Configure(XElement xml)
        {
            EntryFormatter =
              ConstructOrDefault<IEntryFormatter<string>>(xml.GetAttributeValue("Formatter", "EntryFormatter"));
       
        }
    }
}
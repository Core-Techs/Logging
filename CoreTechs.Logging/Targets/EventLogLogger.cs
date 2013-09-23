using System.Diagnostics;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("EventLog")]
    public class EventLogTarget : Target, IConfigurable
    {
        /// <summary>
        /// The windows event log entry source.
        /// </summary>
        public string EventLogSource { get; set; }

        public IEntryConverter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            EventLogEntryType type;

            switch (entry.Level)
            {
                case Level.Debug:
                case Level.Trace:
                case Level.Info:
                    type = EventLogEntryType.Information;
                    break;
                case Level.Warn:
                    type = EventLogEntryType.Warning;
                    break;
                case Level.Error:
                case Level.Fatal:
                    type = EventLogEntryType.Error;
                    break;
                default:
                    type = EventLogEntryType.Information;
                    break;
            }

            using (var eventLog = new EventLog { Source = EventLogSource ?? entry.Source })
            {
                var fmt = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
                var msg = fmt.Convert(entry);
                eventLog.WriteEntry(msg, type);
            }
        }

        public void Configure(XElement xml)
        {
            EventLogSource = xml.GetAttributeValue("EventLogSource");

            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("Formatter", "EntryFormatter"));
        }
    }
}
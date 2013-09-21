using System.Diagnostics;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("EventLog")]
    public class EventLogTarget : Target
    {
        /// <summary>
        /// The windows event log entry source.
        /// </summary>
        public string EventLogSource { get; set; }

        public IEntryFormatter<string> EntryFormatter { get; set; }

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
                var fmt = EntryFormatter ?? entry.Logger.Config.GetFormatter<string>();
                var msg = fmt.Format(entry);
                eventLog.WriteEntry(msg, type);
            }
        }

        public override void Configure(XElement xml)
        {
            EventLogSource = xml.GetAttributeValue("EventLogSource");

            EntryFormatter =
                ConstructOrDefault<IEntryFormatter<string>>(xml.GetAttributeValue("Formatter", "EntryFormatter"));
        }
    }
}
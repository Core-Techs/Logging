using System;
using System.Diagnostics;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("Trace")]
    public class TraceTarget : Target, IConfigurable, IFlushable
    {
        public IEntryConverter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            var fmt = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = fmt.Convert(entry);

            switch (entry.Level)
            {
                case Level.Trace:
                case Level.Debug:
                    Trace.WriteLine(msg);
                    break;
                case Level.Info:
                    Trace.TraceInformation(msg);
                    break;
                case Level.Warn:
                    Trace.TraceWarning(msg);
                    break;
                case Level.Error:
                    Trace.TraceError(msg);
                    break;
                case Level.Fatal:
                    Trace.Fail(msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public void Configure(XElement xml)
        {
            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("EntryFormatter", "Formatter"));
        }

        public void Flush()
        {
            Trace.Flush();
        }
    }
}
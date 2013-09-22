using System.IO;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("File")]
    public class FileTarget : Target, IConfigurableTarget
    {
        public IEntryConverter<string> EntryFormatter { get; set; }
        
        public string FilePath { get; set; }

        public override void Write(LogEntry entry)
        {
            var formatter = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = formatter.Convert(entry);
            var directory = Path.GetDirectoryName(FilePath);
            Directory.CreateDirectory(directory);
            File.AppendAllText(FilePath, msg);
        }

        public void Configure(XElement xml)
        {
            FilePath = xml.GetAttributeValue("filepath") ?? xml.GetAttributeValue("path");

            if (FilePath.IsNullOrWhitespace())
                throw new LoggingConfigurationException("File path was not specified");

            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("Formatter", "EntryFormatter"));
        }
    }
}
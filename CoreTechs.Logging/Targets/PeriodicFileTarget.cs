using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    /// <summary>
    /// A logger that will log to various files that correspond to periods of time.
    /// </summary>
    [FriendlyTypeName("PeriodicFile")]
    public class PeriodicFileTarget : PeriodicTarget, IConfigurableTarget
    {
        public IEntryConverter<string> EntryFormatter { get; set; }

        /// <summary>
        /// The directory that log files will be written to. This directory should only be used for this logger when circular logging is enabled to keep other files from being overwritten.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Specifies the number of files that will be circulated through. The default value is 0. Values less than 2 will disable circular logging.
        /// </summary>
        public int CirculationCount { get; set; }

        public override void Write(LogEntry entry)
        {
            // reset periods if time
            // determine if log file circulation should occur
            var circulate = false;
            if (DateTime.Now >= NextPeriodStart)
            {
                SetPeriods();
                circulate = CirculationCount > 1;
            }

            // log entry
            Directory.CreateDirectory(DirectoryPath);
            var filename = Path.Combine(DirectoryPath, GetCurrentFilename());
            var formatter = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = formatter.Convert(entry);
            File.AppendAllText(filename, msg);

            // circulate if needed
            if (circulate)
            {
                var di = new DirectoryInfo(DirectoryPath);
                var files = di.GetFiles();

                if (files.Length > CirculationCount)
                {
                    var oldest = files.OrderBy(x => x.LastWriteTime).First();
                    oldest.Delete();
                }
            }
        }

        private string GetCurrentFilename()
        {
            return string.Format("{0:yyyyMMdd}_{0:HHmmss}.txt", ThisPeriodStart);
        }

        public new void Configure(XElement xml)
        {
            base.Configure(xml);

            int int32;
            if (int.TryParse(xml.GetAttributeValue("CirculationCount", "Count"), out int32))
                CirculationCount = int32;

            DirectoryPath = xml.GetAttributeValue("DirectoryPath", "Path", "Directory", "Dir") ??
                            Path.Combine(Environment.CurrentDirectory, "logs");
            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("EntryFormatter", "Formatter"));
        }
    }
}
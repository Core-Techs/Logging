using System.IO;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using System.Linq;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("File")]
    public class FileTarget : Target, IConfigurable
    {
        private const string Extension = ".log.txt";
        private FileInfo _logFile;

        public string Path { get; set; }
        public LoggingInterval Interval { get; set; }
        public IEntryConverter<string> EntryFormatter { get; set; }
        public int? ArchiveCount { get; set; }

        public override void Write(LogEntry entry)
        {
            if (Interval != null)
            {
                Interval.Update();
                _logFile = GetNextLogFile();
                DeleteOldLogFiles();
            }

            if (_logFile == null)
            {
                var di = new DirectoryInfo(Path);
                _logFile = di.Exists ? di.File("log.txt") : new FileInfo(Path);
            }

            if (_logFile.Directory != null) _logFile.Directory.Create();
            var fmt = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = fmt.Convert(entry);
            File.AppendAllText(_logFile.FullName, msg);
        }

        private FileInfo GetNextLogFile()
        {
            var di = new DirectoryInfo(Path);
            var filename = string.Format("{0:yyyyMMdd}_{0:HHmmss}{1}", Interval.Beginning, Extension);
            return di.File(filename);
        }

        private void DeleteOldLogFiles()
        {
            if (!ArchiveCount.HasValue) return;

            var oldFiles = new DirectoryInfo(Path)
                .EnumerateFiles("*" + Extension)
                .OrderByDescending(x => x.LastWriteTime)
                .Select((file, i) => new { file, i = i + 1 })
                .Where(x => x.i > ArchiveCount)
                .Select(x => x.file);

            foreach (var file in oldFiles) file.Delete();
        }

        public void Configure(XElement xml)
        {
            Path = xml.GetAttributeValue("path", "file", "folder", "dir", "directory", "filepath");

            ArchiveCount = Try.Get<int?>(() => int.Parse(xml.GetAttributeValue("archivecount"))).Value;

            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("entryformatter", "formatter"));

            Interval = Try.Get(() => LoggingInterval.Parse(xml.GetAttributeValue("interval"))).Value;
        }
    }
}

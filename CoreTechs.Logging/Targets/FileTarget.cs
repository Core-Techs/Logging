using System;
using System.IO;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using System.Linq;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("File")]
    public class FileTarget : Target, IConfigurable, IDisposable
    {
        private const string Extension = ".log.txt";
        private LogFile _logFile;

        public string Path { get; set; }
        public LoggingInterval Interval { get; set; }
        public IEntryConverter<string> EntryFormatter { get; set; }
        public int? ArchiveCount { get; set; }
        public bool KeepFileOpen { get; set; }

        public FileTarget()
        {
            KeepFileOpen = true;
        }

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
                _logFile = CreateLogFile(di.Exists ? di.File("log.txt") : new FileInfo(Path));
            }

            if (_logFile.FileInfo.Directory != null) _logFile.FileInfo.Directory.Create();
            var fmt = EntryFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var msg = fmt.Convert(entry);
            _logFile.Append(msg);
        }

        private LogFile GetNextLogFile()
        {
            var di = new DirectoryInfo(Path);
            var filename = string.Format("{0:yyyyMMdd}_{0:HHmmss}{1}", Interval.Beginning, Extension);
            return CreateLogFile(di.File(filename));
        }

        private LogFile CreateLogFile(FileInfo file)
        {
            return new LogFile(file) {KeepOpen = KeepFileOpen};
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
            var template = new FileTarget(); // used for default values

            KeepFileOpen =
                Try.Get(() => bool.Parse(xml.GetAttributeValue("KeepFileOpen", "KeepOpen")), template.KeepFileOpen)
                    .Value;

            Path = xml.GetAttributeValue("path", "file", "folder", "dir", "directory", "filepath");

            ArchiveCount =
                Try.Get(() => int.Parse(xml.GetAttributeValue("archivecount")), template.ArchiveCount).Value;

            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("entryformatter", "formatter"));

            Interval = Try.Get(() => LoggingInterval.Parse(xml.GetAttributeValue("interval"))).Value;
        }

        public void Dispose()
        {
            _logFile.Dispose();
        }
    }
}

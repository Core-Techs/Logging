using System;
using System.IO;
using JetBrains.Annotations;

namespace CoreTechs.Logging.Targets
{
    internal class LogFile : IDisposable
    {
        private Lazy<StreamWriter> _lazyWriter;

        public LogFile([NotNull] string path) : this(new FileInfo(path)) { }
        public LogFile([NotNull] FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");
            FileInfo = file;
            KeepOpen = true;
            DeleteOnDispose = false;
        }

        public bool DeleteOnDispose { get; set; }

        private Lazy<StreamWriter> LazyWriter
        {
            get { return _lazyWriter ?? (_lazyWriter = CreateLazyWriter()); }
        }

        public bool KeepOpen { get; set; }

        public FileInfo FileInfo { get; private set; }

        public void Dispose()
        {
            CleanupLazyWriter();
            if (DeleteOnDispose)
                Try.Do(() => FileInfo.Delete());
        }

        private Lazy<StreamWriter> CreateLazyWriter()
        {
            return
                new Lazy<StreamWriter>(
                    () =>
                        new StreamWriter(FileInfo.Open(FileMode.Append, FileAccess.Write, FileShare.Read))
                        {
                            AutoFlush = true
                        });
        }

        private void CleanupLazyWriter()
        {
            if (LazyWriter.IsValueCreated)
                Try.Do(() => LazyWriter.Value.Dispose());

            _lazyWriter = null;
        }

        public void Append(string s)
        {
            LazyWriter.Value.Write(s);

            if (!KeepOpen)
                CleanupLazyWriter();
        }

        public string ReadAllText()
        {
            using (var file = FileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(file))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
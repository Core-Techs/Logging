using System;
using System.Collections.Generic;

namespace CoreTechs.Logging
{
    public class LogEntry
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public LogEntry([NotNull] Logger logger)
        {

            Id = Guid.NewGuid();
            Created = DateTimeOffset.Now; if (logger == null) throw new ArgumentNullException("logger");
            Logger = logger;
        }

        public Logger Logger { get; set; }

        public Guid Id { get; private set; }
        public DateTimeOffset Created { get; private set; }

        public string Source { get; set; }

        public string MessageFormat { get; set; }
        public object[] MessageArgs { get; set; }

        public Level Level { get; set; }

        public Dictionary<string, object> Data
        {
            get { return _data; }
        }

        public Exception Exception { get; set; }

        public string GetMessage()
        {
            var format = MessageFormat ?? "";
            var objects = MessageArgs ?? new object[0];
            return format.SafeFormat(objects);
        }
    }
}
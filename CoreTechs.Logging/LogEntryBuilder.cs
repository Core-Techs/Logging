using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lvl = CoreTechs.Logging.Level;

namespace CoreTechs.Logging
{
    public class LogEntryBuilder
    {
        private readonly Logger _logger;

        public LogEntryBuilder([NotNull] Logger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;

            Entry = new LogEntry(logger) {Source = _logger.Name};
        }

        public LogEntry Entry { get; internal set; }

        public void Write()
        {
            _logger.Write(Entry);
        }

        public LogEntryBuilder Data(string key, object value)
        {
            Entry.Data[key] = value;
            return this;
        }

        public LogEntryBuilder Data(IEnumerable<KeyValuePair<string, object>> data)
        {
            foreach (var x in data)
                Entry.Data[x.Key] = x.Value;

            return this;
        }

        public LogEntryBuilder Exception(Exception exception)
        {
            Entry.Exception = exception;
            return this;
        }

        [StringFormatMethod("message")]
        public LogEntryBuilder Message(string message, params object[] args)
        {
            Entry.MessageFormat = message;
            Entry.MessageArgs = args;
            return this;
        }

        public LogEntryBuilder Level(Lvl level)
        {
            Entry.Level = level;
            return this;
        }

        
        public LogEntryBuilder Trace()
        {
            return Level(Lvl.Trace);
        }

        public LogEntryBuilder Debug()
        {
            return Level(Lvl.Debug);
        }

        public LogEntryBuilder Info()
        {
            return Level(Lvl.Info);
        }

        public LogEntryBuilder Warn()
        {
            return Level(Lvl.Warn);
        }

        public LogEntryBuilder Error()
        {
            return Level(Lvl.Error);
        }

        public LogEntryBuilder Fatal()
        {
            return Level(Lvl.Fatal);
        }
    }
}
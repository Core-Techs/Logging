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

        public void Level(Lvl level)
        {
            Entry.Level = level;
            _logger.Write(Entry);
        }


        public void Trace()
        {
             Level(Lvl.Trace);
        }

        public void Debug()
        {
             Level(Lvl.Debug);
        }

        public void Info()
        {
             Level(Lvl.Info);
        }

        public void Warn()
        {
             Level(Lvl.Warn);
        }

        public void Error()
        {
             Level(Lvl.Error);
        }

        public void Fatal()
        {
            Level(Lvl.Fatal);
        }
    }
}
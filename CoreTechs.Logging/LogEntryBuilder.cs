using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lvl = CoreTechs.Logging.Level;

namespace CoreTechs.Logging
{
    public class LogEntryBuilder
    {
        private const string FormatParameterName = "message";
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

        [StringFormatMethod(FormatParameterName)]
        public void Level(Lvl level, string message = null, params object[] args)
        {
            Entry.Level = level;
            _logger.Write(Entry);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Trace(string message = null, params object[] args)
        {
            Level(Lvl.Trace, message, args);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Debug(string message = null, params object[] args)
        {
            Level(Lvl.Debug, message, args);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Info(string message = null, params object[] args)
        {
            Level(Lvl.Info, message, args);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Warn(string message = null, params object[] args)
        {
            Level(Lvl.Warn, message, args);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Error(string message = null, params object[] args)
        {
            Level(Lvl.Error, message, args);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Fatal(string message = null, params object[] args)
        {
            Level(Lvl.Fatal, message, args);
        }
    }
}
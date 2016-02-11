using System;
using System.Collections.Generic;

namespace CoreTechs.Logging
{
    public class Logger
    {
        public LogManager LogManager { get; private set; }

        private const string FormatParameterName = "message";

        public Logger([NotNull] LogManager logManager, [NotNull] string name)
        {
            if (logManager == null) throw new ArgumentNullException(nameof(logManager));
            if (name == null) throw new ArgumentNullException(nameof(name));
            LogManager = logManager;
            Name = name;
        }

        public string Name { get; private set; }

        public void Write([NotNull] LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            LogManager.WriteEntry(entry);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Trace(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Trace(message, args);
        }

        public void Trace(object obj)
        {
            new LogEntryBuilder(this).Trace(obj);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Debug(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Debug(message, args);
        }

        public void Debug(object obj)
        {
            new LogEntryBuilder(this).Debug(obj);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Info(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Info(message, args);
        }

        public void Info(object obj)
        {
            new LogEntryBuilder(this).Info(obj);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Warn(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Warn(message, args);
        }

        public void Warn(object obj)
        {
            new LogEntryBuilder(this).Warn(obj);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Error(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Error(message, args);
        }

        public void Error(object obj)
        {
            new LogEntryBuilder(this).Error(obj);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Fatal(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Fatal(message, args);
        }

        public void Fatal(object obj)
        {
            new LogEntryBuilder(this).Fatal(obj);
        }

        public LogEntryBuilder Exception(Exception exception)
        {
            return new LogEntryBuilder(this).Exception(exception);
        }

        public LogEntryBuilder Data(IEnumerable<KeyValuePair<string, object>> data)
        {
            return new LogEntryBuilder(this).Data(data);
        }

        public LogEntryBuilder Data(ILogDataSource dataSource)
        {
            return new LogEntryBuilder(this).Data(dataSource);
        }

        public LogEntryBuilder Data(string key, object value)
        {
            return new LogEntryBuilder(this).Data(key, value);
        }
    }
}
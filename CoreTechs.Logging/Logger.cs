using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    public class Logger
    {
        public LogManager LogManager { get; set; }

        private const string FormatParameterName = "message";

        public Logger([NotNull] LogManager logManager, [NotNull] string name)
        {
            if (logManager == null) throw new ArgumentNullException("logManager");
            if (name == null) throw new ArgumentNullException("name");
            LogManager = logManager;
            Name = name;
        }

        public string Name { get; private set; }

        public void Write([NotNull] LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            LogManager.WriteEntry(entry);
        }

        [StringFormatMethod(FormatParameterName)]
        public void Trace(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Message(message, args).Trace();
        }

        [StringFormatMethod(FormatParameterName)]
        public void Debug(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Message(message, args).Debug();
        }

        [StringFormatMethod(FormatParameterName)]
        public void Info(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Message(message, args).Info();
        }

        [StringFormatMethod(FormatParameterName)]
        public void Warn(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Message(message, args).Warn();
        }

        [StringFormatMethod(FormatParameterName)]
        public void Error(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Message(message, args).Error();
        }

        [StringFormatMethod(FormatParameterName)]
        public void Fatal(string message = null, params object[] args)
        {
            new LogEntryBuilder(this).Message(message, args).Fatal();
        }

        public LogEntryBuilder Exception(Exception exception)
        {
            return new LogEntryBuilder(this).Exception(exception);
        }

        public LogEntryBuilder Message(string message, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args);
        }

        public LogEntryBuilder Data(IEnumerable<KeyValuePair<string, object>> data)
        {
            return new LogEntryBuilder(this).Data(data);
        }

        public LogEntryBuilder Data(string key, object value)
        {
            return new LogEntryBuilder(this).Data(key, value);
        }
    }
}
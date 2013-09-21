using System;
using System.Configuration;
using System.Diagnostics;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    public class Logger
    {
        public LoggingConfiguration Config { get; set; }

        private const string FormatParameterName = "message";

        internal Logger([NotNull] string name, [NotNull] LoggingConfiguration config)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (config == null) throw new ArgumentNullException("config");
            Name = name;
            Config = config;
        }

        public string Name { get; private set; }

        /// <summary>
        ///     Creates a logger with the given name.
        /// </summary>
        public static Logger For([NotNull] string name, LoggingConfiguration config = null)
        {
            config = config ?? LoggingConfiguration.Current;
            if (name == null) throw new ArgumentNullException("name");
            return config.GetLogger(name);
        }

        /// <summary>
        ///     Creates a logger instance named after the passed type.
        /// </summary>
        public static Logger For(Type type, LoggingConfiguration config = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            return For(type.FullName, config);
        }

        /// <summary>
        ///     Creates a logger instance named after the given type parameter.
        /// </summary>
        public static Logger For<T>(LoggingConfiguration config = null)
        {
            return For(typeof(T), config);
        }

        /// <summary>
        ///     Creates a logger instance named after the calling object's type.
        /// </summary>
        public static Logger ForThis(LoggingConfiguration config = null)
        {
            var type = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return For(type, config);
        }

        public void Write([NotNull] LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            Config.WriteEntry(entry);
        }

        [StringFormatMethod(FormatParameterName)]
        public LogEntryBuilder Trace(string message = null, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args).Trace();
        }

        [StringFormatMethod(FormatParameterName)]
        public LogEntryBuilder Debug(string message = null, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args).Debug();
        }

        [StringFormatMethod(FormatParameterName)]
        public LogEntryBuilder Info(string message = null, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args).Info();
        }

        [StringFormatMethod(FormatParameterName)]
        public LogEntryBuilder Warn(string message = null, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args).Warn();
        }

        [StringFormatMethod(FormatParameterName)]
        public LogEntryBuilder Error(string message = null, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args).Error();
        }

        [StringFormatMethod(FormatParameterName)]
        public LogEntryBuilder Fatal(string message = null, params object[] args)
        {
            return new LogEntryBuilder(this).Message(message, args).Fatal();
        }

        public void AddFormatter<TOutput>(IEntryFormatter<TOutput> formatter)
        {
            Config.Formatters[typeof(TOutput)] = formatter;
        }

        public static LoggingConfiguration Configure(string configSectionName)
        {
            return
                LoggingConfiguration.Current = (LoggingConfiguration)ConfigurationManager.GetSection(configSectionName);
        }

        public static void WaitAllWritesComplete()
        {
            LoggingConfiguration.Current.WaitAllWritesComplete();
        }
    }
}
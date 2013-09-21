using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CoreTechs.Logging.Targets;

namespace CoreTechs.Logging
{
    public class LoggingConfiguration
    {
        private static LoggingConfiguration _currentConfig;
        private readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private readonly object _loggersLock = new object();
        private IDictionary<Type, IEntryFormatter> _formatters;
        private List<Target> _targets;

        private readonly Task _writer;

        public LoggingConfiguration()
        {
            _writer = Task.Factory.StartNew(WriteQueuedEntries);
        }

        private readonly BlockingCollection<LogEntry> _logEntries = new BlockingCollection<LogEntry>();


        public void WaitAllWritesComplete()
        {
            _logEntries.CompleteAdding();
            _writer.Wait();
        }

        private void WriteQueuedEntries()
        {
            foreach (var entry in _logEntries.GetConsumingEnumerable())
            {
                var targets = entry.Logger.Config.Targets.Where(t => t.ShouldWriteInternal(entry));
                foreach (var target in targets)
                {
                    try
                    {
                        target.Write(entry);
                    }
                    catch (Exception ex)
                    {
                        OnUnhandledLoggingException(ex);
                    }
                }
            }
        }

        public IDictionary<Type, IEntryFormatter> Formatters
        {
            get { return _formatters ?? (_formatters = GetDefaultFormatters()); }
            set { _formatters = value; }
        }

        public List<Target> Targets
        {
            get { return _targets ?? (_targets = new List<Target>()); }
            set { _targets = value; }
        }

        public static LoggingConfiguration Current
        {
            get { return _currentConfig ?? (_currentConfig = new LoggingConfiguration()); }
            set { _currentConfig = value; }
        }

        private static IDictionary<Type, IEntryFormatter> GetDefaultFormatters()
        {
            return new Dictionary<Type, IEntryFormatter>
            {
                {typeof (string), new DefaultStringFormatter()}
            };
        }

        public Logger GetLogger(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            lock (_loggersLock)
            {
                // check for existence
                if (_loggers.ContainsKey(name))
                    return _loggers[name];

                // create/cache logger
                var logger = new Logger(name, this);
                _loggers.Add(name, logger);
                return logger;
            }
        }

        public IEntryFormatter<TOutput> GetFormatter<TOutput>()
        {
            IEntryFormatter formatter;
            var type = typeof(TOutput);
            if (Formatters.TryGetValue(type, out formatter))
                return (IEntryFormatter<TOutput>)Formatters[typeof(TOutput)];

            throw new FormatterNotFoundException { FormatterType = type };
        }

        public event EventHandler<UnhandledLoggingExceptionEventArgs> UnhandledLoggingException;

        protected virtual void OnUnhandledLoggingException(Exception ex)
        {
            var e = new UnhandledLoggingExceptionEventArgs(ex);
            var handler = UnhandledLoggingException;
            if (handler != null) handler(this, e);
        }

        public void WriteEntry(LogEntry entry)
        {
            try
            {
                _logEntries.Add(entry);
            }
            catch (Exception ex)
            {
                OnUnhandledLoggingException(ex);
            }
        }
    }
}
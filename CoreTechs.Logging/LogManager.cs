using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using CoreTechs.Logging.Targets;

namespace CoreTechs.Logging
{
    public class LogManager
    {
        private readonly BlockingCollection<LogEntry> _logEntries;
        private readonly Task _writer;
        private IDictionary<Type, IEntryConverter> _formatters;
        private List<Target> _targets;

        public LogManager(IEnumerable<Target> targets = null)
        {
            _logEntries = new BlockingCollection<LogEntry>();
            _writer = Task.Factory.StartNew(WriteQueuedEntries);

            if (targets != null) _targets = targets.ToList();
        }

        public static LogManager Configure(string configSectionName)
        {
            var xml = (XElement) ConfigurationManager.GetSection(configSectionName);
            var targets = xml.Descendants("target", StringComparison.OrdinalIgnoreCase);
            var dlc = new TargetConstructor();
            return new LogManager
                {
                    Targets = (targets.Select(dlc.Construct)).ToList()
                };
        }

        public Logger CreateLogger<T>()
        {
            return CreateLogger(typeof(T));
        }

        public Logger CreateLogger(Type type)
        {
            return new Logger(this, type.FullName);
        }

        public Logger CreateLogger()
        {
            var type = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return CreateLogger(type);
        }

        /// <summary>
        /// Place to store a global instance of the log manager.
        /// </summary>
        public static LogManager Global { get; set; }

        public IDictionary<Type, IEntryConverter> Formatters
        {
            get { return _formatters ?? (_formatters = GetDefaultConverters()); }
            set { _formatters = value; }
        }

        public List<Target> Targets
        {
            get { return _targets ?? (_targets = new List<Target>()); }
            set { _targets = value; }
        }

        public void WaitAllWritesComplete()
        {
            _logEntries.CompleteAdding();
            _writer.Wait();
        }

        private void WriteQueuedEntries()
        {
            foreach (var entry in _logEntries.GetConsumingEnumerable())
            {
                foreach (var target in Targets.Where(t => t.ShouldWriteInternal(entry)))
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

        private static IDictionary<Type, IEntryConverter> GetDefaultConverters()
        {
            return new Dictionary<Type, IEntryConverter>
                {
                    {typeof (string), new DefaultStringConverter()}
                };
        }

        public IEntryConverter<TOutput> GetFormatter<TOutput>()
        {
            IEntryConverter formatter;
            var type = typeof(TOutput);
            if (Formatters.TryGetValue(type, out formatter))
                return (IEntryConverter<TOutput>)Formatters[typeof(TOutput)];

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
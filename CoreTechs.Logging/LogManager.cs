using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using CoreTechs.Logging.Targets;

namespace CoreTechs.Logging
{
    public class LogManager : IDisposable
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
            var xml = (XElement)ConfigurationManager.GetSection(configSectionName);

            // log manager is built here instead of the ConfigSection class
            // because ConfigurationManager caches the instances returned from GetSection()
            // better to cache the xml than the log manager

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

        /// <summary>
        /// Waits for all entries created up to the passed in time to be written and then flushes all targets.
        /// </summary>
        /// <param name="dateTime">Log entries created up to this time will be written before returning.</param>
        public void FlushEntriesAsOf(DateTimeOffset dateTime)
        {
            try
            {
                while (DateTimeOffset.Now <= dateTime || _logEntries.Any(x => x.Created <= dateTime))
                    Thread.Sleep(50);
                
                FlushTargets();
            }
            catch (Exception ex)
            {
                OnUnhandledLoggingException(ex);
            }
        }

        /// <summary>
        /// Waits for all entries created up until now to be written and then flushes all targets.
        /// </summary>
        public void FlushEntriesAsOfNow()
        {
            FlushEntriesAsOf(DateTimeOffset.Now);
        }

        private void FlushTargets()
        {
            foreach (var target in Targets.OfType<IFlushable>())
                target.Flush();
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

        /// <summary>
        /// Waits for all all log entries to be written then flushes and disposes all targets.
        /// </summary>
        public void Dispose()
        {
            _logEntries.CompleteAdding();
            _writer.Wait();

            FlushTargets();

            foreach (var target in Targets.OfType<IDisposable>())
                target.Dispose();
        }
    }
}
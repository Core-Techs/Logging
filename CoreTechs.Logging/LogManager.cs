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
        private readonly ConcurrentList<Target> _targets = new ConcurrentList<Target>();

        public LogManager(IEnumerable<Target> targets = null)
        {
            _logEntries = new BlockingCollection<LogEntry>();
            _writer = Task.Factory.StartNew(WriteQueuedEntries);

            if (targets != null)
                _targets = new ConcurrentList<Target>(targets);
        }

        public static LogManager Configure(string configSectionName)
        {
            var xml = (XElement)ConfigurationManager.GetSection(configSectionName);

            // log manager is built here instead of the ConfigSection class
            // because ConfigurationManager caches the instances returned from GetSection()
            // better to cache the xml than the log manager

            return Configure(xml);
        }

        public static LogManager Configure(XElement xml)
        {
            var targets = xml.Descendants("target", StringComparison.OrdinalIgnoreCase);
            var dlc = new TargetConstructor();
            return new LogManager(targets.Select(dlc.Construct));
        }

        public Logger GetLogger(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name cannot be null or whitespace", "name");

            return new Logger(this, name);
        }

        public Logger GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        public Logger GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public Logger GetLoggerForCallingType()
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            var type = method.DeclaringType;

            if (type == null)
                throw new InvalidOperationException("Calling method has no declaring type");

            return GetLogger(type.FullName);
        }

        public Logger GetLoggerForCallingMethod(bool fullTypeName = true)
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            var type = method.DeclaringType;
            if (type == null) throw new InvalidOperationException("declaring type is null");
            var typeName = fullTypeName ? type.FullName : type.Name;
            return GetLogger(string.Format("{0}.{1}", typeName, method.Name));
        }

        public IDictionary<Type, IEntryConverter> Formatters
        {
            get { return _formatters ?? (_formatters = GetDefaultConverters()); }
            set { _formatters = value; }
        }

        public ICollection<Target> Targets
        {
            get
            {
                return _targets;
            }
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
                var theEntry = entry;

                foreach (var target in Targets.Where(t => t.ShouldWriteInternal(theEntry)))
                {
                    try
                    {
                        target.Write(entry);
                    }
                    catch (Exception ex)
                    {
                        // log if debugger is attached
                        const int level = (int) TraceLevel.Error; // best I could come up with
                        Debugger.Log(level, "CoreTechs.Logging Exception", ex.ToString());

                        // raise event for application to handle
                        OnUnhandledLoggingException(ex);
                    }

                    if (target.Final)
                        break;
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
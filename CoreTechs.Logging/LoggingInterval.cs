using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace CoreTechs.Logging
{
    public class LoggingInterval : IDisposable
    {
        private readonly Timer _timer;

        public LoggingInterval(DateTimeOffset beginning, TimeSpan duration)
        {
            Beginning = beginning;
            Duration = duration;
            _timer = new Timer(_ => OnIntervalEnding());
        }

        public LoggingInterval(int n, UnitOfTime unit, DateTimeOffset? beginning = null)
        {
            Duration = unit.AsTimeSpan().Multiply(n);
            Beginning = beginning ?? Duration.LastInstanceAsOfNow();
            _timer = new Timer(_ => OnIntervalEnding());
        }

        public DateTimeOffset Beginning { get; private set; }
        public TimeSpan Duration { get; }

        public DateTimeOffset Next => Beginning + Duration;

        public event EventHandler IntervalEnding;

        protected virtual void OnIntervalEnding()
        {
            try
            {
                IntervalEnding?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // do not let exceptions go unhandled.
                // can crash hosting application
                // TODO not nice to swallow exception!
                // logmanager has event for logging exceptions
                // but no access to it from here
                // in future, refactor stuff so logmanager 
                // can be accessed
            }
        }

        /// <summary>
        /// Updates the Beginning property based on the current time.
        /// </summary>
        /// <returns>True if the interval state changed; false otherwise.</returns>
        public bool Update()
        {
            var updated = false;
            while (DateTimeOffset.Now >= Next)
            {
                Beginning = Next;
                updated = true;
            }

            return updated;
        }

        public static LoggingInterval Parse(string s)
        {
            var match = Regex.Match(s.Trim(), @"^(?<count>\d+)\s*(?<unit>[a-zA-Z]+)$", RegexOptions.Compiled);

            if (!match.Success)
                throw new FormatException($"Could not parse the string '{s}'. Valid example: 3 Day");

            var n = int.Parse(match.Groups["count"].Value);
            var value = match.Groups["unit"].Value;

            UnitOfTime unit;
            try
            {
                unit = Enums.Parse<UnitOfTime>(value);
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid unit specified: " + value, ex);
            }

            return new LoggingInterval(n, unit);
        }

        public void StartTimer()
        {
            var dueTime = Next - DateTimeOffset.Now;

            if (dueTime < TimeSpan.Zero)
                dueTime = TimeSpan.Zero;

            _timer.Change(dueTime, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
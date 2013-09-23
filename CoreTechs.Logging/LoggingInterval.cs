using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace CoreTechs.Logging
{
    public class LoggingInterval
    {
        private Timer _timer;

        public LoggingInterval(DateTimeOffset beginning, TimeSpan duration)
        {
            Beginning = beginning;
            Duration = duration;
            InitTimer();
        }

        public LoggingInterval(int n, UnitOfTime unit, DateTimeOffset? beginning = null)
        {
            Duration = unit.AsTimeSpan().Multiply(n);
            Beginning = beginning ?? Duration.LastInstanceAsOfNow();
            InitTimer();
        }

        public DateTimeOffset Beginning { get; private set; }
        public TimeSpan Duration { get; private set; }

        public DateTimeOffset Next
        {
            get { return Beginning + Duration; }
        }

        private void InitTimer()
        {
            _timer = new Timer(state => OnIntervalEnding());
        }

        public event EventHandler IntervalEnding;

        protected virtual void OnIntervalEnding()
        {
            var handler = IntervalEnding;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Update()
        {
            while (DateTimeOffset.Now >= Next)
                Beginning = Next;
        }

        public static LoggingInterval Parse(string s)
        {
            var match = Regex.Match(s.Trim(), @"^(?<count>\d+)\s*(?<unit>[a-zA-Z]+)$", RegexOptions.Compiled);

            if (!match.Success)
                throw new FormatException(string.Format("Could not parse the string '{0}'. Valid example: 3 Day", s));

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
            _timer.Change(Next - DateTimeOffset.Now, TimeSpan.FromMilliseconds(-1));
        }
    }
}
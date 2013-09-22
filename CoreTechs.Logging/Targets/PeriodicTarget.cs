using System;
using System.Threading;
using System.Xml.Linq;

namespace CoreTechs.Logging.Targets
{

    public abstract class PeriodicTarget : Target, IConfigurableTarget
    {
        public enum PeriodicUnit
        {
            Day = 0,
            Second,
            Minute,
            Hour,
            Week
        }

        private readonly Timer _timer;
        private int _duration = 1;

        /// <summary>
        /// The unit of measurement for periods of time.
        /// </summary>
        public virtual PeriodicUnit Unit { get; set; }

        /// <summary>
        /// The number of units in each period. The default is 1.
        /// </summary>
        public virtual int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// The time that this period began.
        /// </summary>
        public DateTime ThisPeriodStart { get; private set; }

        /// <summary>
        /// The time that the next period will begin.
        /// </summary>
        public DateTime NextPeriodStart { get; private set; }

        protected PeriodicTarget()
        {
            _timer = new Timer(state => OnPeriodEnd());
        }

        /// <summary>
        /// Calculates and sets ThisPeriodStart and NextPeriodStart.
        /// </summary>
        protected void SetPeriods()
        {
            var periodTimeSpan = CalculateTimeSpan(Unit, Duration);

            ThisPeriodStart = CalculateThisPeriodStart();
            NextPeriodStart = ThisPeriodStart.Add(periodTimeSpan);

            // update timer
            _timer.Change(periodTimeSpan, TimeSpan.FromMilliseconds(-1));
        }

        protected DateTime CalculateThisPeriodStart()
        {
            var theBeginning = new DateTime(2011, 1, 1);
            var now = DateTime.Now;

            var timeSpan = CalculateTimeSpan(Unit, Duration);
            var periodsPassed = (now - theBeginning).Ticks / timeSpan.Ticks;

            var periodBegin = theBeginning.AddTicks(timeSpan.Ticks * periodsPassed);
            return periodBegin;
        }

        protected TimeSpan CalculateTimeSpan(PeriodicUnit unit, int duration)
        {
            TimeSpan ts;
            switch (unit)
            {
                case PeriodicUnit.Day:
                    ts = TimeSpan.FromDays(1);
                    break;
                case PeriodicUnit.Second:
                    ts = TimeSpan.FromSeconds(1);
                    break;
                case PeriodicUnit.Minute:
                    ts = TimeSpan.FromMinutes(1);
                    break;
                case PeriodicUnit.Hour:
                    ts = TimeSpan.FromHours(1);
                    break;
                case PeriodicUnit.Week:
                    ts = TimeSpan.FromDays(7);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unit");
            }

            ts = TimeSpan.FromTicks(ts.Ticks * duration);
            return ts;
        }

        protected virtual void OnPeriodEnd() { }

        public void Configure(XElement xml)
        {
            Duration = Try.Get(() => int.Parse(xml.GetAttributeValue("duration")), 1).Value;
            Unit = Try.Get(() => Enums.Parse<PeriodicUnit>(xml.GetAttributeValue("Unit"))).Value;
        }
    }
}
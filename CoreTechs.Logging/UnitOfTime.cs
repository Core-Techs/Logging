using System;

namespace CoreTechs.Logging
{
    public enum UnitOfTime : long
    {
        Second = 10000000,
        Minute = 60 * Second,
        Hour = 60 * Minute,
        Day = 24 * Hour,
        Week = 7 * Day
    }

    public static class UnitOfTimeExt
    {
        public static TimeSpan AsTimeSpan(this UnitOfTime unit)
        {
            return new TimeSpan((long)unit);
        }
    }
}
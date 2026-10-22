using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Time
{
    /// <summary>The seven-day pattern that drives GDD §4.1: 5 active days, or 6 in a European week.</summary>
    public sealed class WeekSchedule
    {
        private readonly DayType[] _days;

        public WeekSchedule(int weekIndex, DayType[] days, bool hasEuropeanFixture)
        {
            if (days == null || days.Length != 7)
                throw new ArgumentException("A week must contain exactly 7 days.", nameof(days));

            WeekIndex = weekIndex;
            _days = (DayType[])days.Clone();
            HasEuropeanFixture = hasEuropeanFixture;
        }

        public int WeekIndex { get; }
        public bool HasEuropeanFixture { get; }
        public IReadOnlyList<DayType> Days => _days;

        public int ActiveDayCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _days.Length; i++)
                    if (_days[i] != DayType.Off) count++;
                return count;
            }
        }

        public DayType this[int dayOfWeek] => _days[((dayOfWeek % 7) + 7) % 7];

        public DayType ForTime(GameTime time) => this[time.DayOfWeek];

        public int CountOf(DayType type)
        {
            int count = 0;
            for (int i = 0; i < _days.Length; i++)
                if (_days[i] == type) count++;
            return count;
        }

        /// <summary>Index (0-6) of the next non-Off day strictly after <paramref name="dayOfWeek"/>.</summary>
        public int NextActiveDayIndex(int dayOfWeek, int maxLookahead = 14)
        {
            for (int i = 1; i <= maxLookahead; i++)
            {
                int index = ((dayOfWeek + i) % 7 + 7) % 7;
                if (_days[index] != DayType.Off) return i;
            }
            return -1;
        }
    }

    public static class WeekScheduleGenerator
    {
        // No European fixture: 2 gym + 2 facility + 1 match (5 active days).
        private static readonly DayType[] RegularWeek =
        {
            DayType.Facility, // Mon
            DayType.Gym,      // Tue
            DayType.Off,      // Wed
            DayType.Facility, // Thu
            DayType.Gym,      // Fri
            DayType.Match,    // Sat
            DayType.Off       // Sun
        };

        // European fixture: 2 gym + 2 facility + 2 match (6 active days).
        private static readonly DayType[] EuropeanWeek =
        {
            DayType.Facility, // Mon
            DayType.Gym,      // Tue
            DayType.Match,    // Wed - European night
            DayType.Facility, // Thu
            DayType.Gym,      // Fri
            DayType.Match,    // Sat
            DayType.Off       // Sun
        };

        public static WeekSchedule Create(int weekIndex, bool hasEuropeanFixture)
        {
            DayType[] pattern = hasEuropeanFixture ? EuropeanWeek : RegularWeek;
            return new WeekSchedule(weekIndex, pattern, hasEuropeanFixture);
        }
    }
}

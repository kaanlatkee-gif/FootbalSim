using System;

namespace FootbalSim.Core.Time
{
    public interface IScheduleService
    {
        /// <summary>The schedule for the week the clock is currently in.</summary>
        WeekSchedule CurrentWeek { get; }

        DayType Today { get; }

        bool IsBuildingOpen(string buildingId);

        /// <summary>Number of active days left in this week, including today.</summary>
        int ActiveDaysRemaining { get; }

        /// <summary>True when the player may skip (off day, or explicitly from the schedule screen).</summary>
        bool CanSkipToday { get; }

        /// <summary>
        /// Fast-forwards the clock to 07:00 on the next active day.
        /// Returns false if there is no future active day (should not happen).
        /// </summary>
        bool TrySkipToNextActiveDay(out GameTime wakeUpTime);
    }

    public sealed class ScheduleService : IScheduleService
    {
        public const int WakeHour = 7;

        private readonly IClock _clock;
        private readonly Func<int, bool> _hasEuropeanFixture;
        private WeekSchedule _cachedWeek;

        public ScheduleService(IClock clock, Func<int, bool> hasEuropeanFixture = null)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _hasEuropeanFixture = hasEuropeanFixture ?? (_ => false);
        }

        public WeekSchedule CurrentWeek
        {
            get
            {
                int weekIndex = _clock.Now.Day / 7;
                if (_cachedWeek == null || _cachedWeek.WeekIndex != weekIndex)
                    _cachedWeek = WeekScheduleGenerator.Create(weekIndex, _hasEuropeanFixture(weekIndex));

                return _cachedWeek;
            }
        }

        public DayType Today => CurrentWeek.ForTime(_clock.Now);

        public bool IsBuildingOpen(string buildingId) =>
            BuildingAccess.IsOpen(buildingId, Today, _clock.Now.Hour);

        public int ActiveDaysRemaining
        {
            get
            {
                WeekSchedule week = CurrentWeek;
                int todayIndex = _clock.Now.DayOfWeek;
                int remaining = 0;
                for (int i = todayIndex; i < 7; i++)
                    if (week[i] != DayType.Off) remaining++;
                return remaining;
            }
        }

        public bool CanSkipToday =>
            Today == DayType.Off || _clock.Now.Hour >= 22;

        public bool TrySkipToNextActiveDay(out GameTime wakeUpTime)
        {
            WeekSchedule week = CurrentWeek;
            int offset = week.NextActiveDayIndex(_clock.Now.DayOfWeek);
            wakeUpTime = _clock.Now;

            if (offset < 0) return false;

            GameTime target = new GameTime(_clock.Now.Day + offset, WakeHour, 0);
            int minutes = target.TotalMinutes - _clock.Now.TotalMinutes;
            if (minutes <= 0) return false;

            _clock.Advance(minutes);
            wakeUpTime = _clock.Now;
            return true;
        }
    }
}

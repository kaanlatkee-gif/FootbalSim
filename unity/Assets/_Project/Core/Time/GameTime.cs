using System;

namespace FootbalSim.Core.Time
{
    /// <summary>Immutable in-game timestamp. Day 0, 00:00 is the start of a career.</summary>
    public readonly struct GameTime : IEquatable<GameTime>, IComparable<GameTime>
    {
        public const int MinutesPerHour = 60;
        public const int HoursPerDay = 24;
        public const int MinutesPerDay = MinutesPerHour * HoursPerDay;

        public readonly int Day;
        public readonly int Hour;
        public readonly int Minute;

        public GameTime(int day, int hour, int minute)
        {
            int total = day * MinutesPerDay + hour * MinutesPerHour + minute;
            if (total < 0) total = 0;
            Day = total / MinutesPerDay;
            int rem = total % MinutesPerDay;
            Hour = rem / MinutesPerHour;
            Minute = rem % MinutesPerHour;
        }

        public int TotalMinutes => Day * MinutesPerDay + Hour * MinutesPerHour + Minute;

        /// <summary>Day of the week, 0 = Monday.</summary>
        public int DayOfWeek => ((Day % 7) + 7) % 7;

        public static GameTime FromTotalMinutes(int totalMinutes) =>
            new GameTime(0, 0, totalMinutes);

        public GameTime AddMinutes(int minutes) => FromTotalMinutes(TotalMinutes + minutes);
        public GameTime AddHours(int hours) => AddMinutes(hours * MinutesPerHour);

        /// <summary>True when <paramref name="other"/> is on the same calendar day.</summary>
        public bool IsSameDay(GameTime other) => Day == other.Day;

        /// <summary>Minutes until the given hour on this or the next day.</summary>
        public int MinutesUntilHour(int hour)
        {
            GameTime target = new GameTime(Day, hour, 0);
            if (target.TotalMinutes <= TotalMinutes) target = target.AddHours(HoursPerDay);
            return target.TotalMinutes - TotalMinutes;
        }

        public int CompareTo(GameTime other) => TotalMinutes.CompareTo(other.TotalMinutes);
        public bool Equals(GameTime other) => TotalMinutes == other.TotalMinutes;
        public override bool Equals(object obj) => obj is GameTime other && Equals(other);
        public override int GetHashCode() => TotalMinutes;

        public static bool operator ==(GameTime a, GameTime b) => a.Equals(b);
        public static bool operator !=(GameTime a, GameTime b) => !a.Equals(b);
        public static bool operator <(GameTime a, GameTime b) => a.TotalMinutes < b.TotalMinutes;
        public static bool operator >(GameTime a, GameTime b) => a.TotalMinutes > b.TotalMinutes;
        public static bool operator <=(GameTime a, GameTime b) => a.TotalMinutes <= b.TotalMinutes;
        public static bool operator >=(GameTime a, GameTime b) => a.TotalMinutes >= b.TotalMinutes;

        public override string ToString() =>
            "Day " + Day + " " + Hour.ToString("00") + ":" + Minute.ToString("00");
    }
}

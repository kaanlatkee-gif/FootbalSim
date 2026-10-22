using System;
using System.Collections.Generic;
using FootbalSim.Core.Common;
using FootbalSim.Core.Time;

namespace FootbalSim.Core.Progression
{
    /// <summary>
    /// Remembers how many sessions the player has used this week and on which days each drill
    /// was trained, which is what the 1.02× repeat bonus is based on (GDD §3.4).
    /// </summary>
    public sealed class TrainingTracker
    {
        private readonly Dictionary<string, int> _sessionsUsed = new Dictionary<string, int>();
        private readonly Dictionary<string, List<int>> _drillDays = new Dictionary<string, List<int>>();

        private static string SessionKey(int weekIndex, DayType type) => weekIndex + ":" + (int)type;

        public int SessionsUsed(int weekIndex, DayType type) =>
            _sessionsUsed.TryGetValue(SessionKey(weekIndex, type), out int used) ? used : 0;

        public int SessionsRemaining(int weekIndex, DayType type) =>
            Math.Max(0, TrainingRules.SessionsPerTypePerWeek - SessionsUsed(weekIndex, type));

        public Result TryStartSession(int weekIndex, DayType type)
        {
            if (type != DayType.Gym && type != DayType.Facility)
                return Result.Fail("Training sessions can only be started on gym or facility days.");

            if (SessionsRemaining(weekIndex, type) <= 0)
                return Result.Fail("Both " + type + " sessions this week are already used.");

            string key = SessionKey(weekIndex, type);
            _sessionsUsed[key] = (_sessionsUsed.TryGetValue(key, out int used) ? used : 0) + 1;
            return Result.Ok();
        }

        /// <summary>Number of distinct earlier days on which this drill was trained.</summary>
        public int PriorDistinctDays(string drillId, int currentDay)
        {
            if (!_drillDays.TryGetValue(drillId, out List<int> days)) return 0;

            int count = 0;
            var seen = new HashSet<int>();
            for (int i = 0; i < days.Count; i++)
                if (days[i] < currentDay && seen.Add(days[i])) count++;
            return count;
        }

        public float RepeatMultiplierFor(string drillId, int currentDay) =>
            TrainingRules.RepeatMultiplier(PriorDistinctDays(drillId, currentDay));

        /// <summary>Records a completed drill. Duplicate calls on the same day do not stack.</summary>
        public void RecordDrill(string drillId, int day)
        {
            if (string.IsNullOrEmpty(drillId)) return;

            if (!_drillDays.TryGetValue(drillId, out List<int> days))
            {
                days = new List<int>(4);
                _drillDays[drillId] = days;
            }

            if (!days.Contains(day)) days.Add(day);
        }

        public void LoadSessions(IReadOnlyDictionary<string, int> sessions)
        {
            _sessionsUsed.Clear();
            if (sessions == null) return;
            foreach (KeyValuePair<string, int> pair in sessions) _sessionsUsed[pair.Key] = pair.Value;
        }

        public Dictionary<string, int> SaveSessions() =>
            new Dictionary<string, int>(_sessionsUsed);

        public void LoadDrillDays(IReadOnlyDictionary<string, List<int>> days)
        {
            _drillDays.Clear();
            if (days == null) return;
            foreach (KeyValuePair<string, List<int>> pair in days)
                _drillDays[pair.Key] = new List<int>(pair.Value);
        }

        public Dictionary<string, List<int>> SaveDrillDays()
        {
            var copy = new Dictionary<string, List<int>>(_drillDays.Count);
            foreach (KeyValuePair<string, List<int>> pair in _drillDays)
                copy[pair.Key] = new List<int>(pair.Value);
            return copy;
        }
    }
}

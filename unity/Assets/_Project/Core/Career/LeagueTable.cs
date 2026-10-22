using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Career
{
    public sealed class Standing
    {
        public Standing(string clubId) => ClubId = clubId;

        public string ClubId { get; }
        public int Played { get; private set; }
        public int Won { get; private set; }
        public int Drawn { get; private set; }
        public int Lost { get; private set; }
        public int GoalsFor { get; private set; }
        public int GoalsAgainst { get; private set; }
        public int Points => Won * 3 + Drawn;
        public int GoalDifference => GoalsFor - GoalsAgainst;

        internal void Record(int scored, int conceded)
        {
            Played++;
            GoalsFor += scored;
            GoalsAgainst += conceded;

            if (scored > conceded) Won++;
            else if (scored == conceded) Drawn++;
            else Lost++;
        }
    }

    public sealed class LeagueTable
    {
        private readonly Dictionary<string, Standing> _standings = new Dictionary<string, Standing>();
        private readonly List<Standing> _sorted = new List<Standing>();

        public LeagueTable(IEnumerable<string> clubIds)
        {
            if (clubIds == null) throw new ArgumentNullException(nameof(clubIds));
            foreach (string id in clubIds) GetOrCreate(id);
        }

        public Standing GetOrCreate(string clubId)
        {
            if (!_standings.TryGetValue(clubId, out Standing standing))
            {
                standing = new Standing(clubId);
                _standings[clubId] = standing;
            }
            return standing;
        }

        public void Apply(string homeId, string awayId, int homeGoals, int awayGoals)
        {
            GetOrCreate(homeId).Record(homeGoals, awayGoals);
            GetOrCreate(awayId).Record(awayGoals, homeGoals);
        }

        /// <summary>Points, then goal difference, then goals scored, then name.</summary>
        public IReadOnlyList<Standing> Standings
        {
            get
            {
                _sorted.Clear();
                _sorted.AddRange(_standings.Values);
                _sorted.Sort(Compare);
                return _sorted;
            }
        }

        public int PositionOf(string clubId)
        {
            IReadOnlyList<Standing> table = Standings;
            for (int i = 0; i < table.Count; i++)
                if (string.Equals(table[i].ClubId, clubId, StringComparison.Ordinal)) return i + 1;
            return -1;
        }

        private static int Compare(Standing a, Standing b)
        {
            int byPoints = b.Points.CompareTo(a.Points);
            if (byPoints != 0) return byPoints;

            int byDifference = b.GoalDifference.CompareTo(a.GoalDifference);
            if (byDifference != 0) return byDifference;

            int byGoals = b.GoalsFor.CompareTo(a.GoalsFor);
            if (byGoals != 0) return byGoals;

            return string.CompareOrdinal(a.ClubId, b.ClubId);
        }
    }
}

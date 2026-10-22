using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Career
{
    public readonly struct Fixture
    {
        public readonly int Round;
        public readonly string HomeId;
        public readonly string AwayId;

        public Fixture(int round, string homeId, string awayId)
        {
            Round = round;
            HomeId = homeId;
            AwayId = awayId;
        }
    }

    /// <summary>Seeded double round-robin (circle method). Deterministic for a given club order.</summary>
    public static class FixtureGenerator
    {
        public static List<Fixture> RoundRobin(IReadOnlyList<string> clubIds, bool doubleRound)
        {
            if (clubIds == null || clubIds.Count < 2)
                throw new ArgumentException("At least two clubs are required.", nameof(clubIds));

            var teams = new List<string>(clubIds);
            bool hasBye = teams.Count % 2 != 0;
            if (hasBye) teams.Add(null); // null = bye

            int n = teams.Count;
            int roundsPerLeg = n - 1;
            int matchesPerRound = n / 2;
            var fixtures = new List<Fixture>(roundsPerLeg * matchesPerRound * (doubleRound ? 2 : 1));

            var rotation = new List<string>(teams);

            for (int round = 0; round < roundsPerLeg; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    string home = rotation[match];
                    string away = rotation[n - 1 - match];

                    if (home == null || away == null) continue;

                    bool swap = (round + match) % 2 == 1;
                    fixtures.Add(new Fixture(round, swap ? away : home, swap ? home : away));
                }

                // Rotate all but the first team.
                string last = rotation[n - 1];
                rotation.RemoveAt(n - 1);
                rotation.Insert(1, last);
            }

            if (doubleRound)
            {
                int secondLegStart = roundsPerLeg;
                int firstLegCount = fixtures.Count;
                for (int i = 0; i < firstLegCount; i++)
                {
                    Fixture f = fixtures[i];
                    fixtures.Add(new Fixture(f.Round + secondLegStart, f.AwayId, f.HomeId));
                }
            }

            return fixtures;
        }
    }
}

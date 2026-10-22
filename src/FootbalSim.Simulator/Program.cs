using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FootbalSim.Core.Attributes;
using FootbalSim.Core.Career;
using FootbalSim.Core.Common;
using FootbalSim.Core.Economy;
using FootbalSim.Core.Match;
using FootbalSim.Core.Needs;
using FootbalSim.Core.News;
using FootbalSim.Core.Progression;
using FootbalSim.Core.Time;

namespace FootbalSim.Simulator
{
    /// <summary>
    /// Headless demonstration of the Core library: plays a career week by week,
    /// training through the XP system and playing matches through the simulator.
    /// No Unity, no graphics — just the systems the game is built on.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            int seed = 20261022;
            int seasons = 3;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--help" || args[i] == "-h")
                {
                    Console.WriteLine("FootbalSim.Simulator [--seed N] [--seasons N]");
                    return 0;
                }

                if (args[i] == "--seed" && i + 1 < args.Length)
                    int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out seed);

                if (args[i] == "--seasons" && i + 1 < args.Length)
                    int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out seasons);
            }

            if (seasons < 1) seasons = 1;
            if (seasons > 10) seasons = 10;

            var career = new CareerSimulation(seed);
            career.Run(seasons);
            return 0;
        }
    }

    internal sealed class CareerSimulation
    {
        private const int StartingAttribute = 45;
        private const int PlayerClubIndex = 9;

        private readonly int _seed;
        private readonly IRandomSource _rng;
        private readonly InstantMatchSimulator _simulator = new InstantMatchSimulator();
        private readonly PositionProfile _profile = PositionProfile.Striker();
        private readonly AttributeProgression _progression;
        private readonly PlayerLevel _level = new PlayerLevel();
        private readonly NeedsModel _needs = new NeedsModel();
        private readonly Wallet _wallet = new Wallet();
        private readonly TrainingTracker _training = new TrainingTracker();
        private readonly NewsComposer _news = new NewsComposer();
        private readonly List<string> _clubs;
        private readonly int[] _clubStrength;

        private readonly List<NewsEvent> _pending = new List<NewsEvent>();

        private GameTime _now = new GameTime(0, 7, 0);
        private int _technicalCursor;
        private int _physicalCursor;
        private string _playerClub;
        private int _age = 19;
        private float _reputation = 0.8f;

        private int _seasonGoals;
        private int _seasonAssists;
        private float _ratingSum;
        private int _matchesPlayed;

        public CareerSimulation(int seed)
        {
            _seed = seed;
            _rng = new SplitMixRandom(seed);
            _progression = new AttributeProgression(StartingAttribute);

            _clubs = new List<string>
            {
                "Anadolu SK", "Karadeniz FK", "Ege United", "Toros SK", "Kale SK",
                "Liman FK", "Zeytin SK", "Bozkir SK", "Vadi FK", "Ates SK",
                "Goksu FK", "Yenisehir SK", "Demir SK", "Kuzey Yildizi", "Bati FK",
                "Sahil SK", "Marmara FK", "Ceyhan SK"
            };

            // Deterministic club strength, 55-78.
            _clubStrength = new int[_clubs.Count];
            var strengthRng = new SplitMixRandom(seed + 1);
            for (int i = 0; i < _clubStrength.Length; i++)
                _clubStrength[i] = 55 + strengthRng.NextInt(0, 24);

            _playerClub = _clubs[PlayerClubIndex];
        }

        public void Run(int seasons)
        {
            PrintHeader(seasons);

            for (int season = 1; season <= seasons; season++)
                RunSeason(season);

            PrintCareerSummary();
        }

        // ---------------------------------------------------------------- season

        private void RunSeason(int season)
        {
            List<Fixture> fixtures = FixtureGenerator.RoundRobin(_clubs, doubleRound: true);
            var table = new LeagueTable(_clubs);

            var rounds = fixtures.GroupBy(f => f.Round)
                                 .OrderBy(g => g.Key)
                                 .Select(g => g.ToList())
                                 .ToList();
            int roundCursor = 0;

            int beforeOverall = Overall();
            var before = SnapshotAttributes();

            _seasonGoals = 0;
            _seasonAssists = 0;
            _ratingSum = 0f;
            _matchesPlayed = 0;
            int seasonStartXp = _level.TotalXp;
            int seasonStartDay = _now.Day;

            int week = 0;
            while (roundCursor < rounds.Count && week < 60)
            {
                bool european = week % 4 == 2;
                WeekSchedule schedule = WeekScheduleGenerator.Create(week, european);

                if (season == 1 && week == 0) PrintWeek(schedule);

                for (int day = 0; day < 7 && roundCursor < rounds.Count; day++)
                {
                    DayType type = schedule[day];

                    if (type == DayType.Gym || type == DayType.Facility)
                    {
                        RunTrainingSession(type);
                    }
                    else if (type == DayType.Match)
                    {
                        PlayRound(rounds[roundCursor], table);
                        roundCursor++;
                    }

                    EndOfDay();
                }

                week++;
            }

            PrintSeasonReport(season, table, before, beforeOverall, seasonStartXp, seasonStartDay);
            _age++;
        }

        // -------------------------------------------------------------- training

        private void RunTrainingSession(DayType type)
        {
            // Absolute week index: sessions are limited to two per type per week, and the
            // counter must not restart when a new season begins.
            int weekIndex = _now.Day / 7;

            Result started = _training.TryStartSession(weekIndex, type);
            if (!started.IsSuccess) return;

            for (int slot = 0; slot < TrainingRules.SlotsPerSession; slot++)
            {
                AttributeId attribute = PickAttribute(type);

                string drillId = "drill." + AttributeCatalog.Get(attribute).Key.Substring("attr.".Length);
                float score = PerformanceScore();
                float repeat = _training.RepeatMultiplierFor(drillId, _now.Day);
                int xp = TrainingRules.AwardXp(score, repeat, _level.TrainingMultiplier);

                AttributeGain gain = _progression.ApplyXp(attribute, xp);

                if (gain.PointsGained > 0 && _progression.ValueOf(attribute) >= 70)
                    _pending.Add(new NewsEvent("training.milestone", 1, "Ada Yilmaz", _playerClub,
                        _progression.ValueOf(attribute)));

                _training.RecordDrill(drillId, _now.Day);
                _needs.Apply(ActivityCosts.TrainingDrill);
            }

            _now = _now.AddMinutes(90);
        }

        private AttributeId PickAttribute(DayType type)
        {
            // Gym trains the six physical attributes; the facility trains the seven technical ones.
            // Each group keeps its own cursor so no attribute is starved by the other group's length.
            var pool = new List<AttributeId>();
            AttributeGroup group = type == DayType.Gym ? AttributeGroup.Physical : AttributeGroup.Technical;

            foreach (AttributeId id in AttributeCatalog.InGroup(group)) pool.Add(id);

            int index;
            if (type == DayType.Gym)
            {
                _physicalCursor++;
                index = _physicalCursor % pool.Count;
            }
            else
            {
                _technicalCursor++;
                index = _technicalCursor % pool.Count;
            }

            return pool[index];
        }

        private float PerformanceScore()
        {
            float performance = _needs.PerformanceFactor;
            float raw = 0.25f + 0.55f * _rng.NextFloat() * performance;
            raw += 0.05f; // coach present at the facility
            return GameMath.Clamp(raw, 0f, 1f);
        }

        // --------------------------------------------------------------- matches

        private void PlayRound(List<Fixture> round, LeagueTable table)
        {
            for (int i = 0; i < round.Count; i++)
            {
                Fixture fixture = round[i];
                bool isPlayerMatch = fixture.HomeId == _playerClub || fixture.AwayId == _playerClub;

                if (isPlayerMatch)
                {
                    PlayPlayerMatch(fixture, table);
                }
                else
                {
                    int homeIndex = _clubs.IndexOf(fixture.HomeId);
                    int awayIndex = _clubs.IndexOf(fixture.AwayId);
                    MatchResult simulated = _simulator.Simulate(
                        new MatchSimInput(_clubStrength[homeIndex], _clubStrength[homeIndex],
                                          _clubStrength[homeIndex], _clubStrength[awayIndex]),
                        _rng);

                    table.Apply(fixture.HomeId, fixture.AwayId, simulated.TeamGoals, simulated.OpponentGoals);
                }
            }
        }

        private void PlayPlayerMatch(Fixture fixture, LeagueTable table)
        {
            bool home = fixture.HomeId == _playerClub;
            string opponent = home ? fixture.AwayId : fixture.HomeId;
            int opponentStrength = _clubStrength[_clubs.IndexOf(opponent)];

            int overall = Overall();
            MatchResult result = _simulator.Simulate(
                new MatchSimInput(overall,
                                  _progression.ValueOf(AttributeId.Finishing),
                                  _progression.ValueOf(AttributeId.Passing),
                                  opponentStrength),
                _rng);

            int teamGoals = home ? result.TeamGoals : result.OpponentGoals;
            int opponentGoals = home ? result.OpponentGoals : result.TeamGoals;

            // Mirror the scoreline when the player is away.
            var final = new MatchResult(result.Rating, result.Goals, result.Assists, teamGoals, opponentGoals, true);

            table.Apply(fixture.HomeId, fixture.AwayId,
                home ? result.TeamGoals : result.OpponentGoals,
                home ? result.OpponentGoals : result.TeamGoals);

            _seasonGoals += final.Goals;
            _seasonAssists += final.Assists;
            _ratingSum += final.Rating;
            _matchesPlayed++;

            int careerXp = MatchXp.For(final);
            _level.AddXp(careerXp);

            bool won = final.Outcome == MatchOutcome.Win;
            _wallet.Earn(1500 + (won ? 2000 : 0) + final.Goals * 500 + final.Assists * 250,
                _now, "wage.match");

            if (final.Rating >= 8.0f)
                _pending.Add(new NewsEvent("match.rating.high", 2, "Ada Yilmaz", _playerClub));
            else if (final.Rating <= 5.0f)
                _pending.Add(new NewsEvent("match.rating.low", 1, "Ada Yilmaz", _playerClub));

            _needs.Apply(ActivityCosts.Match);
            _now = _now.AddMinutes(150);
        }

        // ----------------------------------------------------------------- misc

        private void EndOfDay()
        {
            _needs.Tick(8 * 60);                  // waking hours
            _needs.Set(NeedId.Energy, 100f);       // sleep
            _needs.Apply(ActivityCosts.Shower);    // morning shower
            _now = new GameTime(_now.Day + 1, 7, 0);
        }

        private int Overall() => _progression.ToAttributeSet().Overall(_profile);

        private int[] SnapshotAttributes()
        {
            var values = new int[AttributeCatalog.Count];
            for (int i = 0; i < values.Length; i++)
                values[i] = _progression.ValueOf((AttributeId)i);
            return values;
        }

        private int MarketValue()
        {
            float average = _matchesPlayed == 0 ? 6.5f : _ratingSum / _matchesPlayed;
            return MarketValueCalculator.Compute(Overall(), _age, average, leagueTier: 2);
        }

        // --------------------------------------------------------------- output

        private void PrintHeader(int seasons)
        {
            WriteLine("");
            WriteLine("=====================================================================");
            WriteLine("  FootbalSim - headless career simulation");
            WriteLine("=====================================================================");
            WriteLine("  seed            : " + _seed);
            WriteLine("  seasons         : " + seasons);
            WriteLine("  player          : Ada Yilmaz, 19, ST");
            WriteLine("  club            : " + _playerClub + "  (league tier 2)");
            WriteLine("  start overall   : " + Overall() + "   (every attribute " + StartingAttribute + ")");
            WriteLine("  start money     : " + Money(_wallet.Balance));
            WriteLine("");
        }

        private void PrintWeek(WeekSchedule week)
        {
            WriteLine("  First week of the season:");
            string[] names = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int i = 0; i < 7; i++)
            {
                DayType type = week[i];
                string label;
                switch (type)
                {
                    case DayType.Gym: label = "gym session (3 drills)"; break;
                    case DayType.Facility: label = "facility session (3 drills)"; break;
                    case DayType.Match: label = "match day"; break;
                    default: label = "off - skip to next active day"; break;
                }
                WriteLine("    " + names[i] + "  " + label);
            }
            WriteLine("    active days: " + week.ActiveDayCount + " of 7");
            WriteLine("");
        }

        private void PrintSeasonReport(int season, LeagueTable table, int[] before, int beforeOverall,
                                       int seasonStartXp, int seasonStartDay)
        {
            int position = table.PositionOf(_playerClub);
            Standing standing = table.GetOrCreate(_playerClub);

            float averageRating = _matchesPlayed == 0 ? 0f : _ratingSum / _matchesPlayed;
            int xpGained = _level.TotalXp - seasonStartXp;

            WriteLine("");
            WriteLine("---------------------------------------------------------------------");
            WriteLine("  SEASON " + season + "   (age " + _age + ", " + (_now.Day - seasonStartDay) + " days)");
            WriteLine("---------------------------------------------------------------------");
            WriteLine("  matches        : " + _matchesPlayed);
            WriteLine("  record         : " + standing.Won + "W " + standing.Drawn + "D " + standing.Lost + "L"
                        + "   position " + position + " of " + _clubs.Count);
            WriteLine("  goals/assists  : " + _seasonGoals + " / " + _seasonAssists);
            WriteLine("  average rating : " + averageRating.ToString("0.00", CultureInfo.InvariantCulture));
            WriteLine("  overall        : " + beforeOverall + " -> " + Overall());
            WriteLine("  career level   : " + _level.Level + "  (+" + xpGained + " career XP, "
                        + Percent(_level.TrainingMultiplier - 1f) + " training bonus)");
            WriteLine("  market value   : " + Money(MarketValue() * 1000) + "   wage "
                        + Money(MarketValueCalculator.WeeklyWageFor(MarketValue())) + "/wk");
            WriteLine("  bank balance   : " + Money(_wallet.Balance));
            WriteLine("");

            PrintRisers(before);
            PrintNews();
            PrintTransferInterest();
        }

        private void PrintRisers(int[] before)
        {
            var risers = new List<(AttributeId id, int delta)>();
            for (int i = 0; i < AttributeCatalog.Count; i++)
            {
                int delta = _progression.ValueOf((AttributeId)i) - before[i];
                if (delta != 0) risers.Add(((AttributeId)i, delta));
            }

            risers.Sort((a, b) => b.delta.CompareTo(a.delta));

            WriteLine("  biggest gains:");
            for (int i = 0; i < Math.Min(5, risers.Count); i++)
            {
                AttributeDefinition definition = AttributeCatalog.Get(risers[i].id);
                WriteLine("    " + definition.DisplayName.PadRight(14)
                          + (before[(int)risers[i].id]) + " -> " + _progression.ValueOf(risers[i].id)
                          + "  (+" + risers[i].delta + ")");
            }

            if (risers.Count == 0) WriteLine("    (none)");
            WriteLine("");
        }

        private void PrintNews()
        {
            IReadOnlyList<string> lines = _news.Compose(_pending, 3);
            WriteLine("  evening bulletin:");
            if (lines.Count == 0) WriteLine("    (a quiet day)");
            for (int i = 0; i < lines.Count; i++) WriteLine("    - " + lines[i]);

            _pending.Clear();
            WriteLine("");
        }

        private void PrintTransferInterest()
        {
            int value = MarketValue();
            var suitors = new List<(string club, int expected)>
            {
                (_clubs[15], value * 3 / 2),
                (_clubs[4],  value),
                (_clubs[0],  value * 2 / 3)
            };

            WriteLine("  transfer interest (your approach, shown as on the wheel):");
            foreach ((string club, int expected) in suitors)
            {
                float even = TransferProbability.Accept(value, expected, expected / 2, expected / 2, _reputation);
                float greedy = TransferProbability.Accept(value, expected, expected / 2, expected / 2 * 3 / 2, _reputation);
                WriteLine("    " + club.PadRight(14) + " at asking wage " + Percent(even) + "   at +50% wage " + Percent(greedy));
            }

            (string target, int expectedValue) = suitors[1];
            float probability = TransferProbability.Accept(value, expectedValue, expectedValue / 2, expectedValue / 2, _reputation);
            TransferOutcome outcome = TransferResolver.Resolve(probability, OfferOrigin.PlayerToClub, _rng);

            WriteLine("    -> approach to " + target + ": " + Percent(probability) + " chance, "
                      + (outcome.Accepted ? "ACCEPTED" : "rejected"));

            if (!outcome.Accepted)
                WriteLine("       fans " + outcome.FansDelta + ", coach " + outcome.CoachDelta
                          + "  (the cost of a failed approach)");

            WriteLine("");
        }

        private void PrintCareerSummary()
        {
            WriteLine("");
            WriteLine("=====================================================================");
            WriteLine("  CAREER SUMMARY");
            WriteLine("=====================================================================");
            WriteLine("  name        : Ada Yilmaz, " + _age + ", " + _profile.Name);
            WriteLine("  overall     : " + Overall());
            WriteLine("  level       : " + _level.Level + "  (" + _level.TotalXp + " career XP)");
            WriteLine("  market value: " + Money(MarketValue() * 1000));
            WriteLine("");
            WriteLine("  attribute            value   next point");
            WriteLine("  -------------------  -----   ----------");

            for (int i = 0; i < AttributeCatalog.Count; i++)
            {
                AttributePool pool = _progression.Pool((AttributeId)i);
                string cost = pool.IsMaxed ? "maxed" : pool.Xp + " / " + pool.CostToNext;
                WriteLine("  " + AttributeCatalog.Get((AttributeId)i).DisplayName.PadRight(19)
                          + pool.Value.ToString().PadLeft(5) + "   " + cost);
            }

            WriteLine("");
            WriteLine("  All numbers above come from the same Core library the Unity build uses.");
            WriteLine("  Run the tests with:  dotnet test tests/Core.Tests");
            WriteLine("");
        }

        private static string Money(int amount) =>
            amount.ToString("N0", CultureInfo.InvariantCulture) + " EUR";

        private static string Percent(float fraction) =>
            (fraction * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";

        private static void WriteLine(string text) => Console.WriteLine(text);
    }
}

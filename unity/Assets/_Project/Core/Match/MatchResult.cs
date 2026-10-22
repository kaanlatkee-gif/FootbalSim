using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Match
{
    /// <summary>
    /// The single result shape produced by both a played match and a skipped (simulated) one,
    /// so downstream systems never branch on "was it played" (ARCHITECTURE §7.7).
    /// </summary>
    public readonly struct MatchResult
    {
        public readonly float Rating;
        public readonly int Goals;
        public readonly int Assists;
        public readonly int TeamGoals;
        public readonly int OpponentGoals;
        public readonly MatchOutcome Outcome;
        public readonly bool PlayedByPlayer;

        public MatchResult(float rating, int goals, int assists, int teamGoals, int opponentGoals, bool playedByPlayer)
        {
            Rating = GameMath.Clamp(rating, RatingTracker.MinRating, RatingTracker.MaxRating);
            Goals = Math.Max(0, goals);
            Assists = Math.Max(0, assists);
            TeamGoals = Math.Max(0, teamGoals);
            OpponentGoals = Math.Max(0, opponentGoals);
            PlayedByPlayer = playedByPlayer;

            Outcome = TeamGoals > OpponentGoals ? MatchOutcome.Win
                : TeamGoals == OpponentGoals ? MatchOutcome.Draw
                : MatchOutcome.Loss;
        }
    }

    /// <summary>
    /// GDD §3.5 — career XP comes only from matches (decision D17).
    /// </summary>
    public static class MatchXp
    {
        public const int MaxRatingContribution = 600;

        public static int For(MatchResult result)
        {
            int ratingXp = (int)MathF.Max(0f, 120f * (result.Rating - 5f));
            if (ratingXp > MaxRatingContribution) ratingXp = MaxRatingContribution;

            int resultXp = result.Outcome == MatchOutcome.Win ? 120
                : result.Outcome == MatchOutcome.Draw ? 60
                : 0;

            return ratingXp + resultXp + 60 * result.Goals + 40 * result.Assists;
        }
    }

    /// <summary>Inputs for a skipped match. Played matches do not use this.</summary>
    public readonly struct MatchSimInput
    {
        public readonly int Overall;
        public readonly int Finishing;
        public readonly int Passing;
        public readonly float OpponentStrength;

        public MatchSimInput(int overall, int finishing, int passing, float opponentStrength)
        {
            Overall = overall;
            Finishing = finishing;
            Passing = passing;
            OpponentStrength = opponentStrength;
        }
    }

    /// <summary>All tunable match-simulation constants in one place (ARCHITECTURE §4).</summary>
    public sealed class MatchSimTuning
    {
        public float SquadBaseline = 62f;
        public float PlayerWeight = 0.55f;
        public float BaseGoals = 1.15f;
        public float StrengthDivisor = 26f;
        public float MinLambda = 0.15f;
        public float MaxLambda = 3.2f;
        public float GoalShareBase = 0.10f;
        public float GoalShareFromFinishing = 0.40f;
        public float AssistShareBase = 0.10f;
        public float AssistShareFromPassing = 0.40f;
        public float MaxShare = 0.55f;
        public float MinShare = 0.08f;
        public float WinRatingBonus = 0.35f;
        public float DrawRatingBonus = 0.05f;
        public float LossRatingBonus = -0.25f;
        public float RatingNoise = 0.5f;

        public static MatchSimTuning Default() => new MatchSimTuning();
    }

    /// <summary>
    /// Attribute-weighted instant result for skipped match days. Pure Core, seeded, deterministic.
    /// </summary>
    public sealed class InstantMatchSimulator
    {
        private readonly MatchSimTuning _tuning;

        public InstantMatchSimulator(MatchSimTuning tuning = null) =>
            _tuning = tuning ?? MatchSimTuning.Default();

        public MatchResult Simulate(MatchSimInput input, IRandomSource rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            // Own team strength is the player's own quality plus a flat squad baseline.
            double teamStrength = _tuning.PlayerWeight * input.Overall
                                + (1f - _tuning.PlayerWeight) * _tuning.SquadBaseline;
            double diff = teamStrength - input.OpponentStrength;

            double lambdaFor = GameMath.Clamp(
                (float)(_tuning.BaseGoals + diff / _tuning.StrengthDivisor),
                _tuning.MinLambda, _tuning.MaxLambda);
            double lambdaAgainst = GameMath.Clamp(
                (float)(_tuning.BaseGoals - diff / _tuning.StrengthDivisor),
                _tuning.MinLambda, _tuning.MaxLambda);

            int teamGoals = SamplePoisson(rng, lambdaFor);
            int opponentGoals = SamplePoisson(rng, lambdaAgainst);

            int goals = 0;
            int assists = 0;

            float scoringShare = GameMath.Clamp(
                _tuning.GoalShareBase + input.Finishing / 99f * _tuning.GoalShareFromFinishing,
                _tuning.MinShare, _tuning.MaxShare);
            float assistShare = GameMath.Clamp(
                _tuning.AssistShareBase + input.Passing / 99f * _tuning.AssistShareFromPassing,
                _tuning.MinShare, _tuning.MaxShare);

            for (int i = 0; i < teamGoals; i++)
            {
                if (rng.NextFloat() < scoringShare) goals++;
                else if (rng.NextFloat() < assistShare) assists++;
            }

            MatchOutcome outcome = teamGoals > opponentGoals ? MatchOutcome.Win
                : teamGoals == opponentGoals ? MatchOutcome.Draw
                : MatchOutcome.Loss;

            float rating = RatingTracker.StartRating
                + 0.90f * goals
                + 0.60f * assists
                + (outcome == MatchOutcome.Win ? _tuning.WinRatingBonus
                    : outcome == MatchOutcome.Draw ? _tuning.DrawRatingBonus
                    : _tuning.LossRatingBonus)
                + (rng.NextFloat() - 0.5f) * _tuning.RatingNoise;

            return new MatchResult(rating, goals, assists, teamGoals, opponentGoals, playedByPlayer: false);
        }

        private static int SamplePoisson(IRandomSource rng, double lambda)
        {
            double limit = Math.Exp(-lambda);
            double product = 1.0;
            int k = 0;

            do
            {
                k++;
                product *= rng.NextDouble();
            }
            while (product > limit && k < 18);

            return k - 1;
        }
    }
}

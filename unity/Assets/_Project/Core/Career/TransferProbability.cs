using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Career
{
    public enum OfferOrigin
    {
        ClubToPlayer = 0,
        PlayerToClub = 1
    }

    public readonly struct TransferOffer
    {
        public readonly string ClubId;
        public readonly int Wage;
        public readonly int Years;
        public readonly int Fee;
        public readonly int SigningBonus;
        public readonly OfferOrigin Origin;

        public TransferOffer(string clubId, int wage, int years, int fee, int signingBonus, OfferOrigin origin)
        {
            ClubId = clubId;
            Wage = wage;
            Years = Math.Max(1, years);
            Fee = fee;
            SigningBonus = signingBonus;
            Origin = origin;
        }
    }

    /// <summary>GDD §4.8. The wheel shows this probability; the RNG is rolled before it animates.</summary>
    public static class TransferProbability
    {
        public const float MinAccept = 0.05f;
        public const float MaxAccept = 0.90f;

        public static float Accept(
            int marketValue,
            int clubExpectedValue,
            int expectedWage,
            int demandedWage,
            float reputationFactor = 1f)
        {
            if (clubExpectedValue <= 0) return MinAccept;

            float fit = GameMath.Clamp((float)marketValue / clubExpectedValue, 0.2f, 1.5f);

            float wageFactor;
            if (expectedWage <= 0)
            {
                wageFactor = 1f;
            }
            else
            {
                float overDemand = (float)(demandedWage - expectedWage) / expectedWage;
                wageFactor = GameMath.Clamp(1f - 0.5f * overDemand, 0.10f, 1.10f);
            }

            float probability = 0.55f * fit * wageFactor * GameMath.Clamp(reputationFactor, 0.5f, 1.5f);
            return GameMath.Clamp(probability, MinAccept, MaxAccept);
        }
    }

    public readonly struct TransferOutcome
    {
        public readonly bool Accepted;
        public readonly float Probability;
        public readonly int FansDelta;
        public readonly int CoachDelta;

        public TransferOutcome(bool accepted, float probability, int fansDelta, int coachDelta)
        {
            Accepted = accepted;
            Probability = probability;
            FansDelta = fansDelta;
            CoachDelta = coachDelta;
        }
    }

    /// <summary>
    /// Rolls the outcome. A rejected self-initiated approach costs fan and coach goodwill
    /// (GDD §4.8) — the only negative consequence of trying.
    /// </summary>
    public static class TransferResolver
    {
        public const int RejectedFansPenalty = -8;
        public const int RejectedCoachPenalty = -5;
        public const int RejectedReputationPenalty = -3;

        public static TransferOutcome Resolve(
            float probability, OfferOrigin origin, IRandomSource rng)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            bool accepted = rng.NextFloat() < GameMath.Clamp(probability, 0f, 1f);

            if (accepted || origin == OfferOrigin.ClubToPlayer)
                return new TransferOutcome(accepted, probability, 0, 0);

            return new TransferOutcome(false, probability, RejectedFansPenalty, RejectedCoachPenalty);
        }
    }
}

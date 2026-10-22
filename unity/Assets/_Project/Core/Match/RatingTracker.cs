using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Match
{
    /// <summary>GDD §4.5 — starts at 6.5, moves with events, clamped 3.0-10.0.</summary>
    public sealed class RatingTracker
    {
        public const float StartRating = 6.5f;
        public const float MinRating = 3.0f;
        public const float MaxRating = 10.0f;

        private float _rating = StartRating;

        public float Rating => GameMath.Clamp(_rating, MinRating, MaxRating);

        public event Action<float> RatingChanged;
        public event Action<MatchEventType, float> EventApplied;

        public void Reset()
        {
            _rating = StartRating;
            RatingChanged?.Invoke(Rating);
        }

        public float Apply(MatchEventType type)
        {
            _rating += DeltaFor(type);
            float clamped = GameMath.Clamp(_rating, MinRating, MaxRating);
            _rating = clamped;

            EventApplied?.Invoke(type, clamped);
            RatingChanged?.Invoke(clamped);
            return clamped;
        }

        public static float DeltaFor(MatchEventType type)
        {
            switch (type)
            {
                case MatchEventType.Goal: return 0.90f;
                case MatchEventType.Assist: return 0.60f;
                case MatchEventType.KeyPass: return 0.15f;
                case MatchEventType.ShotOnTarget: return 0.08f;
                case MatchEventType.SuccessfulDribble: return 0.05f;
                case MatchEventType.TackleWon: return 0.10f;
                case MatchEventType.PossessionLost: return -0.12f;
                case MatchEventType.FailedPassOwnThird: return -0.08f;
                case MatchEventType.BigMiss: return -0.30f;
                case MatchEventType.Foul: return -0.10f;
                case MatchEventType.YellowCard: return -0.25f;
                case MatchEventType.RedCard: return -0.90f;
                case MatchEventType.OwnGoal: return -0.90f;
                default: return 0f;
            }
        }
    }
}

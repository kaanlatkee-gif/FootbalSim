using System;
using System.Collections.Generic;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Progression
{
    /// <summary>GDD §3.4 / §8 — every training constraint from the source note, in one testable place.</summary>
    public static class TrainingRules
    {
        public const int SessionsPerTypePerWeek = 2;
        public const int SlotsPerSession = 3;
        public const int MaxSameDrillPerSession = 2;
        public const float RepeatBonusPerDay = 1.02f;
        public const float RepeatBonusCap = 1.50f;

        /// <summary>Bonus for having trained this drill on N previous distinct days.</summary>
        public static float RepeatMultiplier(int priorDistinctDays)
        {
            if (priorDistinctDays <= 0) return 1f;
            float value = MathF.Pow(RepeatBonusPerDay, priorDistinctDays);
            return MathF.Min(RepeatBonusCap, value);
        }

        /// <summary>XP actually awarded for a drill performance.</summary>
        public static int AwardXp(float performanceScore01, float repeatMultiplier, float coachMultiplier = 1f)
        {
            int tier = XpTier.FromScore(performanceScore01);
            return (int)Math.Round(tier * (double)repeatMultiplier * coachMultiplier, MidpointRounding.AwayFromZero);
        }
    }

    /// <summary>One training session: at most 3 slots, at most 2 uses of the same drill.</summary>
    public sealed class TrainingSession
    {
        public readonly struct Slot
        {
            public readonly string DrillId;
            public readonly int SlotIndex;

            public Slot(string drillId, int slotIndex)
            {
                DrillId = drillId;
                SlotIndex = slotIndex;
            }
        }

        private readonly List<Slot> _slots = new List<Slot>(TrainingRules.SlotsPerSession);

        public IReadOnlyList<Slot> Slots => _slots;
        public int SlotsUsed => _slots.Count;
        public bool IsComplete => _slots.Count >= TrainingRules.SlotsPerSession;

        public int CountOf(string drillId)
        {
            int count = 0;
            for (int i = 0; i < _slots.Count; i++)
                if (string.Equals(_slots[i].DrillId, drillId, StringComparison.Ordinal)) count++;
            return count;
        }

        public Result TryAdd(string drillId)
        {
            if (string.IsNullOrEmpty(drillId)) return Result.Fail("Unknown drill.");
            if (IsComplete) return Result.Fail("This session already has 3 drills.");
            if (CountOf(drillId) >= TrainingRules.MaxSameDrillPerSession)
                return Result.Fail("That drill can be used at most twice per session.");

            _slots.Add(new Slot(drillId, _slots.Count));
            return Result.Ok();
        }

        public void Clear() => _slots.Clear();
    }
}

using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Progression
{
    /// <summary>
    /// GDD §3.5 — career level. Deliberately slow: 1000·(L−1)² + 500·(L−1).
    /// Level 2 at 1 500 XP, level 3 at 5 000, level 4 at 10 500.
    /// </summary>
    public static class LevelCurve
    {
        public const int MaxLevel = 99;
        public const float TrainingBonusPerLevel = 0.04f;
        public const float MaxTrainingMultiplier = 2.00f;

        public static int TotalXpForLevel(int level)
        {
            if (level <= 1) return 0;
            int n = level - 1;
            return 1000 * n * n + 500 * n;
        }

        public static int LevelForTotalXp(int totalXp)
        {
            if (totalXp <= 0) return 1;
            int level = 1;
            while (level < MaxLevel && TotalXpForLevel(level + 1) <= totalXp) level++;
            return level;
        }

        public static float TrainingMultiplier(int level) =>
            MathF.Min(MaxTrainingMultiplier, 1f + TrainingBonusPerLevel * (Math.Max(1, level) - 1));

        public static int XpIntoLevel(int totalXp, int level) =>
            totalXp - TotalXpForLevel(level);

        public static float ProgressToNextLevel(int totalXp, int level)
        {
            if (level >= MaxLevel) return 1f;
            int currentFloor = TotalXpForLevel(level);
            int nextFloor = TotalXpForLevel(level + 1);
            int span = nextFloor - currentFloor;
            if (span <= 0) return 1f;
            return GameMath.Clamp((float)(totalXp - currentFloor) / span, 0f, 1f);
        }
    }

    /// <summary>Player career level. Only <see cref="TotalXp"/> is persisted; Level is derived.</summary>
    public sealed class PlayerLevel
    {
        public PlayerLevel(int totalXp = 0) => TotalXp = Math.Max(0, totalXp);

        public event Action<int> LeveledUp;

        public int TotalXp { get; private set; }
        public int Level => LevelCurve.LevelForTotalXp(TotalXp);
        public float TrainingMultiplier => LevelCurve.TrainingMultiplier(Level);
        public float ProgressToNext => LevelCurve.ProgressToNextLevel(TotalXp, Level);
        public int XpIntoLevel => LevelCurve.XpIntoLevel(TotalXp, Level);

        /// <summary>Awards career XP and returns the number of levels gained.</summary>
        public int AddXp(int amount)
        {
            if (amount <= 0) return 0;

            int before = Level;
            TotalXp += amount;
            int after = Level;
            int gained = Math.Max(0, after - before);

            if (gained > 0) LeveledUp?.Invoke(after);
            return gained;
        }
    }
}

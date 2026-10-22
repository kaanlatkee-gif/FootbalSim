using System;
using FootbalSim.Core.Attributes;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Progression
{
    public readonly struct AttributeGain
    {
        public readonly int PointsGained;
        public readonly int RemainingXp;
        public readonly int CostToNext;
        public readonly bool IsMaxed;

        public AttributeGain(int pointsGained, int remainingXp, int costToNext, bool isMaxed)
        {
            PointsGained = pointsGained;
            RemainingXp = remainingXp;
            CostToNext = costToNext;
            IsMaxed = isMaxed;
        }
    }

    /// <summary>One attribute's value plus its XP buffer.</summary>
    public sealed class AttributePool
    {
        public AttributePool(AttributeId id, int value = 40, int xp = 0)
        {
            Id = id;
            Value = GameMath.Clamp(value, AttributeCatalog.MinValue, AttributeCatalog.MaxValue);
            Xp = Math.Max(0, xp);
        }

        public AttributeId Id { get; }
        public int Value { get; private set; }
        public int Xp { get; private set; }

        public int CostToNext => XpCurve.CostToNext(Value);
        public bool IsMaxed => Value >= AttributeCatalog.MaxValue;

        /// <summary>0-1 progress towards the next point (1 when maxed).</summary>
        public float Progress
        {
            get
            {
                if (IsMaxed) return 1f;
                int cost = CostToNext;
                return cost <= 0 ? 1f : GameMath.Clamp((float)Xp / cost, 0f, 1f);
            }
        }

        public AttributeGain AddXp(int amount)
        {
            if (amount <= 0 || IsMaxed) return new AttributeGain(0, Xp, CostToNext, IsMaxed);

            Xp += amount;
            int gained = 0;

            while (!IsMaxed && Xp >= CostToNext)
            {
                Xp -= CostToNext;
                Value++;
                gained++;
            }

            if (IsMaxed) Xp = 0;

            return new AttributeGain(gained, Xp, CostToNext, IsMaxed);
        }

        public void Set(int value, int xp)
        {
            Value = GameMath.Clamp(value, AttributeCatalog.MinValue, AttributeCatalog.MaxValue);
            Xp = IsMaxed ? 0 : Math.Max(0, xp);
        }
    }
}

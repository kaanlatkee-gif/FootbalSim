using System;
using FootbalSim.Core.Attributes;

namespace FootbalSim.Core.Progression
{
    /// <summary>
    /// GDD §3.3 — from the source note: 65→66 costs 1000, 66→67 costs 1250, +250 per point,
    /// flattening to a 250 floor below 63 so early levels are quick.
    /// </summary>
    public static class XpCurve
    {
        public const int FloorCost = 250;
        public const int BaseCost = 1000;
        public const int BaseAttribute = 65;
        public const int StepPerPoint = 250;

        /// <summary>XP needed to go from <paramref name="value"/> to value+1. Returns 0 at 99.</summary>
        public static int CostToNext(int value)
        {
            if (value >= AttributeCatalog.MaxValue) return 0;
            int cost = BaseCost + StepPerPoint * (value - BaseAttribute);
            return Math.Max(FloorCost, cost);
        }

        /// <summary>Total XP required to go from <paramref name="from"/> up to <paramref name="to"/>.</summary>
        public static int TotalCost(int from, int to)
        {
            int total = 0;
            for (int v = from; v < to && v < AttributeCatalog.MaxValue; v++)
                total += CostToNext(v);
            return total;
        }
    }
}

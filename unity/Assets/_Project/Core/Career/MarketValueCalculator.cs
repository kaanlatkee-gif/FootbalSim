using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Career
{
    /// <summary>
    /// GDD §4.8. Values are expressed in thousands of the display currency so the formula
    /// stays in integer range for a 99-rated player.
    /// </summary>
    public static class MarketValueCalculator
    {
        public static int Compute(int overall, int age, float averageRating, int leagueTier)
        {
            double baseValue = 1000.0 * Math.Pow(1.12, overall - 50);

            double ageFactor;
            if (age <= 21) ageFactor = 1.30;
            else if (age <= 25) ageFactor = 1.15;
            else if (age <= 28) ageFactor = 1.00;
            else if (age <= 31) ageFactor = 0.75;
            else if (age <= 34) ageFactor = 0.50;
            else ageFactor = 0.25;

            double formFactor = GameMath.Clamp(0.8f + 0.3f * (averageRating - RatingBaseline), 0.5f, 1.4f);

            double leagueFactor = leagueTier == 1 ? 1.00 : leagueTier == 2 ? 0.60 : 0.35;

            double value = baseValue * ageFactor * formFactor * leagueFactor;
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        public const float RatingBaseline = 6.5f;

        /// <summary>Weekly wage derived from market value (GDD §4.8: roughly value/500).</summary>
        public static int WeeklyWageFor(int marketValue) => Math.Max(500, marketValue * 2);
    }
}

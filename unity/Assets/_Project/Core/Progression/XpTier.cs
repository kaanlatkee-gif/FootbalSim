namespace FootbalSim.Core.Progression
{
    /// <summary>
    /// GDD §3.4 — the five award tiers from the source note: 50 / 100 / 150 / 200 / 250.
    /// Fatigue and coach presence change the performance score, never the table.
    /// </summary>
    public static class XpTier
    {
        public const int Poor = 50;
        public const int Average = 100;
        public const int Good = 150;
        public const int Great = 200;
        public const int Perfect = 250;

        public static int FromScore(float score01)
        {
            if (score01 < 0.40f) return Poor;
            if (score01 < 0.60f) return Average;
            if (score01 < 0.80f) return Good;
            if (score01 < 0.95f) return Great;
            return Perfect;
        }

        public static string GradeName(float score01)
        {
            if (score01 < 0.40f) return "Poor";
            if (score01 < 0.60f) return "Average";
            if (score01 < 0.80f) return "Good";
            if (score01 < 0.95f) return "Great";
            return "Perfect";
        }
    }
}

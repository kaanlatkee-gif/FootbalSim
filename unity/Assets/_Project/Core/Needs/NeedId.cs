using System;

namespace FootbalSim.Core.Needs
{
    /// <summary>GDD §4.2 — four needs, 0-100. No hunger bar: food restores Energy.</summary>
    public enum NeedId
    {
        Energy = 0,
        Hygiene = 1,
        Bladder = 2,
        Intoxication = 3
    }

    public static class NeedIds
    {
        public const int Count = 4;
        public const float Max = 100f;

        public static string KeyOf(NeedId id)
        {
            switch (id)
            {
                case NeedId.Energy: return "need.energy";
                case NeedId.Hygiene: return "need.hygiene";
                case NeedId.Bladder: return "need.bladder";
                default: return "need.intoxication";
            }
        }

        public static bool TryParseKey(string key, out NeedId id)
        {
            switch (key)
            {
                case "need.energy": id = NeedId.Energy; return true;
                case "need.hygiene": id = NeedId.Hygiene; return true;
                case "need.bladder": id = NeedId.Bladder; return true;
                case "need.intoxication": id = NeedId.Intoxication; return true;
                default: id = NeedId.Energy; return false;
            }
        }
    }

    /// <summary>Per-hour rates. Positive = rises over time, negative = falls.</summary>
    public sealed class NeedsTuning
    {
        public NeedsTuning(float[] perHour)
        {
            if (perHour == null || perHour.Length != NeedIds.Count)
                throw new ArgumentException("Needs tuning needs one rate per need.", nameof(perHour));

            PerHour = (float[])perHour.Clone();
        }

        public float[] PerHour { get; }

        public float RateOf(NeedId id) => PerHour[(int)id];

        public static NeedsTuning Default() => new NeedsTuning(new[]
        {
            -1.5f,  // Energy
            -2.0f,  // Hygiene
             6.0f,  // Bladder
            -8.0f   // Intoxication
        });
    }

    /// <summary>Fixed costs applied by activities (GDD §4.2, §10.1).</summary>
    public static class ActivityCosts
    {
        public static readonly (NeedId need, float delta)[] TrainingDrill =
        {
            (NeedId.Energy, -18f),
            (NeedId.Hygiene, -12f)
        };

        public static readonly (NeedId need, float delta)[] Match =
        {
            (NeedId.Energy, -25f),
            (NeedId.Hygiene, -20f)
        };

        public static readonly (NeedId need, float delta)[] Shower =
        {
            (NeedId.Hygiene, 100f)
        };

        public static readonly (NeedId need, float delta)[] Toilet =
        {
            (NeedId.Bladder, -100f)
        };

        public static (NeedId need, float delta)[] Drink(int intoxication = 12, float energy = -4f) => new[]
        {
            (NeedId.Intoxication, intoxication),
            (NeedId.Energy, energy),
            (NeedId.Bladder, 12f)
        };
    }
}

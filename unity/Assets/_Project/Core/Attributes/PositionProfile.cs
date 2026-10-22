using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Attributes
{
    /// <summary>GDD §3.2 — overall is a weighted average; weights need not sum to 1.</summary>
    public sealed class PositionProfile
    {
        private readonly Dictionary<AttributeId, float> _weights;
        private readonly float _weightSum;

        public PositionProfile(string name, Dictionary<AttributeId, float> weights)
        {
            Name = name;
            _weights = weights ?? throw new ArgumentNullException(nameof(weights));
            _weightSum = 0f;
            foreach (KeyValuePair<AttributeId, float> pair in _weights)
                _weightSum += Math.Max(0f, pair.Value);

            if (_weightSum <= 0f) throw new ArgumentException("Profile weights must be positive.", nameof(weights));
        }

        public string Name { get; }
        public IReadOnlyDictionary<AttributeId, float> Weights => _weights;

        public float WeightOf(AttributeId id) => _weights.TryGetValue(id, out float w) ? w : 0f;

        public int Overall(AttributeSet attributes)
        {
            float sum = 0f;
            foreach (KeyValuePair<AttributeId, float> pair in _weights)
                sum += attributes.Get(pair.Key) * Math.Max(0f, pair.Value);

            return (int)Math.Round((double)sum / _weightSum, MidpointRounding.AwayFromZero);
        }

        public static PositionProfile Striker() => new PositionProfile("ST", new Dictionary<AttributeId, float>
        {
            { AttributeId.Finishing, 0.22f }, { AttributeId.Dribbling, 0.12f }, { AttributeId.Heading, 0.12f },
            { AttributeId.Crossing, 0.05f },  { AttributeId.Passing, 0.08f },   { AttributeId.Tackling, 0.03f },
            { AttributeId.LongShots, 0.10f }, { AttributeId.SprintSpeed, 0.10f }, { AttributeId.Stamina, 0.05f },
            { AttributeId.Balance, 0.05f },   { AttributeId.Strength, 0.05f },  { AttributeId.Jumping, 0.08f },
            { AttributeId.Acceleration, 0.05f }
        });

        public static PositionProfile Winger() => new PositionProfile("W", new Dictionary<AttributeId, float>
        {
            { AttributeId.Finishing, 0.10f }, { AttributeId.Dribbling, 0.20f }, { AttributeId.Heading, 0.03f },
            { AttributeId.Crossing, 0.18f },  { AttributeId.Passing, 0.10f },   { AttributeId.Tackling, 0.03f },
            { AttributeId.LongShots, 0.08f }, { AttributeId.SprintSpeed, 0.15f }, { AttributeId.Stamina, 0.05f },
            { AttributeId.Balance, 0.05f },   { AttributeId.Strength, 0.02f },  { AttributeId.Jumping, 0.01f },
            { AttributeId.Acceleration, 0.10f }
        });

        public static PositionProfile Midfielder() => new PositionProfile("CM", new Dictionary<AttributeId, float>
        {
            { AttributeId.Finishing, 0.05f }, { AttributeId.Dribbling, 0.08f }, { AttributeId.Heading, 0.05f },
            { AttributeId.Crossing, 0.08f },  { AttributeId.Passing, 0.20f },   { AttributeId.Tackling, 0.10f },
            { AttributeId.LongShots, 0.08f }, { AttributeId.SprintSpeed, 0.08f }, { AttributeId.Stamina, 0.12f },
            { AttributeId.Balance, 0.06f },   { AttributeId.Strength, 0.04f },  { AttributeId.Jumping, 0.03f },
            { AttributeId.Acceleration, 0.08f }
        });

        public static PositionProfile Defender() => new PositionProfile("CB", new Dictionary<AttributeId, float>
        {
            { AttributeId.Finishing, 0.02f }, { AttributeId.Dribbling, 0.03f }, { AttributeId.Heading, 0.14f },
            { AttributeId.Crossing, 0.03f },  { AttributeId.Passing, 0.10f },   { AttributeId.Tackling, 0.22f },
            { AttributeId.LongShots, 0.02f }, { AttributeId.SprintSpeed, 0.12f }, { AttributeId.Stamina, 0.10f },
            { AttributeId.Balance, 0.06f },   { AttributeId.Strength, 0.12f },  { AttributeId.Jumping, 0.09f },
            { AttributeId.Acceleration, 0.09f }
        });

        public static PositionProfile FromKey(string key)
        {
            switch (key)
            {
                case "ST": return Striker();
                case "W": return Winger();
                case "CM": return Midfielder();
                case "CB": return Defender();
                default: return Striker();
            }
        }
    }
}

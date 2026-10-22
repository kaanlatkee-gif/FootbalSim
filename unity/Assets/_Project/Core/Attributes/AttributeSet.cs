using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Attributes
{
    /// <summary>The 13 attribute values (1-99). XP pools live in <see cref="Progression.AttributeProgression"/>.</summary>
    public sealed class AttributeSet
    {
        private readonly int[] _values = new int[AttributeCatalog.Count];

        public AttributeSet(int initialValue = 40)
        {
            int start = GameMath.Clamp(initialValue, AttributeCatalog.MinValue, AttributeCatalog.MaxValue);
            for (int i = 0; i < _values.Length; i++) _values[i] = start;
        }

        public event Action<AttributeId, int> Changed;

        public int Get(AttributeId id) => _values[(int)id];

        public void Set(AttributeId id, int value)
        {
            int clamped = GameMath.Clamp(value, AttributeCatalog.MinValue, AttributeCatalog.MaxValue);
            if (_values[(int)id] == clamped) return;
            _values[(int)id] = clamped;
            Changed?.Invoke(id, clamped);
        }

        public void Add(AttributeId id, int delta) => Set(id, Get(id) + delta);

        public int Overall(PositionProfile profile) => profile.Overall(this);

        public float Average(AttributeGroup group)
        {
            int sum = 0, count = 0;
            for (int i = 0; i < _values.Length; i++)
            {
                if (AttributeCatalog.Get((AttributeId)i).Group != group) continue;
                sum += _values[i];
                count++;
            }
            return count == 0 ? 0f : (float)sum / count;
        }

        public float AverageAll()
        {
            int sum = 0;
            for (int i = 0; i < _values.Length; i++) sum += _values[i];
            return (float)sum / _values.Length;
        }
    }
}

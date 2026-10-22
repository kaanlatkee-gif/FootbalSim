using System;
using System.Collections.Generic;
using FootbalSim.Core.Attributes;

namespace FootbalSim.Core.Progression
{
    /// <summary>All 13 XP pools. The single place an attribute may ever change.</summary>
    public sealed class AttributeProgression
    {
        private readonly AttributePool[] _pools = new AttributePool[AttributeCatalog.Count];

        public AttributeProgression(int startingValue = 40)
        {
            for (int i = 0; i < _pools.Length; i++)
                _pools[i] = new AttributePool((AttributeId)i, startingValue);
        }

        /// <summary>Raised whenever a point is gained (value, new value).</summary>
        public event Action<AttributeId, int> AttributeIncreased;

        public AttributePool Pool(AttributeId id) => _pools[(int)id];

        public int ValueOf(AttributeId id) => _pools[(int)id].Value;

        /// <summary>Applies already-multiplied XP. Callers (ITrainingService) own the rules.</summary>
        public AttributeGain ApplyXp(AttributeId id, int xp)
        {
            AttributeGain gain = _pools[(int)id].AddXp(xp);
            for (int i = 0; i < gain.PointsGained; i++)
                AttributeIncreased?.Invoke(id, _pools[(int)id].Value);
            return gain;
        }

        public AttributeSet ToAttributeSet()
        {
            var set = new AttributeSet(AttributeCatalog.MinValue);
            for (int i = 0; i < _pools.Length; i++)
                set.Set((AttributeId)i, _pools[i].Value);
            return set;
        }

        public void Load(IReadOnlyDictionary<string, (int value, int xp)> saved)
        {
            if (saved == null) return;
            foreach (KeyValuePair<string, (int value, int xp)> pair in saved)
                if (AttributeCatalog.TryParseKey(pair.Key, out AttributeId id))
                    _pools[(int)id].Set(pair.Value.value, pair.Value.xp);
        }

        public Dictionary<string, (int value, int xp)> ToSaveDictionary()
        {
            var result = new Dictionary<string, (int, int)>(AttributeCatalog.Count);
            for (int i = 0; i < _pools.Length; i++)
                result[AttributeCatalog.KeyOf((AttributeId)i)] = (_pools[i].Value, _pools[i].Xp);
            return result;
        }
    }
}

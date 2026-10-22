using System;
using FootbalSim.Core.Common;

namespace FootbalSim.Core.Needs
{
    public sealed class NeedsModel
    {
        private readonly float[] _values = new float[NeedIds.Count];
        private readonly NeedsTuning _tuning;

        public NeedsModel(NeedsTuning tuning = null)
        {
            _tuning = tuning ?? NeedsTuning.Default();
            for (int i = 0; i < _values.Length; i++) _values[i] = 0f;
            _values[(int)NeedId.Energy] = 100f;
            _values[(int)NeedId.Hygiene] = 100f;
            _values[(int)NeedId.Bladder] = 0f;
            _values[(int)NeedId.Intoxication] = 0f;
        }

        public event Action<NeedId, float> Changed;

        public float Get(NeedId id) => _values[(int)id];
        public float Normalized(NeedId id) => _values[(int)id] / NeedIds.Max;

        public void Set(NeedId id, float value)
        {
            float clamped = GameMath.Clamp(value, 0f, NeedIds.Max);
            if (MathF.Abs(_values[(int)id] - clamped) < 0.0001f) return;
            _values[(int)id] = clamped;
            Changed?.Invoke(id, clamped);
        }

        public void Add(NeedId id, float delta) => Set(id, _values[(int)id] + delta);

        /// <summary>Advances decay for a span of in-game minutes.</summary>
        public void Tick(int gameMinutes)
        {
            if (gameMinutes <= 0) return;
            float hours = gameMinutes / 60f;
            for (int i = 0; i < _values.Length; i++)
                Set((NeedId)i, _values[i] + _tuning.RateOf((NeedId)i) * hours);
        }

        public void Apply((NeedId need, float delta)[] costs)
        {
            if (costs == null) return;
            for (int i = 0; i < costs.Length; i++) Add(costs[i].need, costs[i].delta);
        }

        /// <summary>GDD §4.2 — energy below 25 caps drill performance; at 0 the player collapses.</summary>
        public bool IsExhausted => _values[(int)NeedId.Energy] <= 0f;
        public bool IsLowEnergy => _values[(int)NeedId.Energy] < 25f;
        public bool IsFilthy => _values[(int)NeedId.Hygiene] < 30f;
        public bool NeedsToilet => _values[(int)NeedId.Bladder] >= 100f;
        public bool IsDrunk => _values[(int)NeedId.Intoxication] > 60f;

        /// <summary>0-1 multiplier applied to drill performance from the player's state.</summary>
        public float PerformanceFactor
        {
            get
            {
                float factor = 1f;
                if (IsLowEnergy) factor *= 0.6f;
                if (IsFilthy) factor *= 0.85f;
                if (IsDrunk) factor *= 0.6f;
                return factor;
            }
        }

        public void Load(System.Collections.Generic.IReadOnlyDictionary<string, float> saved)
        {
            if (saved == null) return;
            foreach (System.Collections.Generic.KeyValuePair<string, float> pair in saved)
                if (NeedIds.TryParseKey(pair.Key, out NeedId id))
                    Set(id, pair.Value);
        }

        public System.Collections.Generic.Dictionary<string, float> ToSaveDictionary()
        {
            var result = new System.Collections.Generic.Dictionary<string, float>(NeedIds.Count);
            for (int i = 0; i < _values.Length; i++)
                result[NeedIds.KeyOf((NeedId)i)] = _values[i];
            return result;
        }
    }
}

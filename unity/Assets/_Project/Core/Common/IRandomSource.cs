namespace FootbalSim.Core.Common
{
    /// <summary>
    /// Deterministic random source. Core never uses UnityEngine.Random, so results are
    /// reproducible from a seed (saves, replays, tests).
    /// </summary>
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();   // [0,1)
        double NextDouble(); // [0,1)
        void Reseed(int seed);
        int Seed { get; }
    }

    /// <summary>
    /// SplitMix32. Chosen over xorshift32 because sequential seeds (1, 2, 3…) must produce
    /// decorrelated streams — xorshift32 emits tiny first values for small seeds, which
    /// skewed match simulations toward 0-0.
    /// </summary>
    public sealed class SplitMixRandom : IRandomSource
    {
        private uint _state;

        public SplitMixRandom(int seed) => Reseed(seed);

        public int Seed { get; private set; }

        public void Reseed(int seed)
        {
            Seed = seed;
            _state = unchecked((uint)seed);
        }

        private uint NextUInt()
        {
            unchecked
            {
                _state += 0x9E3779B9u;
                uint z = _state;
                z = (z ^ (z >> 16)) * 0x85EBCA6Bu;
                z = (z ^ (z >> 13)) * 0xC2B2AE35u;
                z ^= z >> 16;
                return z;
            }
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint range = unchecked((uint)(maxExclusive - minInclusive));
            return minInclusive + (int)(NextUInt() % range);
        }

        public float NextFloat() => (NextUInt() >> 8) / 16777216f;   // 24 bits
        public double NextDouble() => (NextUInt() >> 8) / 16777216.0;
    }
}

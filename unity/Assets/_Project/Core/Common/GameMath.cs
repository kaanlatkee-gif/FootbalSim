using System;

namespace FootbalSim.Core.Common
{
    /// <summary>Engine-free math helpers. Core must never reference UnityEngine.Mathf.</summary>
    public static class GameMath
    {
        public static float Clamp(float value, float min, float max) =>
            value < min ? min : (value > max ? max : value);

        public static int Clamp(int value, int min, int max) =>
            value < min ? min : (value > max ? max : value);

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp(t, 0f, 1f);

        public static float InverseLerp(float a, float b, float value) =>
            Math.Abs(b - a) < 1e-6f ? 0f : Clamp((value - a) / (b - a), 0f, 1f);

        public static bool Approximately(float a, float b, float tolerance = 1e-4f) =>
            Math.Abs(a - b) <= tolerance;
    }
}

using System;

namespace FootbalSim.Core.Time
{
    public sealed class GameClock : IClock
    {
        private const int MaxMinutesPerAdvance = 60 * 24 * 400; // guard against runaway skips
        private float _minuteAccumulator;

        public GameClock(GameTime start, float minutesPerRealSecond = 2f)
        {
            Now = start;
            MinutesPerRealSecond = minutesPerRealSecond;
            TimeScale = 1f;
        }

        public GameTime Now { get; private set; }
        public float TimeScale { get; set; }
        public bool IsPaused { get; set; }
        public float MinutesPerRealSecond { get; set; }

        public event Action<GameTime> MinuteChanged;
        public event Action<GameTime> HourChanged;
        public event Action<GameTime> DayChanged;

        public void Tick(float realDeltaSeconds)
        {
            if (IsPaused || realDeltaSeconds <= 0f) return;

            _minuteAccumulator += realDeltaSeconds * MinutesPerRealSecond * TimeScale;
            if (_minuteAccumulator < 1f) return;

            int wholeMinutes = (int)_minuteAccumulator;
            _minuteAccumulator -= wholeMinutes;
            Advance(wholeMinutes);
        }

        public void Advance(int gameMinutes)
        {
            if (gameMinutes <= 0) return;
            if (gameMinutes > MaxMinutesPerAdvance) gameMinutes = MaxMinutesPerAdvance;

            for (int i = 0; i < gameMinutes; i++)
            {
                Now = Now.AddMinutes(1);
                MinuteChanged?.Invoke(Now);
                if (Now.Minute == 0) HourChanged?.Invoke(Now);
                if (Now.Hour == 0 && Now.Minute == 0) DayChanged?.Invoke(Now);
            }
        }

        public void SetTime(GameTime time) => Now = time;
    }
}

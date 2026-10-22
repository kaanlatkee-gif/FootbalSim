using System;

namespace FootbalSim.Core.Time
{
    /// <summary>
    /// Single source of truth for in-game time. The Unity layer drives Tick() from Update;
    /// everything else reads Now and subscribes to the events.
    /// </summary>
    public interface IClock
    {
        GameTime Now { get; }

        /// <summary>Multiplier on the base rate (0.5 = slow, 2 = fast).</summary>
        float TimeScale { get; set; }

        /// <summary>When true, Tick() does nothing. Set while in menus, dialogue and minigames.</summary>
        bool IsPaused { get; set; }

        /// <summary>Base rate: in-game minutes per real second at TimeScale 1.</summary>
        float MinutesPerRealSecond { get; set; }

        /// <summary>Real-time step. Ignored while paused.</summary>
        void Tick(float realDeltaSeconds);

        /// <summary>Instant jump forward (sleeping, skipping a day). Raises events.</summary>
        void Advance(int gameMinutes);

        void SetTime(GameTime time);

        event Action<GameTime> MinuteChanged;
        event Action<GameTime> HourChanged;
        event Action<GameTime> DayChanged;
    }
}

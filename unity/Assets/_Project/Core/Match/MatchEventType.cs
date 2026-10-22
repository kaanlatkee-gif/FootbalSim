namespace FootbalSim.Core.Match
{
    /// <summary>GDD §4.5 — every event that moves the live match rating.</summary>
    public enum MatchEventType
    {
        Goal,
        Assist,
        KeyPass,
        ShotOnTarget,
        SuccessfulDribble,
        TackleWon,
        PossessionLost,
        FailedPassOwnThird,
        BigMiss,
        Foul,
        YellowCard,
        RedCard,
        OwnGoal
    }

    public enum MatchOutcome
    {
        Loss = 0,
        Draw = 1,
        Win = 2
    }
}

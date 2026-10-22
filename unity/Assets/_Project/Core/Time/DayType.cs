namespace FootbalSim.Core.Time
{
    public enum DayType
    {
        /// <summary>No club activity: flat and market only.</summary>
        Off = 0,
        Gym = 1,
        Facility = 2,
        Match = 3
    }

    public static class BuildingIds
    {
        public const string Flat = "building.flat";
        public const string Market = "building.market";
        public const string Bar = "building.bar";
        public const string Gym = "building.gym";
        public const string Facility = "building.facility";
        public const string Gallery = "building.gallery";
        public const string Stadium = "building.stadium";
    }

    /// <summary>GDD §5.1 — which doors are unlocked on which kind of day.</summary>
    public static class BuildingAccess
    {
        public static bool IsOpen(string buildingId, DayType day, int hour)
        {
            // The flat is always home.
            if (buildingId == BuildingIds.Flat) return true;

            switch (day)
            {
                case DayType.Off:
                    return buildingId == BuildingIds.Market;

                case DayType.Gym:
                    return buildingId == BuildingIds.Market
                        || buildingId == BuildingIds.Gym
                        || buildingId == BuildingIds.Bar;

                case DayType.Facility:
                    return buildingId == BuildingIds.Market
                        || buildingId == BuildingIds.Facility
                        || buildingId == BuildingIds.Bar;

                case DayType.Match:
                    return buildingId == BuildingIds.Market
                        || buildingId == BuildingIds.Stadium
                        || buildingId == BuildingIds.Bar;

                default:
                    return false;
            }
        }

        public static string ClosedReason(DayType day) =>
            day == DayType.Off ? "Closed today" : "Nothing scheduled here today";
    }
}

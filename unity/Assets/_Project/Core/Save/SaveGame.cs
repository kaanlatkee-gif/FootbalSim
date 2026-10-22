using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Save
{
    /// <summary>
    /// The complete persisted state. Plain POCOs only: no tuples, no derived values,
    /// so System.Text.Json round-trips it without converters (ARCHITECTURE §9).
    /// </summary>
    [Serializable]
    public sealed class SaveGame
    {
        public const int CurrentSchemaVersion = 2;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CareerName = "New Career";
        public int PlaytimeSeconds;
        public long CreatedUtcTicks;
        public long SavedUtcTicks;

        public TimeSave Time = new TimeSave();
        public string FlatId = "flat.cheap";
        public string ClubId = "club.starter";
        public string LeagueId = "league.tr.1";
        public string CountryId = "TR";
        public string PositionKey = "ST";

        public Dictionary<string, float> Needs = new Dictionary<string, float>();
        public Dictionary<string, AttributeSave> Attributes = new Dictionary<string, AttributeSave>();
        public Dictionary<string, int> Inventory = new Dictionary<string, int>();
        public Dictionary<string, float> Relationships = new Dictionary<string, float>();

        public int CareerXp;
        public int Balance = 3000;
        public int Reputation = 20;
        public int Age = 19;

        public ContractSave Contract;
        public List<string> OwnedCarIds = new List<string>();
        public List<string> WorldFlags = new List<string>();
        public List<string> ClearedTutorials = new List<string>();
        public List<NewsSave> NewsLog = new List<NewsSave>();
        public List<FixtureSave> Fixtures = new List<FixtureSave>();
        public List<TransferOfferSave> PendingOffers = new List<TransferOfferSave>();

        /// <summary>key = "weekIndex:dayType", value = sessions used.</summary>
        public Dictionary<string, int> TrainingSessions = new Dictionary<string, int>();

        /// <summary>key = drill id, value = absolute day numbers on which it was trained.</summary>
        public Dictionary<string, List<int>> DrillDays = new Dictionary<string, List<int>>();

        public bool IsCompatible => SchemaVersion >= 1 && SchemaVersion <= CurrentSchemaVersion;
    }

    [Serializable]
    public sealed class TimeSave
    {
        public int Day;
        public int Hour = 7;
        public int Minute;
    }

    [Serializable]
    public sealed class AttributeSave
    {
        public int Value;
        public int Xp;
    }

    [Serializable]
    public sealed class ContractSave
    {
        public string ClubId;
        public int WeeklyWage;
        public int YearsRemaining;
        public int GoalBonus;
        public int AssistBonus;
        public int SigningBonus;
    }

    [Serializable]
    public sealed class NewsSave
    {
        public int Day;
        public string Tag;
        public string Line;
    }

    [Serializable]
    public sealed class FixtureSave
    {
        public int Round;
        public string HomeId;
        public string AwayId;
        public int HomeGoals;
        public int AwayGoals;
        public bool Played;
    }

    [Serializable]
    public sealed class TransferOfferSave
    {
        public string ClubId;
        public int Wage;
        public int Years;
        public int Fee;
        public int SigningBonus;
        public int Origin;
    }

    /// <summary>Upgrades older saves. Every schema bump adds a step here plus a test.</summary>
    public static class SaveMigration
    {
        public static SaveGame Migrate(SaveGame save)
        {
            if (save == null) return null;
            if (save.SchemaVersion == CurrentVersion) return save;

            // v1 -> v2: career XP and level were introduced. Older saves start at level 1.
            if (save.SchemaVersion < 2)
            {
                save.CareerXp = 0;
                save.SchemaVersion = 2;
            }

            return save;
        }

        public const int CurrentVersion = SaveGame.CurrentSchemaVersion;
    }
}

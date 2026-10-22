using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Attributes
{
    /// <summary>GDD §3.1 — 13 trainable attributes. Order is part of the save format: never reorder.</summary>
    public enum AttributeId
    {
        Finishing = 0,
        Dribbling = 1,
        Heading = 2,
        Crossing = 3,
        Passing = 4,
        Tackling = 5,
        LongShots = 6,
        SprintSpeed = 7,
        Stamina = 8,
        Balance = 9,
        Strength = 10,
        Jumping = 11,
        Acceleration = 12
    }

    public enum AttributeGroup
    {
        Technical = 0,
        Physical = 1
    }

    public sealed class AttributeDefinition
    {
        public AttributeDefinition(AttributeId id, string key, string displayName, AttributeGroup group)
        {
            Id = id;
            Key = key;
            DisplayName = displayName;
            Group = group;
        }

        public AttributeId Id { get; }
        /// <summary>Stable save key, e.g. "attr.finishing". Never change once shipped.</summary>
        public string Key { get; }
        public string DisplayName { get; }
        public AttributeGroup Group { get; }
    }

    public static class AttributeCatalog
    {
        public const int Count = 13;
        public const int MinValue = 1;
        public const int MaxValue = 99;

        private static readonly AttributeDefinition[] Definitions =
        {
            new AttributeDefinition(AttributeId.Finishing,    "attr.finishing",    "Finishing",    AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.Dribbling,    "attr.dribbling",    "Dribbling",    AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.Heading,      "attr.heading",      "Heading",      AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.Crossing,     "attr.crossing",     "Crossing",     AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.Passing,      "attr.passing",      "Passing",      AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.Tackling,     "attr.tackling",     "Tackling",     AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.LongShots,    "attr.long_shots",   "Long shots",   AttributeGroup.Technical),
            new AttributeDefinition(AttributeId.SprintSpeed,  "attr.sprint_speed", "Sprint speed", AttributeGroup.Physical),
            new AttributeDefinition(AttributeId.Stamina,      "attr.stamina",      "Stamina",      AttributeGroup.Physical),
            new AttributeDefinition(AttributeId.Balance,      "attr.balance",      "Balance",      AttributeGroup.Physical),
            new AttributeDefinition(AttributeId.Strength,     "attr.strength",     "Strength",     AttributeGroup.Physical),
            new AttributeDefinition(AttributeId.Jumping,      "attr.jumping",      "Jumping",      AttributeGroup.Physical),
            new AttributeDefinition(AttributeId.Acceleration, "attr.acceleration", "Acceleration", AttributeGroup.Physical)
        };

        public static IReadOnlyList<AttributeDefinition> All => Definitions;

        public static AttributeDefinition Get(AttributeId id) => Definitions[(int)id];

        public static string KeyOf(AttributeId id) => Definitions[(int)id].Key;

        public static bool TryParseKey(string key, out AttributeId id)
        {
            for (int i = 0; i < Definitions.Length; i++)
            {
                if (string.Equals(Definitions[i].Key, key, StringComparison.Ordinal))
                {
                    id = Definitions[i].Id;
                    return true;
                }
            }
            id = AttributeId.Finishing;
            return false;
        }

        public static IEnumerable<AttributeId> InGroup(AttributeGroup group)
        {
            for (int i = 0; i < Definitions.Length; i++)
                if (Definitions[i].Group == group) yield return Definitions[i].Id;
        }
    }
}

using FootbalSim.Core.Attributes;
using FootbalSim.Core.Progression;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class AttributeCatalogTests
    {
        [Test]
        public void Catalog_HasThirteenUniqueKeys()
        {
            var keys = new System.Collections.Generic.HashSet<string>();
            foreach (AttributeDefinition definition in AttributeCatalog.All)
                Assert.IsTrue(keys.Add(definition.Key), "Duplicate key: " + definition.Key);

            Assert.AreEqual(13, keys.Count);
        }

        [Test]
        public void Catalog_RoundTripsKeys()
        {
            foreach (AttributeDefinition definition in AttributeCatalog.All)
            {
                Assert.IsTrue(AttributeCatalog.TryParseKey(definition.Key, out AttributeId parsed));
                Assert.AreEqual(definition.Id, parsed);
            }
        }
    }

    [TestFixture]
    public class AttributeSetTests
    {
        [Test]
        public void Values_ClampToValidRange()
        {
            var set = new AttributeSet(50);
            set.Set(AttributeId.Finishing, 500);
            set.Set(AttributeId.Passing, -10);

            Assert.AreEqual(99, set.Get(AttributeId.Finishing));
            Assert.AreEqual(1, set.Get(AttributeId.Passing));
        }

        [Test]
        public void Overall_IsWeightedAverageOfProfile()
        {
            var set = new AttributeSet(50);
            set.Set(AttributeId.Finishing, 90);
            set.Set(AttributeId.Passing, 60);

            int overall = set.Overall(PositionProfile.Striker());
            Assert.Greater(overall, 50);
            Assert.Less(overall, 90);
        }

        [Test]
        public void GroupAverage_SplitsTechnicalAndPhysical()
        {
            var set = new AttributeSet(40);
            foreach (AttributeId id in AttributeCatalog.InGroup(AttributeGroup.Technical))
                set.Set(id, 70);

            Assert.AreEqual(70f, set.Average(AttributeGroup.Technical), 0.001f);
            Assert.AreEqual(40f, set.Average(AttributeGroup.Physical), 0.001f);
        }

        [Test]
        public void Changed_OnlyFiresOnRealChanges()
        {
            var set = new AttributeSet(40);
            int fired = 0;
            set.Changed += (_, __) => fired++;

            set.Set(AttributeId.Heading, 40);
            set.Set(AttributeId.Heading, 41);

            Assert.AreEqual(1, fired);
        }
    }

    [TestFixture]
    public class AttributeProgressionTests
    {
        [Test]
        public void ApplyXp_FlowsIntoAttributeSet()
        {
            var progression = new AttributeProgression(65);
            progression.ApplyXp(AttributeId.Finishing, 1000);

            Assert.AreEqual(66, progression.ValueOf(AttributeId.Finishing));
            Assert.AreEqual(66, progression.ToAttributeSet().Get(AttributeId.Finishing));
        }

        [Test]
        public void SaveRoundtrip_PreservesValueAndXp()
        {
            var progression = new AttributeProgression(65);
            progression.ApplyXp(AttributeId.Dribbling, 1100);

            var saved = progression.ToSaveDictionary();

            var reloaded = new AttributeProgression(40);
            reloaded.Load(saved);

            Assert.AreEqual(66, reloaded.ValueOf(AttributeId.Dribbling));
            Assert.AreEqual(100, reloaded.Pool(AttributeId.Dribbling).Xp);
        }
    }
}

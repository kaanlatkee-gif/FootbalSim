using System.Collections.Generic;
using FootbalSim.Core.Common;
using FootbalSim.Core.News;
using FootbalSim.Core.Save;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class GameMathTests
    {
        [Test]
        public void Clamp_RespectsBounds()
        {
            Assert.AreEqual(0f, GameMath.Clamp(-5f, 0f, 10f));
            Assert.AreEqual(10f, GameMath.Clamp(50f, 0f, 10f));
            Assert.AreEqual(5f, GameMath.Clamp(5f, 0f, 10f));
        }

        [Test]
        public void Lerp_IsClamped()
        {
            Assert.AreEqual(0f, GameMath.Lerp(0f, 10f, -1f));
            Assert.AreEqual(10f, GameMath.Lerp(0f, 10f, 2f));
            Assert.AreEqual(5f, GameMath.Lerp(0f, 10f, 0.5f));
        }

        [Test]
        public void InverseLerp_HandlesZeroSpan()
        {
            Assert.AreEqual(0f, GameMath.InverseLerp(5f, 5f, 5f));
        }
    }

    [TestFixture]
    public class RandomSourceTests
    {
        [Test]
        public void SameSeed_GivesSameSequence()
        {
            var a = new SplitMixRandom(42);
            var b = new SplitMixRandom(42);

            for (int i = 0; i < 50; i++)
                Assert.AreEqual(a.NextFloat(), b.NextFloat(), 0.0000001f);
        }

        [Test]
        public void ZeroSeed_IsAllowed()
        {
            var rng = new SplitMixRandom(0);
            float value = rng.NextFloat();
            Assert.GreaterOrEqual(value, 0f);
            Assert.Less(value, 1f);
        }

        [Test]
        public void NextInt_StaysInRange()
        {
            var rng = new SplitMixRandom(7);
            for (int i = 0; i < 1000; i++)
            {
                int value = rng.NextInt(3, 9);
                Assert.GreaterOrEqual(value, 3);
                Assert.Less(value, 9);
            }
        }

        [Test]
        public void Reseed_RestartsSequence()
        {
            var rng = new SplitMixRandom(1);
            float first = rng.NextFloat();
            rng.NextFloat();
            rng.Reseed(1);
            Assert.AreEqual(first, rng.NextFloat(), 0.0000001f);
        }
    }

    [TestFixture]
    public class EventBusTests
    {
        private struct Ping { public int Value; }

        [Test]
        public void Publish_ReachesSubscribers()
        {
            var bus = new EventBus();
            int received = 0;
            bus.Subscribe<Ping>(p => received = p.Value);

            bus.Publish(new Ping { Value = 7 });
            Assert.AreEqual(7, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            int count = 0;
            System.Action<Ping> handler = _ => count++;

            bus.Subscribe(handler);
            bus.Publish(new Ping());
            bus.Unsubscribe(handler);
            bus.Publish(new Ping());

            Assert.AreEqual(1, count);
        }

        [Test]
        public void PublishingWithNoSubscribers_IsSafe()
        {
            var bus = new EventBus();
            Assert.DoesNotThrow(() => bus.Publish(new Ping()));
        }
    }

    [TestFixture]
    public class NewsComposerTests
    {
        [Test]
        public void HighRating_ProducesALineAboutThePlayer()
        {
            var composer = new NewsComposer();
            var lines = composer.Compose(new[]
            {
                new NewsEvent("match.rating.high", 2, "Ada Yılmaz", "Ankara FC")
            });

            Assert.AreEqual(1, lines.Count);
            Assert.IsTrue(lines[0].Contains("Ada Yılmaz"));
        }

        [Test]
        public void SameTagAppearsOnce()
        {
            var composer = new NewsComposer();
            var lines = composer.Compose(new[]
            {
                new NewsEvent("match.rating.high", 2, "Ada"),
                new NewsEvent("match.rating.high", 1, "Ada")
            });

            Assert.AreEqual(1, lines.Count);
        }

        [Test]
        public void HigherSeverityComesFirst()
        {
            var composer = new NewsComposer();
            var lines = composer.Compose(new[]
            {
                new NewsEvent("purchase.car", 1, "Ada"),
                new NewsEvent("bar.binge", 2, "Ada")
            });

            Assert.AreEqual(2, lines.Count);
            Assert.IsTrue(lines[0].Contains("leaving the bar") || lines[0].Contains("long night"));
        }

        [Test]
        public void MaxLinesIsRespected()
        {
            var composer = new NewsComposer();
            var lines = composer.Compose(new[]
            {
                new NewsEvent("purchase.car", 1, "Ada"),
                new NewsEvent("bar.binge", 2, "Ada"),
                new NewsEvent("club.missed", 1, "Ada", "Ankara FC")
            }, maxLines: 2);

            Assert.AreEqual(2, lines.Count);
        }

        [Test]
        public void UnknownTag_IsIgnored()
        {
            var composer = new NewsComposer();
            var lines = composer.Compose(new[] { new NewsEvent("nonexistent.tag", 2, "Ada") });
            Assert.AreEqual(0, lines.Count);
        }

        [Test]
        public void ZeroSeverity_IsFillerAndDropped()
        {
            var composer = new NewsComposer();
            var lines = composer.Compose(new[] { new NewsEvent("bar.binge", 0, "Ada") });
            Assert.AreEqual(0, lines.Count);
        }
    }

    [TestFixture]
    public class SaveTests
    {
        [Test]
        public void NewSave_HasCurrentSchema()
        {
            var save = new SaveGame();
            Assert.AreEqual(SaveGame.CurrentSchemaVersion, save.SchemaVersion);
            Assert.IsTrue(save.IsCompatible);
        }

        [Test]
        public void Migration_UpgradesOldSchema()
        {
            var save = new SaveGame { SchemaVersion = 1, CareerXp = 999 };
            SaveGame migrated = SaveMigration.Migrate(save);

            Assert.AreEqual(SaveGame.CurrentSchemaVersion, migrated.SchemaVersion);
            Assert.AreEqual(0, migrated.CareerXp);
        }

        [Test]
        public void Migration_LeavesCurrentSavesAlone()
        {
            var save = new SaveGame { CareerXp = 4321 };
            SaveGame result = SaveMigration.Migrate(save);

            Assert.AreSame(save, result);
            Assert.AreEqual(4321, result.CareerXp);
        }
    }
}

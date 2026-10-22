using System.Collections.Generic;
using FootbalSim.Core.Attributes;
using FootbalSim.Core.Common;
using FootbalSim.Core.Progression;
using FootbalSim.Core.Time;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class XpCurveTests
    {
        [Test]
        public void CostToNext_MatchesTheSourceNote()
        {
            Assert.AreEqual(1000, XpCurve.CostToNext(65));
            Assert.AreEqual(1250, XpCurve.CostToNext(66));
            Assert.AreEqual(1500, XpCurve.CostToNext(67));
        }

        [Test]
        public void CostToNext_FlattensBelowSixtyThree()
        {
            Assert.AreEqual(750, XpCurve.CostToNext(64));
            Assert.AreEqual(500, XpCurve.CostToNext(63));
            Assert.AreEqual(250, XpCurve.CostToNext(62));
            Assert.AreEqual(250, XpCurve.CostToNext(40));
        }

        [Test]
        public void CostToNext_IsZeroAtMax()
        {
            Assert.AreEqual(0, XpCurve.CostToNext(AttributeCatalog.MaxValue));
        }
    }

    [TestFixture]
    public class AttributePoolTests
    {
        [Test]
        public void AddXp_RaisesOnePointAtExactlyTheThreshold()
        {
            var pool = new AttributePool(AttributeId.Finishing, 65);
            AttributeGain gain = pool.AddXp(1000);

            Assert.AreEqual(1, gain.PointsGained);
            Assert.AreEqual(66, pool.Value);
            Assert.AreEqual(0, pool.Xp);
        }

        [Test]
        public void AddXp_KeepsRemainder()
        {
            var pool = new AttributePool(AttributeId.Finishing, 65);
            AttributeGain gain = pool.AddXp(1100);

            Assert.AreEqual(1, gain.PointsGained);
            Assert.AreEqual(66, pool.Value);
            Assert.AreEqual(100, pool.Xp);
        }

        [Test]
        public void AddXp_CanGrantMultiplePoints()
        {
            var pool = new AttributePool(AttributeId.Finishing, 62);
            pool.AddXp(250 + 250 + 500 + 750); // 62->63, 63->64, 64->65
            Assert.AreEqual(65, pool.Value);
        }

        [Test]
        public void AddXp_StopsAtMax()
        {
            var pool = new AttributePool(AttributeId.Finishing, 98);
            pool.AddXp(1_000_000);

            Assert.AreEqual(99, pool.Value);
            Assert.AreEqual(0, pool.Xp);
            Assert.IsTrue(pool.IsMaxed);
            Assert.AreEqual(1f, pool.Progress);
        }

        [Test]
        public void Progress_IsFractionOfNextPoint()
        {
            var pool = new AttributePool(AttributeId.Passing, 70);
            pool.AddXp(1125); // cost at 70 is 2250
            Assert.AreEqual(0.5f, pool.Progress, 0.001f);
        }
    }

    [TestFixture]
    public class LevelCurveTests
    {
        [Test]
        public void Thresholds_MatchTheDesign()
        {
            Assert.AreEqual(0, LevelCurve.TotalXpForLevel(1));
            Assert.AreEqual(1500, LevelCurve.TotalXpForLevel(2));
            Assert.AreEqual(5000, LevelCurve.TotalXpForLevel(3));
            Assert.AreEqual(10500, LevelCurve.TotalXpForLevel(4));
        }

        [Test]
        public void LevelForTotalXp_IsInclusiveAtTheThreshold()
        {
            Assert.AreEqual(1, LevelCurve.LevelForTotalXp(1499));
            Assert.AreEqual(2, LevelCurve.LevelForTotalXp(1500));
            Assert.AreEqual(3, LevelCurve.LevelForTotalXp(5000));
        }

        [Test]
        public void TrainingMultiplier_AddsFourPercentPerLevel()
        {
            Assert.AreEqual(1.00f, LevelCurve.TrainingMultiplier(1), 0.0001f);
            Assert.AreEqual(1.04f, LevelCurve.TrainingMultiplier(2), 0.0001f);
            Assert.AreEqual(1.08f, LevelCurve.TrainingMultiplier(3), 0.0001f);
            Assert.AreEqual(2.00f, LevelCurve.TrainingMultiplier(26), 0.0001f);
            Assert.AreEqual(2.00f, LevelCurve.TrainingMultiplier(99), 0.0001f);
        }

        [Test]
        public void PlayerLevel_RaisesLevelUpEventOnce()
        {
            var level = new PlayerLevel();
            int events = 0;
            level.LeveledUp += _ => events++;

            level.AddXp(1400);
            Assert.AreEqual(1, level.Level);

            level.AddXp(200);
            Assert.AreEqual(2, level.Level);
            Assert.AreEqual(1, events);
        }
    }

    [TestFixture]
    public class TrainingRulesTests
    {
        [Test]
        public void RepeatMultiplier_MatchesTheSourceExample()
        {
            // "first time 250, second time 255"
            Assert.AreEqual(1f, TrainingRules.RepeatMultiplier(0), 0.0001f);
            Assert.AreEqual(1.02f, TrainingRules.RepeatMultiplier(1), 0.0001f);

            int first = TrainingRules.AwardXp(0.99f, TrainingRules.RepeatMultiplier(0));
            int second = TrainingRules.AwardXp(0.99f, TrainingRules.RepeatMultiplier(1));

            Assert.AreEqual(250, first);
            Assert.AreEqual(255, second);
        }

        [Test]
        public void RepeatMultiplier_IsCapped()
        {
            Assert.AreEqual(TrainingRules.RepeatBonusCap, TrainingRules.RepeatMultiplier(40), 0.0001f);
        }

        [Test]
        public void XpTiers_UseTheFiveBands()
        {
            Assert.AreEqual(50, XpTier.FromScore(0.10f));
            Assert.AreEqual(100, XpTier.FromScore(0.45f));
            Assert.AreEqual(150, XpTier.FromScore(0.70f));
            Assert.AreEqual(200, XpTier.FromScore(0.90f));
            Assert.AreEqual(250, XpTier.FromScore(0.99f));
        }

        [Test]
        public void Session_LimitsSlotsAndDuplicates()
        {
            var session = new TrainingSession();

            Assert.IsTrue(session.TryAdd("drill.finishing").IsSuccess);
            Assert.IsTrue(session.TryAdd("drill.finishing").IsSuccess);
            Assert.IsFalse(session.TryAdd("drill.finishing").IsSuccess); // third duplicate
            Assert.IsTrue(session.TryAdd("drill.passing").IsSuccess);
            Assert.IsTrue(session.IsComplete);
            Assert.IsFalse(session.TryAdd("drill.heading").IsSuccess);   // fourth slot
        }
    }

    [TestFixture]
    public class TrainingTrackerTests
    {
        [Test]
        public void Sessions_AreLimitedPerTypePerWeek()
        {
            var tracker = new TrainingTracker();

            Assert.IsTrue(tracker.TryStartSession(0, DayType.Gym).IsSuccess);
            Assert.IsTrue(tracker.TryStartSession(0, DayType.Gym).IsSuccess);
            Assert.IsFalse(tracker.TryStartSession(0, DayType.Gym).IsSuccess);
            Assert.IsTrue(tracker.TryStartSession(0, DayType.Facility).IsSuccess);
        }

        [Test]
        public void Sessions_ResetInTheNextWeek()
        {
            var tracker = new TrainingTracker();
            tracker.TryStartSession(0, DayType.Gym);
            tracker.TryStartSession(0, DayType.Gym);

            Assert.IsTrue(tracker.TryStartSession(1, DayType.Gym).IsSuccess);
        }

        [Test]
        public void RepeatBonus_CountsDistinctEarlierDaysOnly()
        {
            var tracker = new TrainingTracker();
            const string drill = "drill.finishing";

            tracker.RecordDrill(drill, 0);
            tracker.RecordDrill(drill, 0); // same day, must not stack
            Assert.AreEqual(1, tracker.PriorDistinctDays(drill, 1));
            Assert.AreEqual(1.02f, tracker.RepeatMultiplierFor(drill, 1), 0.0001f);

            tracker.RecordDrill(drill, 1);
            Assert.AreEqual(1.0404f, tracker.RepeatMultiplierFor(drill, 2), 0.0001f);
        }

        [Test]
        public void Sessions_CannotStartOnOffDays()
        {
            var tracker = new TrainingTracker();
            Assert.IsFalse(tracker.TryStartSession(0, DayType.Off).IsSuccess);
        }
    }
}

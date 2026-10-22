using System.Collections.Generic;
using System.Linq;
using FootbalSim.Core.Career;
using FootbalSim.Core.Common;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class TransferProbabilityTests
    {
        [Test]
        public void MatchedValueAndWage_GivesFiftyFivePercent()
        {
            float p = TransferProbability.Accept(25000, 25000, 5000, 5000);
            Assert.AreEqual(0.55f, p, 0.001f);
        }

        [Test]
        public void DemandingMoreWage_LowersTheChance()
        {
            float modest = TransferProbability.Accept(25000, 25000, 5000, 5000);
            float greedy = TransferProbability.Accept(25000, 25000, 5000, 7500);

            Assert.Less(greedy, modest);
            Assert.AreEqual(0.4125f, greedy, 0.001f); // 0.55 * 0.75
        }

        [Test]
        public void AbsurdWageDemand_IsClampedNotNegative()
        {
            float p = TransferProbability.Accept(25000, 25000, 5000, 50000);
            Assert.Greater(p, 0f);
            Assert.LessOrEqual(p, TransferProbability.MinAccept + 0.01f);
        }

        [Test]
        public void BiggerClub_IsHarderToJoin()
        {
            float easier = TransferProbability.Accept(25000, 15000, 5000, 5000);
            float harder = TransferProbability.Accept(5000, 25000, 5000, 5000);

            Assert.Greater(easier, harder);
        }
    }

    [TestFixture]
    public class TransferResolverTests
    {
        [Test]
        public void RejectedSelfOffer_CostsFansAndCoach()
        {
            var outcome = TransferResolver.Resolve(0.01f, OfferOrigin.PlayerToClub, new SplitMixRandom(1));

            Assert.IsFalse(outcome.Accepted);
            Assert.AreEqual(TransferResolver.RejectedFansPenalty, outcome.FansDelta);
            Assert.AreEqual(TransferResolver.RejectedCoachPenalty, outcome.CoachDelta);
        }

        [Test]
        public void RejectedClubOffer_HasNoPenalty()
        {
            var outcome = TransferResolver.Resolve(0.01f, OfferOrigin.ClubToPlayer, new SplitMixRandom(1));

            Assert.IsFalse(outcome.Accepted);
            Assert.AreEqual(0, outcome.FansDelta);
            Assert.AreEqual(0, outcome.CoachDelta);
        }

        [Test]
        public void CertainProbability_AlwaysAccepts()
        {
            var outcome = TransferResolver.Resolve(1f, OfferOrigin.PlayerToClub, new SplitMixRandom(7));
            Assert.IsTrue(outcome.Accepted);
        }
    }

    [TestFixture]
    public class MarketValueTests
    {
        [Test]
        public void PeakAgeStrongYoungPlayer_IsWorthMillions()
        {
            int value = MarketValueCalculator.Compute(78, 24, 7.2f, 1);
            Assert.That(value, Is.InRange(25000, 30000));
        }

        [Test]
        public void AgeAndTier_ReduceValue()
        {
            int young = MarketValueCalculator.Compute(78, 22, 7.0f, 1);
            int old = MarketValueCalculator.Compute(78, 35, 7.0f, 1);
            int lowerLeague = MarketValueCalculator.Compute(78, 22, 7.0f, 3);

            Assert.Greater(young, old);
            Assert.Greater(young, lowerLeague);
        }

        [Test]
        public void BetterForm_RaisesValue()
        {
            int good = MarketValueCalculator.Compute(75, 25, 8.5f, 1);
            int poor = MarketValueCalculator.Compute(75, 25, 5.5f, 1);
            Assert.Greater(good, poor);
        }

        [Test]
        public void WageFollowsValue()
        {
            Assert.AreEqual(500, MarketValueCalculator.WeeklyWageFor(100));
            Assert.AreEqual(40000, MarketValueCalculator.WeeklyWageFor(20000));
        }
    }

    [TestFixture]
    public class FixtureGeneratorTests
    {
        [Test]
        public void DoubleRoundRobin_EveryTeamPlaysTwiceAgainstEveryOther()
        {
            var clubs = new[] { "a", "b", "c", "d" };
            List<Fixture> fixtures = FixtureGenerator.RoundRobin(clubs, doubleRound: true);

            Assert.AreEqual(12, fixtures.Count);
            foreach (string club in clubs)
            {
                int played = fixtures.Count(f => f.HomeId == club || f.AwayId == club);
                Assert.AreEqual(6, played, club + " should play 6 matches");
            }
        }

        [Test]
        public void NoTeamPlaysItself()
        {
            List<Fixture> fixtures = FixtureGenerator.RoundRobin(new[] { "a", "b", "c", "d", "e" }, true);
            Assert.IsFalse(fixtures.Any(f => f.HomeId == f.AwayId));
        }

        [Test]
        public void OddTeamCount_HandlesTheBye()
        {
            List<Fixture> fixtures = FixtureGenerator.RoundRobin(new[] { "a", "b", "c" }, doubleRound: false);
            Assert.AreEqual(3, fixtures.Count);
        }
    }

    [TestFixture]
    public class LeagueTableTests
    {
        [Test]
        public void WinGivesThreePoints()
        {
            var table = new LeagueTable(new[] { "a", "b" });
            table.Apply("a", "b", 2, 0);

            Assert.AreEqual(3, table.GetOrCreate("a").Points);
            Assert.AreEqual(0, table.GetOrCreate("b").Points);
            Assert.AreEqual(1, table.PositionOf("a"));
            Assert.AreEqual(2, table.PositionOf("b"));
        }

        [Test]
        public void GoalDifferenceBreaksTies()
        {
            var table = new LeagueTable(new[] { "a", "b", "c" });
            table.Apply("a", "c", 1, 0);
            table.Apply("b", "c", 4, 0);

            Assert.AreEqual("b", table.Standings[0].ClubId);
            Assert.AreEqual("a", table.Standings[1].ClubId);
        }

        [Test]
        public void DrawGivesOnePointEach()
        {
            var table = new LeagueTable(new[] { "a", "b" });
            table.Apply("a", "b", 1, 1);

            Assert.AreEqual(1, table.GetOrCreate("a").Points);
            Assert.AreEqual(1, table.GetOrCreate("b").Points);
            Assert.AreEqual(0, table.GetOrCreate("a").GoalDifference);
        }
    }
}

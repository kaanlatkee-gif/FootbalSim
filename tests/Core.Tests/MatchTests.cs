using FootbalSim.Core.Common;
using FootbalSim.Core.Match;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class RatingTrackerTests
    {
        [Test]
        public void StartsAtSixPointFive()
        {
            Assert.AreEqual(6.5f, new RatingTracker().Rating, 0.001f);
        }

        [Test]
        public void GoalAndAssist_RaiseRating()
        {
            var tracker = new RatingTracker();
            tracker.Apply(MatchEventType.Goal);
            Assert.AreEqual(7.4f, tracker.Rating, 0.001f);

            tracker.Apply(MatchEventType.Assist);
            Assert.AreEqual(8.0f, tracker.Rating, 0.001f);
        }

        [Test]
        public void PossessionLost_LowersRating()
        {
            var tracker = new RatingTracker();
            tracker.Apply(MatchEventType.PossessionLost);
            Assert.AreEqual(6.38f, tracker.Rating, 0.001f);
        }

        [Test]
        public void Rating_IsClampedToTen()
        {
            var tracker = new RatingTracker();
            for (int i = 0; i < 20; i++) tracker.Apply(MatchEventType.Goal);
            Assert.AreEqual(10f, tracker.Rating, 0.001f);
        }

        [Test]
        public void Rating_IsClampedToThree()
        {
            var tracker = new RatingTracker();
            for (int i = 0; i < 20; i++) tracker.Apply(MatchEventType.RedCard);
            Assert.AreEqual(3f, tracker.Rating, 0.001f);
        }

        [Test]
        public void Reset_ReturnsToSixPointFive()
        {
            var tracker = new RatingTracker();
            tracker.Apply(MatchEventType.Goal);
            tracker.Reset();
            Assert.AreEqual(6.5f, tracker.Rating, 0.001f);
        }
    }

    [TestFixture]
    public class MatchXpTests
    {
        [Test]
        public void WinWithGoal_MatchesFormula()
        {
            var result = new MatchResult(7.5f, goals: 1, assists: 0, teamGoals: 2, opponentGoals: 0, playedByPlayer: true);
            // 120 * (7.5-5) = 300, win 120, goal 60
            Assert.AreEqual(480, MatchXp.For(result));
        }

        [Test]
        public void LossLowRating_EarnsNothing()
        {
            var result = new MatchResult(4.0f, 0, 0, 0, 2, true);
            Assert.AreEqual(0, MatchXp.For(result));
        }

        [Test]
        public void Outcome_IsDerivedFromScore()
        {
            Assert.AreEqual(MatchOutcome.Win, new MatchResult(7, 0, 0, 2, 1, true).Outcome);
            Assert.AreEqual(MatchOutcome.Draw, new MatchResult(7, 0, 0, 1, 1, true).Outcome);
            Assert.AreEqual(MatchOutcome.Loss, new MatchResult(7, 0, 0, 0, 1, true).Outcome);
        }
    }

    [TestFixture]
    public class InstantMatchSimulatorTests
    {
        [Test]
        public void SameSeed_ProducesSameResult()
        {
            var simulator = new InstantMatchSimulator();
            var input = new MatchSimInput(70, 72, 68, 68f);

            MatchResult a = simulator.Simulate(input, new SplitMixRandom(12345));
            MatchResult b = simulator.Simulate(input, new SplitMixRandom(12345));

            Assert.AreEqual(a.Rating, b.Rating, 0.0001f);
            Assert.AreEqual(a.Goals, b.Goals);
            Assert.AreEqual(a.Assists, b.Assists);
            Assert.AreEqual(a.TeamGoals, b.TeamGoals);
            Assert.AreEqual(a.OpponentGoals, b.OpponentGoals);
        }

        [Test]
        public void DifferentSeeds_Diverge()
        {
            var simulator = new InstantMatchSimulator();
            var input = new MatchSimInput(70, 72, 68, 68f);

            MatchResult a = simulator.Simulate(input, new SplitMixRandom(1));
            MatchResult b = simulator.Simulate(input, new SplitMixRandom(999));

            Assert.IsFalse(a.TeamGoals == b.TeamGoals && a.OpponentGoals == b.OpponentGoals && a.Goals == b.Goals);
        }

        [Test]
        public void PlayerGoals_NeverExceedTeamGoals()
        {
            var simulator = new InstantMatchSimulator();
            var input = new MatchSimInput(85, 90, 80, 50f);

            for (int seed = 1; seed < 200; seed++)
            {
                MatchResult result = simulator.Simulate(input, new SplitMixRandom(seed));
                Assert.LessOrEqual(result.Goals, result.TeamGoals);
                Assert.GreaterOrEqual(result.Rating, RatingTracker.MinRating);
                Assert.LessOrEqual(result.Rating, RatingTracker.MaxRating);
            }
        }

        [Test]
        public void StrongerTeam_ScoresMoreOnAverage()
        {
            var simulator = new InstantMatchSimulator();
            int strongGoals = 0, weakGoals = 0;

            for (int seed = 1; seed <= 300; seed++)
            {
                strongGoals += simulator.Simulate(new MatchSimInput(85, 85, 85, 55f), new SplitMixRandom(seed)).TeamGoals;
                weakGoals += simulator.Simulate(new MatchSimInput(55, 55, 55, 85f), new SplitMixRandom(seed)).TeamGoals;
            }

            Assert.Greater(strongGoals, weakGoals);
        }
    }
}

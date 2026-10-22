using System;
using FootbalSim.Core.Time;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class GameTimeTests
    {
        [Test]
        public void TotalMinutes_RollsOverCorrectly()
        {
            var time = new GameTime(1, 23, 59);
            Assert.AreEqual(1440 + 23 * 60 + 59, time.TotalMinutes);
        }

        [Test]
        public void AddMinutes_WrapsToNextDay()
        {
            var time = new GameTime(0, 23, 30).AddMinutes(45);
            Assert.AreEqual(1, time.Day);
            Assert.AreEqual(0, time.Hour);
            Assert.AreEqual(15, time.Minute);
        }

        [Test]
        public void DayOfWeek_StartsOnMonday()
        {
            Assert.AreEqual(0, new GameTime(0, 8, 0).DayOfWeek);
            Assert.AreEqual(6, new GameTime(6, 8, 0).DayOfWeek);
            Assert.AreEqual(0, new GameTime(7, 8, 0).DayOfWeek);
        }

        [Test]
        public void MinutesUntilHour_GoesToTomorrowWhenPast()
        {
            var time = new GameTime(0, 23, 0);
            Assert.AreEqual(60, time.MinutesUntilHour(0));
        }

        [Test]
        public void ComparisonOperators_Work()
        {
            Assert.IsTrue(new GameTime(0, 9, 0) < new GameTime(0, 10, 0));
            Assert.IsTrue(new GameTime(1, 0, 0) > new GameTime(0, 23, 59));
        }
    }

    [TestFixture]
    public class GameClockTests
    {
        [Test]
        public void Tick_AdvancesAtConfiguredRate()
        {
            var clock = new GameClock(new GameTime(0, 8, 0), minutesPerRealSecond: 2f);
            clock.Tick(30f); // 30 s * 2 min/s = 60 minutes
            Assert.AreEqual(9, clock.Now.Hour);
            Assert.AreEqual(0, clock.Now.Minute);
        }

        [Test]
        public void Tick_DoesNothingWhilePaused()
        {
            var clock = new GameClock(new GameTime(0, 8, 0));
            clock.IsPaused = true;
            clock.Tick(100f);
            Assert.AreEqual(new GameTime(0, 8, 0), clock.Now);
        }

        [Test]
        public void Advance_RaisesHourAndDayEvents()
        {
            var clock = new GameClock(new GameTime(0, 8, 30));
            int hours = 0, days = 0;
            clock.HourChanged += _ => hours++;
            clock.DayChanged += _ => days++;

            clock.Advance(16 * 60); // to 00:30 next day

            Assert.AreEqual(16, hours);
            Assert.AreEqual(1, days);
            Assert.AreEqual(1, clock.Now.Day);
            Assert.AreEqual(0, clock.Now.Hour);
        }

        [Test]
        public void Advance_IsDeterministic()
        {
            var a = new GameClock(new GameTime(0, 0, 0));
            var b = new GameClock(new GameTime(0, 0, 0));
            a.Advance(500);
            b.Advance(500);
            Assert.AreEqual(a.Now, b.Now);
        }
    }

    [TestFixture]
    public class ScheduleTests
    {
        [Test]
        public void RegularWeek_HasFiveActiveDays()
        {
            WeekSchedule week = WeekScheduleGenerator.Create(0, hasEuropeanFixture: false);

            Assert.AreEqual(5, week.ActiveDayCount);
            Assert.AreEqual(2, week.CountOf(DayType.Gym));
            Assert.AreEqual(2, week.CountOf(DayType.Facility));
            Assert.AreEqual(1, week.CountOf(DayType.Match));
            Assert.AreEqual(2, week.CountOf(DayType.Off));
        }

        [Test]
        public void EuropeanWeek_HasSixActiveDaysAndTwoMatches()
        {
            WeekSchedule week = WeekScheduleGenerator.Create(3, hasEuropeanFixture: true);

            Assert.AreEqual(6, week.ActiveDayCount);
            Assert.AreEqual(2, week.CountOf(DayType.Match));
            Assert.AreEqual(1, week.CountOf(DayType.Off));
        }

        [Test]
        public void OffDay_ClosesEverythingButMarketAndFlat()
        {
            Assert.IsTrue(BuildingAccess.IsOpen(BuildingIds.Market, DayType.Off, 12));
            Assert.IsTrue(BuildingAccess.IsOpen(BuildingIds.Flat, DayType.Off, 12));
            Assert.IsFalse(BuildingAccess.IsOpen(BuildingIds.Facility, DayType.Off, 12));
            Assert.IsFalse(BuildingAccess.IsOpen(BuildingIds.Gym, DayType.Off, 12));
            Assert.IsFalse(BuildingAccess.IsOpen(BuildingIds.Stadium, DayType.Off, 12));
        }

        [Test]
        public void GymDay_OpensGymButNotFacility()
        {
            Assert.IsTrue(BuildingAccess.IsOpen(BuildingIds.Gym, DayType.Gym, 10));
            Assert.IsFalse(BuildingAccess.IsOpen(BuildingIds.Facility, DayType.Gym, 10));
        }

        [Test]
        public void ScheduleService_SkipsOffDayToSevenAm()
        {
            // Day 2 is Wednesday, which the regular week makes an off day.
            var clock = new GameClock(new GameTime(2, 12, 0));
            var schedule = new ScheduleService(clock);

            Assert.AreEqual(DayType.Off, schedule.Today);

            bool skipped = schedule.TrySkipToNextActiveDay(out GameTime wake);

            Assert.IsTrue(skipped);
            Assert.AreEqual(3, wake.Day);
            Assert.AreEqual(ScheduleService.WakeHour, wake.Hour);
            Assert.AreEqual(0, wake.Minute);
        }

        [Test]
        public void ScheduleService_ReportsEuropeanWeeks()
        {
            var clock = new GameClock(new GameTime(8, 9, 0)); // week 1, Tuesday
            var schedule = new ScheduleService(clock, week => week == 1);

            Assert.IsTrue(schedule.CurrentWeek.HasEuropeanFixture);
            Assert.AreEqual(6, schedule.CurrentWeek.ActiveDayCount);
        }
    }
}

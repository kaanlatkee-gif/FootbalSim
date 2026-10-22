using FootbalSim.Core.Economy;
using FootbalSim.Core.Needs;
using FootbalSim.Core.Time;
using NUnit.Framework;

namespace FootbalSim.Core.Tests
{
    [TestFixture]
    public class NeedsModelTests
    {
        [Test]
        public void StartsFresh()
        {
            var needs = new NeedsModel();
            Assert.AreEqual(100f, needs.Get(NeedId.Energy), 0.001f);
            Assert.AreEqual(100f, needs.Get(NeedId.Hygiene), 0.001f);
            Assert.AreEqual(0f, needs.Get(NeedId.Bladder), 0.001f);
            Assert.AreEqual(0f, needs.Get(NeedId.Intoxication), 0.001f);
        }

        [Test]
        public void Tick_AppliesPerHourRates()
        {
            var needs = new NeedsModel();
            needs.Tick(60);

            Assert.AreEqual(98.5f, needs.Get(NeedId.Energy), 0.001f);
            Assert.AreEqual(98f, needs.Get(NeedId.Hygiene), 0.001f);
            Assert.AreEqual(6f, needs.Get(NeedId.Bladder), 0.001f);
        }

        [Test]
        public void Tick_IsAdditiveAcrossSmallSteps()
        {
            var many = new NeedsModel();
            for (int i = 0; i < 60; i++) many.Tick(1);

            var once = new NeedsModel();
            once.Tick(60);

            Assert.AreEqual(once.Get(NeedId.Energy), many.Get(NeedId.Energy), 0.01f);
        }

        [Test]
        public void Values_ClampAtBounds()
        {
            var needs = new NeedsModel();
            needs.Add(NeedId.Energy, -1000f);
            needs.Add(NeedId.Bladder, 1000f);

            Assert.AreEqual(0f, needs.Get(NeedId.Energy), 0.001f);
            Assert.AreEqual(100f, needs.Get(NeedId.Bladder), 0.001f);
        }

        [Test]
        public void ShowerCleansAndToiletEmpties()
        {
            var needs = new NeedsModel();
            needs.Set(NeedId.Hygiene, 20f);
            needs.Set(NeedId.Bladder, 100f);

            needs.Apply(ActivityCosts.Shower);
            needs.Apply(ActivityCosts.Toilet);

            Assert.AreEqual(100f, needs.Get(NeedId.Hygiene), 0.001f);
            Assert.AreEqual(0f, needs.Get(NeedId.Bladder), 0.001f);
        }

        [Test]
        public void PerformanceFactor_DropsWhenTiredFilthyOrDrunk()
        {
            var needs = new NeedsModel();
            Assert.AreEqual(1f, needs.PerformanceFactor, 0.001f);

            needs.Set(NeedId.Energy, 10f);
            Assert.AreEqual(0.6f, needs.PerformanceFactor, 0.001f);
        }

        [Test]
        public void DrinksRaiseIntoxicationAndBladder()
        {
            var needs = new NeedsModel();
            needs.Apply(ActivityCosts.Drink());

            Assert.AreEqual(12f, needs.Get(NeedId.Intoxication), 0.001f);
            Assert.AreEqual(12f, needs.Get(NeedId.Bladder), 0.001f);
            Assert.AreEqual(96f, needs.Get(NeedId.Energy), 0.001f);
        }

        [Test]
        public void SaveRoundtrip_KeepsValues()
        {
            var needs = new NeedsModel();
            needs.Set(NeedId.Energy, 42f);

            var reloaded = new NeedsModel();
            reloaded.Load(needs.ToSaveDictionary());

            Assert.AreEqual(42f, reloaded.Get(NeedId.Energy), 0.001f);
        }
    }

    [TestFixture]
    public class WalletTests
    {
        private static readonly GameTime Noon = new GameTime(0, 12, 0);

        [Test]
        public void StartsAtThreeThousand()
        {
            Assert.AreEqual(3000, new Wallet().Balance);
        }

        [Test]
        public void Spending_ReducesBalanceAndJournals()
        {
            var wallet = new Wallet();
            var result = wallet.TrySpend(500, Noon, "shop.groceries", "item.bread");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2500, wallet.Balance);
            Assert.AreEqual(1, wallet.Entries.Count);
            Assert.AreEqual(-500, wallet.Entries[0].Amount);
        }

        [Test]
        public void Spending_IsRefusedWithoutFunds()
        {
            var wallet = new Wallet(100);
            var result = wallet.TrySpend(500, Noon, "shop.groceries");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(100, wallet.Balance);
            Assert.AreEqual(0, wallet.Entries.Count);
        }

        [Test]
        public void Earning_AddsAndJournals()
        {
            var wallet = new Wallet();
            wallet.Earn(1500, Noon, "wage.appearance");
            Assert.AreEqual(4500, wallet.Balance);
        }

        [Test]
        public void Bankrupt_TriggersBelowFiveThousandDebt()
        {
            var wallet = new Wallet(-4999);
            Assert.IsFalse(wallet.IsBankrupt);
            wallet.Earn(1, Noon, "test");
            Assert.IsTrue(wallet.IsBankrupt == false);

            var broke = new Wallet(-6000);
            Assert.IsTrue(broke.IsBankrupt);
        }

        [Test]
        public void NetForDay_SumsOnlyThatDay()
        {
            var wallet = new Wallet();
            wallet.Earn(100, new GameTime(0, 9, 0), "a");
            wallet.Earn(200, new GameTime(1, 9, 0), "b");

            Assert.AreEqual(100, wallet.NetForDay(0));
        }
    }
}

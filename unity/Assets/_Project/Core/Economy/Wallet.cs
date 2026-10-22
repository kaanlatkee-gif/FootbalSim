using System;
using System.Collections.Generic;
using FootbalSim.Core.Common;
using FootbalSim.Core.Time;

namespace FootbalSim.Core.Economy
{
    public readonly struct LedgerEntry
    {
        public readonly GameTime When;
        public readonly int Amount;      // negative = spent
        public readonly string ReasonKey;
        public readonly string SourceId;

        public LedgerEntry(GameTime when, int amount, string reasonKey, string sourceId)
        {
            When = when;
            Amount = amount;
            ReasonKey = reasonKey;
            SourceId = sourceId;
        }
    }

    /// <summary>Every transaction is journalled; the journal feeds the news and the debug screen.</summary>
    public sealed class Wallet
    {
        public const int StartingBalance = 3000;
        public const int EvictionThreshold = -5000;

        private readonly List<LedgerEntry> _entries = new List<LedgerEntry>(64);

        public Wallet(int balance = StartingBalance) => Balance = balance;

        public int Balance { get; private set; }
        public IReadOnlyList<LedgerEntry> Entries => _entries;
        public bool IsBankrupt => Balance < EvictionThreshold;

        public event Action<int> BalanceChanged;

        public bool CanAfford(int amount) => amount <= Balance;

        public Result TrySpend(int amount, GameTime when, string reasonKey, string sourceId = null)
        {
            if (amount <= 0) return Result.Fail("Amount must be positive.");
            if (!CanAfford(amount)) return Result.Fail("Not enough money.");

            Apply(-amount, when, reasonKey, sourceId);
            return Result.Ok();
        }

        public void Earn(int amount, GameTime when, string reasonKey, string sourceId = null)
        {
            if (amount <= 0) return;
            Apply(amount, when, reasonKey, sourceId);
        }

        private void Apply(int amount, GameTime when, string reasonKey, string sourceId)
        {
            Balance += amount;
            _entries.Add(new LedgerEntry(when, amount, reasonKey, sourceId));
            BalanceChanged?.Invoke(Balance);
        }

        public IEnumerable<LedgerEntry> ForDay(int day)
        {
            for (int i = 0; i < _entries.Count; i++)
                if (_entries[i].When.Day == day) yield return _entries[i];
        }

        public int NetForDay(int day)
        {
            int net = 0;
            for (int i = 0; i < _entries.Count; i++)
                if (_entries[i].When.Day == day) net += _entries[i].Amount;
            return net;
        }
    }
}

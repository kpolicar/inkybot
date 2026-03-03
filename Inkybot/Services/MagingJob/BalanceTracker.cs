using System;
using Inkybot.Events;

namespace Inkybot.Services
{
    internal class BalanceTracker
    {
        private const int MaxReasonableBalanceDifference = 300000;

        private readonly MageSession session;
        private int totalSpent;

        public event EventHandler<BalanceChangedEventArgs> BalanceChanged;
        public event EventHandler<BalanceChangedEventArgs> BalanceSpent;

        public BalanceTracker(MageSession session) {
            this.session = session;
        }

        public void Update(int newValue) {
            var newBalance = Math.Max(newValue, 0);
            var previousBalance = session.Balance;
            var spent = Math.Max(0, previousBalance - newBalance);

            session.Balance = newBalance;
            BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(previousBalance, newBalance));

            if (spent <= MaxReasonableBalanceDifference) {
                var previousTotalSpent = totalSpent;
                totalSpent += spent;
                BalanceSpent?.Invoke(this, new BalanceChangedEventArgs(previousTotalSpent, totalSpent));
            }
        }

        public void Reset() {
            session.Balance = 0;
        }
    }
}

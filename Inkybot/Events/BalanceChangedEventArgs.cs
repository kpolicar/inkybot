using System;

namespace Inkybot.Events
{
    public class BalanceChangedEventArgs : EventArgs
    {
        public readonly int Balance;
        public readonly int OldBalance;


        public BalanceChangedEventArgs(int oldBalance, int balance) {
            this.Balance = balance;
            this.OldBalance = oldBalance;
        }
    }
}

/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: CheckingAccount is a DERIVED (child) class of Account. It adds a
 *          monthly maintenance fee and an overdraft limit. It does NOT earn
 *          interest, so it does not implement IInterestBearing.
 *          Week 2: overrides ProcessMonthEnd() to charge the monthly fee.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: CheckingAccount IS-AN Account.
    public class CheckingAccount : Account
    {
        public decimal MonthlyFee { get; set; }
        public decimal OverdraftLimit { get; set; }

        // ": base(...)" passes the shared values up to the Account constructor.
        public CheckingAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                               decimal monthlyFee, decimal overdraftLimit)
            : base(accountNumber, openingBalance, dateOpened)
        {
            AccountType = "Checking";
            MonthlyFee = monthlyFee;
            OverdraftLimit = overdraftLimit;
        }

        // POLYMORPHISM (override): checking accounts can go below zero,
        // up to the overdraft limit.
        public override void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.");
            }
            if (amount > Balance + OverdraftLimit)
            {
                throw new InvalidOperationException("Withdrawal exceeds the overdraft limit.");
            }
            Balance -= amount;
        }

        // POLYMORPHISM (override): checking-specific details.
        public override string GetAccountDetails()
        {
            return $"Fee {MonthlyFee:C}/mo, Overdraft {OverdraftLimit:C}";
        }

        // POLYMORPHISM (override): month-end for checking = charge the fee.
        public override string ProcessMonthEnd()
        {
            if (MonthlyFee == 0)
            {
                return "No monthly fee (fee waived)";
            }
            Balance -= MonthlyFee;
            return $"Charged monthly fee of {MonthlyFee:C}";
        }
    }
}

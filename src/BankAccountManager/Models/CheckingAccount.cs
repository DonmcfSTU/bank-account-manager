/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: CheckingAccount is a DERIVED (child) class of Account. It adds a
 *          monthly maintenance fee and an overdraft limit, which are things
 *          a savings account doesn't have.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: CheckingAccount IS-AN Account.
    // It gets AccountNumber, Balance, Deposit(), etc. from the base class.
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

        // Checking accounts can go below zero, up to the overdraft limit.
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

        public override string GetAccountDetails()
        {
            return $"Fee {MonthlyFee:C}/mo, Overdraft {OverdraftLimit:C}";
        }
    }
}

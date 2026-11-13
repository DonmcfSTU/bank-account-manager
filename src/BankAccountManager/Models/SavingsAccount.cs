/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: SavingsAccount is a DERIVED (child) class of Account. It adds an
 *          annual interest rate and a method that calculates the interest
 *          the account would earn in one month.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: SavingsAccount IS-AN Account.
    public class SavingsAccount : Account
    {
        // Stored as a decimal, so 0.0425 means 4.25% per year.
        public decimal InterestRate { get; set; }

        public SavingsAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                              decimal interestRate)
            : base(accountNumber, openingBalance, dateOpened)
        {
            AccountType = "Savings";
            InterestRate = interestRate;
        }

        // Estimated interest for one month based on the current balance.
        public decimal CalculateMonthlyInterest()
        {
            return Math.Round(Balance * InterestRate / 12, 2);
        }

        public override string GetAccountDetails()
        {
            return $"APY {InterestRate:P2}, ~{CalculateMonthlyInterest():C}/mo";
        }
    }
}

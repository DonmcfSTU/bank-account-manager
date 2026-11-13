/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: SavingsAccount is a DERIVED (child) class of Account. It adds an
 *          annual interest rate.
 *          Week 2: SavingsAccount now IMPLEMENTS the IInterestBearing
 *          interface, and overrides ProcessMonthEnd() to add interest.
 */

using BankAccountManager.Interfaces;

namespace BankAccountManager.Models
{
    // INHERITANCE: SavingsAccount IS-AN Account.
    // INTERFACE: SavingsAccount also implements IInterestBearing, so it
    // promises to provide InterestRate, CalculateMonthlyInterest(), and
    // ApplyMonthlyInterest().
    public class SavingsAccount : Account, IInterestBearing
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

        // INTERFACE implementation. Marked virtual so MoneyMarketAccount
        // can change how interest is calculated (POLYMORPHISM).
        public virtual decimal CalculateMonthlyInterest()
        {
            if (Balance <= 0)
            {
                return 0m;
            }
            return Math.Round(Balance * InterestRate / 12, 2);
        }

        // INTERFACE implementation: adds one month of interest.
        public decimal ApplyMonthlyInterest()
        {
            decimal interest = CalculateMonthlyInterest();
            Balance += interest;
            return interest;
        }

        // POLYMORPHISM (override)
        public override string GetAccountDetails()
        {
            return $"APY {InterestRate:P2}, ~{CalculateMonthlyInterest():C}/mo";
        }

        // POLYMORPHISM (override): month-end for savings = add interest.
        public override string ProcessMonthEnd()
        {
            decimal interest = ApplyMonthlyInterest();
            return $"Added {interest:C} interest";
        }
    }
}

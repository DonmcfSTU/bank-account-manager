/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: IInterestBearing is an INTERFACE for any account that earns
 *          interest. It defines what an interest-earning account must be
 *          able to do (report its rate, estimate its monthly interest, and
 *          apply interest), but not how to do it. SavingsAccount implements
 *          this interface, and MoneyMarketAccount gets it through
 *          inheritance. CheckingAccount does not earn interest, so it does
 *          not implement it.
 */

namespace BankAccountManager.Interfaces
{
    // INTERFACE: the contract every interest-earning account must follow.
    public interface IInterestBearing
    {
        // Annual interest rate as a decimal (0.0425 = 4.25%).
        decimal InterestRate { get; }

        // Estimated interest for one month based on the current balance.
        decimal CalculateMonthlyInterest();

        // Adds one month of interest to the balance and returns the amount added.
        decimal ApplyMonthlyInterest();
    }
}

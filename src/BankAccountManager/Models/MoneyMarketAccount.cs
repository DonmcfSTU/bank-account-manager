/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: MoneyMarketAccount is a DERIVED class of SavingsAccount, which is
 *          itself derived from Account. This shows a multi-level inheritance
 *          chain: MoneyMarketAccount -> SavingsAccount -> Account. A money
 *          market account earns interest like a savings account, but it
 *          requires a minimum balance.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: MoneyMarketAccount IS-A SavingsAccount (and IS-AN Account).
    public class MoneyMarketAccount : SavingsAccount
    {
        public decimal MinimumBalance { get; set; }

        public MoneyMarketAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                                  decimal interestRate, decimal minimumBalance)
            : base(accountNumber, openingBalance, dateOpened, interestRate)
        {
            AccountType = "Money Market";
            MinimumBalance = minimumBalance;
        }

        // A withdrawal is not allowed if it would drop the balance below the minimum.
        public override void Withdraw(decimal amount)
        {
            if (Balance - amount < MinimumBalance)
            {
                throw new InvalidOperationException(
                    $"Withdrawal would drop the balance below the {MinimumBalance:C} minimum.");
            }
            base.Withdraw(amount);
        }

        public override string GetAccountDetails()
        {
            return $"APY {InterestRate:P2}, Min {MinimumBalance:C}";
        }
    }
}

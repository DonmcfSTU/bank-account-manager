/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: MoneyMarketAccount is a DERIVED class of SavingsAccount, which is
 *          itself derived from Account (MoneyMarketAccount -> SavingsAccount
 *          -> Account). It requires a minimum balance.
 *          Week 2: it inherits the IInterestBearing implementation from
 *          SavingsAccount, but overrides how interest is calculated (no
 *          interest is paid for a month when the balance is below the
 *          minimum) and adds a low-balance fee at month end.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: MoneyMarketAccount IS-A SavingsAccount (and IS-AN Account),
    // so it is also an IInterestBearing account.
    public class MoneyMarketAccount : SavingsAccount
    {
        public decimal MinimumBalance { get; set; }
        public decimal LowBalanceFee { get; set; } = 15.00m;

        public MoneyMarketAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                                  decimal interestRate, decimal minimumBalance)
            : base(accountNumber, openingBalance, dateOpened, interestRate)
        {
            AccountType = "Money Market";
            MinimumBalance = minimumBalance;
        }

        // POLYMORPHISM (override): a withdrawal is not allowed if it would
        // drop the balance below the minimum.
        public override void Withdraw(decimal amount)
        {
            if (Balance - amount < MinimumBalance)
            {
                throw new InvalidOperationException(
                    $"Withdrawal would drop the balance below the {MinimumBalance:C} minimum.");
            }
            base.Withdraw(amount);
        }

        // POLYMORPHISM (override of the interface method from SavingsAccount):
        // no interest is earned while the balance is under the minimum.
        public override decimal CalculateMonthlyInterest()
        {
            if (Balance < MinimumBalance)
            {
                return 0m;
            }
            return base.CalculateMonthlyInterest();
        }

        public override string GetAccountDetails()
        {
            return $"APY {InterestRate:P2}, Min {MinimumBalance:C}";
        }

        // POLYMORPHISM (override): interest if at/above the minimum,
        // otherwise a low-balance fee.
        public override string ProcessMonthEnd()
        {
            if (Balance < MinimumBalance)
            {
                Balance -= LowBalanceFee;
                return $"Below minimum - charged {LowBalanceFee:C} low-balance fee";
            }
            return base.ProcessMonthEnd();
        }
    }
}

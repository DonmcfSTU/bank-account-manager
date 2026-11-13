/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: MoneyMarketAccount is derived from SavingsAccount (which is derived
 *          from the abstract Account class). It requires a minimum balance:
 *          withdrawals can't go below it, no interest is paid while the
 *          balance is under it, and a low-balance fee is charged at month end.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: MoneyMarketAccount IS-A SavingsAccount IS-AN Account.
    public class MoneyMarketAccount : SavingsAccount
    {
        private const decimal DefaultInterestRate = 0.0465m;
        private const decimal DefaultMinimumBalance = 2500.00m;

        // ACCESS SPECIFIERS: read-only from outside.
        public decimal MinimumBalance { get; }
        public decimal LowBalanceFee { get; } = 15.00m;

        public override string AccountType => "Money Market";

        // CONSTRUCTOR (full): ": base(...)" passes the first four values up to
        // SavingsAccount, which passes three of them up to Account.
        public MoneyMarketAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                                  decimal interestRate, decimal minimumBalance)
            : base(accountNumber, openingBalance, dateOpened, interestRate)
        {
            if (minimumBalance < 0)
            {
                throw new ArgumentException("Minimum balance cannot be negative.");
            }
            MinimumBalance = minimumBalance;
        }

        // CONSTRUCTOR (overload): new money market account with standard terms.
        // A brand-new account must be opened with at least the minimum, so the
        // opening balance is checked by RequireMinimum() BEFORE the chained
        // constructor runs.
        public MoneyMarketAccount(string accountNumber, decimal openingBalance)
            : this(accountNumber, RequireMinimum(openingBalance), DateTime.Today,
                   DefaultInterestRate, DefaultMinimumBalance)
        {
        }

        // ACCESS SPECIFIER - private static: a helper used only by the
        // constructor above.
        private static decimal RequireMinimum(decimal openingBalance)
        {
            if (openingBalance < DefaultMinimumBalance)
            {
                throw new ArgumentException(
                    $"A new money market account requires an opening deposit of at least {DefaultMinimumBalance:C}.");
            }
            return openingBalance;
        }

        // ABSTRACTION + ACCESS SPECIFIER (protected override): adds the
        // minimum-balance rule on top of SavingsAccount's rule.
        protected override bool CanWithdraw(decimal amount, out string reason)
        {
            if (Balance - amount < MinimumBalance)
            {
                reason = $"Withdrawal would drop the balance below the {MinimumBalance:C} minimum.";
                return false;
            }
            return base.CanWithdraw(amount, out reason);
        }

        // No interest while below the minimum.
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

        public override string ProcessMonthEnd()
        {
            if (Balance < MinimumBalance)
            {
                PostTransaction(TransactionType.Fee, -LowBalanceFee, "Low-balance fee");
                return $"Below minimum - charged {LowBalanceFee:C} low-balance fee";
            }
            return base.ProcessMonthEnd();
        }
    }
}

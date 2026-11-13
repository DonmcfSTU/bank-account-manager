/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: SavingsAccount is a concrete DERIVED class of the abstract Account
 *          class, and it implements the IInterestBearing interface (Week 2).
 *          It earns monthly interest and fills in the abstract methods with
 *          savings rules. MoneyMarketAccount inherits from this class.
 */

using BankAccountManager.Interfaces;

namespace BankAccountManager.Models
{
    // INHERITANCE + INTERFACE: SavingsAccount IS-AN Account and IS
    // IInterestBearing.
    public class SavingsAccount : Account, IInterestBearing
    {
        private const decimal DefaultInterestRate = 0.0400m;

        // ACCESS SPECIFIER: public get (the interface requires it), private
        // set so the rate can only change through ChangeInterestRate().
        public decimal InterestRate { get; private set; }

        public override string AccountType => "Savings";

        // CONSTRUCTOR (full)
        public SavingsAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                              decimal interestRate)
            : base(accountNumber, openingBalance, dateOpened)
        {
            SetRate(interestRate);
        }

        // CONSTRUCTOR (overload): new savings account opened today at the
        // standard rate. Chains to the full constructor.
        public SavingsAccount(string accountNumber, decimal openingBalance)
            : this(accountNumber, openingBalance, DateTime.Today, DefaultInterestRate)
        {
        }

        public void ChangeInterestRate(decimal newRate) => SetRate(newRate);

        // INTERFACE implementation. Virtual so MoneyMarketAccount can change it.
        public virtual decimal CalculateMonthlyInterest()
        {
            if (Balance <= 0)
            {
                return 0m;
            }
            return Math.Round(Balance * InterestRate / 12, 2);
        }

        // INTERFACE implementation: posts one month of interest as a transaction.
        public decimal ApplyMonthlyInterest()
        {
            decimal interest = CalculateMonthlyInterest();
            if (interest > 0)
            {
                PostTransaction(TransactionType.Interest, interest, "Monthly interest");
            }
            return interest;
        }

        // ABSTRACTION (implemented): savings can't go below $0.
        // Protected + virtual so MoneyMarketAccount can add its minimum rule.
        protected override bool CanWithdraw(decimal amount, out string reason)
        {
            if (amount > Balance)
            {
                reason = "Insufficient funds for this withdrawal.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public override string GetAccountDetails()
        {
            return $"APY {InterestRate:P2}, ~{CalculateMonthlyInterest():C}/mo";
        }

        public override string ProcessMonthEnd()
        {
            decimal interest = ApplyMonthlyInterest();
            return $"Added {interest:C} interest";
        }

        // ACCESS SPECIFIER - private: shared validation for the constructor
        // and ChangeInterestRate(); nobody outside needs to call it.
        private void SetRate(decimal rate)
        {
            if (rate < 0 || rate > 0.25m)
            {
                throw new ArgumentException("Interest rate must be between 0% and 25%.");
            }
            InterestRate = rate;
        }
    }
}

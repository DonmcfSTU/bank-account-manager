/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: CheckingAccount is a concrete DERIVED class of the abstract Account
 *          class. It has a monthly maintenance fee and an overdraft limit,
 *          and it fills in the abstract methods with checking-account rules.
 *          It does not earn interest, so it does not implement IInterestBearing.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: CheckingAccount IS-AN Account (concrete class).
    public class CheckingAccount : Account
    {
        // Default values used by the shorter constructor. Private const because
        // nothing outside this class needs them.
        private const decimal DefaultMonthlyFee = 5.00m;
        private const decimal DefaultOverdraftLimit = 250.00m;

        // ACCESS SPECIFIERS: public get so the screen can show them, private
        // set so they can only be set by the constructor or ChangeFee().
        public decimal MonthlyFee { get; private set; }
        public decimal OverdraftLimit { get; private set; }

        public override string AccountType => "Checking";

        // CONSTRUCTOR (full): used when loading existing accounts with a known
        // open date, fee, and overdraft limit. ": base(...)" calls Account's
        // protected constructor.
        public CheckingAccount(string accountNumber, decimal openingBalance, DateTime dateOpened,
                               decimal monthlyFee, decimal overdraftLimit)
            : base(accountNumber, openingBalance, dateOpened)
        {
            if (monthlyFee < 0 || overdraftLimit < 0)
            {
                throw new ArgumentException("Fee and overdraft limit cannot be negative.");
            }
            MonthlyFee = monthlyFee;
            OverdraftLimit = overdraftLimit;
        }

        // CONSTRUCTOR (overload): used when a customer opens a new checking
        // account from the menu. It chains to the full constructor with today's
        // date and the standard fee and overdraft limit.
        public CheckingAccount(string accountNumber, decimal openingBalance)
            : this(accountNumber, openingBalance, DateTime.Today, DefaultMonthlyFee, DefaultOverdraftLimit)
        {
        }

        // Public method instead of a public setter, so the rule (no negative
        // fee) is always checked.
        public void ChangeFee(decimal newFee)
        {
            if (newFee < 0)
            {
                throw new ArgumentException("Fee cannot be negative.");
            }
            MonthlyFee = newFee;
        }

        // ABSTRACTION (implemented): checking can go negative up to the limit.
        protected override bool CanWithdraw(decimal amount, out string reason)
        {
            if (amount > Balance + OverdraftLimit)
            {
                reason = $"Withdrawal exceeds the {OverdraftLimit:C} overdraft limit.";
                return false;
            }
            reason = string.Empty;
            return true;
        }

        // ABSTRACTION (implemented)
        public override string GetAccountDetails()
        {
            return $"Fee {MonthlyFee:C}/mo, Overdraft {OverdraftLimit:C}";
        }

        // ABSTRACTION (implemented): month-end for checking = charge the fee.
        public override string ProcessMonthEnd()
        {
            if (MonthlyFee == 0)
            {
                return "No monthly fee (fee waived)";
            }
            PostTransaction(TransactionType.Fee, -MonthlyFee, "Monthly maintenance fee");
            return $"Charged monthly fee of {MonthlyFee:C}";
        }
    }
}

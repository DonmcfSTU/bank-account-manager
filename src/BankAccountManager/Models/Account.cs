/*
 * Name:    Donavan D. McFadden
 * Date:    12/06/2026 (updated for Week 4)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 4 - Database Interactions
 *
 * Purpose: Account is the ABSTRACT BASE CLASS for every type of bank account.
 *          It holds what all accounts share (account number, owner, date
 *          opened, balance, and transaction history) and does the work that
 *          is the same for every account: validating amounts, posting
 *          deposits and withdrawals, and recording transactions.
 *
 *          Week 3 changes:
 *          - The class is now abstract, so a plain "Account" can't be created.
 *          - AccountType, GetAccountDetails(), ProcessMonthEnd(), and
 *            CanWithdraw() are abstract - every child class MUST write them.
 *          - Balance can only be changed inside this class. Child classes go
 *            through the protected PostTransaction() method, so every balance
 *            change is recorded in the transaction history.
 *          - Constructors are protected and chained.
 *
 *          Week 4 changes:
 *          - Added the internal RestoreHistory() method so the database layer
 *            can rebuild an account with its saved balance and transactions.
 */

namespace BankAccountManager.Models
{
    // ABSTRACTION + INHERITANCE: abstract base class. It is the general idea
    // of an "account"; the real accounts are Checking, Savings, Money Market.
    public abstract class Account
    {
        // ACCESS SPECIFIERS:
        // - AccountNumber and DateOpened never change, so they are get-only.
        // - OwnerName has an internal setter: only Customer.AddAccount() (in
        //   this project) sets it; outside code can only read it.
        // - Balance is public to read, but the field behind it is private, so
        //   not even child classes can change it without recording a transaction.
        public string AccountNumber { get; }
        public DateTime DateOpened { get; }
        public string OwnerName { get; internal set; } = string.Empty;

        private decimal _balance;
        public decimal Balance => _balance;

        // COMPOSITION: an Account HAS a list of Transactions. The list itself
        // is private; outside code gets a read-only view.
        private readonly List<Transaction> _transactions = new List<Transaction>();
        public IReadOnlyList<Transaction> Transactions => _transactions;

        // ABSTRACTION: each child class supplies its own type name.
        public abstract string AccountType { get; }

        // CONSTRUCTOR (main): protected because Account is abstract - only
        // child classes can call it through ": base(...)". It validates the
        // opening balance and records the opening deposit.
        protected Account(string accountNumber, decimal openingBalance, DateTime dateOpened)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                throw new ArgumentException("Account number is required.");
            }
            if (openingBalance < 0)
            {
                throw new ArgumentException("Opening balance cannot be negative.");
            }

            AccountNumber = accountNumber.ToUpper();
            DateOpened = dateOpened;
            _balance = openingBalance;
            _transactions.Add(new Transaction(dateOpened, TransactionType.OpeningDeposit,
                openingBalance, _balance, "Account opened"));
        }

        // CONSTRUCTOR (overload): for a brand-new account opened today.
        // CONSTRUCTOR CHAINING: ": this(...)" reuses the main constructor.
        protected Account(string accountNumber, decimal openingBalance)
            : this(accountNumber, openingBalance, DateTime.Today)
        {
        }

        // Public: the menu needs to make deposits. Same rules for every account,
        // so it is NOT virtual.
        public void Deposit(decimal amount)
        {
            ValidateAmount(amount);
            PostTransaction(TransactionType.Deposit, amount, "Deposit");
        }

        // Public: the menu needs to make withdrawals. This is a "template":
        // the steps are always the same (validate, check the rule, post), but
        // the RULE comes from the child class through the abstract CanWithdraw().
        public void Withdraw(decimal amount)
        {
            ValidateAmount(amount);
            if (!CanWithdraw(amount, out string reason))
            {
                throw new InvalidOperationException(reason);
            }
            PostTransaction(TransactionType.Withdrawal, -amount, "Withdrawal");
        }

        // Closes out the account by paying out the remaining balance. Internal
        // because only Customer.CloseAccount() should be able to do this.
        internal decimal CloseOut()
        {
            decimal payout = _balance;
            if (payout != 0)
            {
                PostTransaction(TransactionType.Closing, -payout, "Account closed - balance paid out");
            }
            return payout;
        }

        // Week 4: used only by DatabaseManager when it loads an account from
        // SQLite. The constructor records a new "Account opened" transaction,
        // so this swaps in the real saved history and balance instead.
        // ACCESS SPECIFIER - internal: only code inside this project (the
        // database layer) may do this; the menu and outside code can't.
        internal void RestoreHistory(decimal balance, IEnumerable<Transaction> savedHistory)
        {
            var history = new List<Transaction>(savedHistory);
            if (history.Count == 0)
            {
                throw new ArgumentException("An account must have at least one transaction.");
            }
            _transactions.Clear();
            _transactions.AddRange(history);
            _balance = balance;
        }

        // ABSTRACTION: each account type decides if a withdrawal is allowed.
        // Protected because only the base class (Withdraw) and children use it.
        protected abstract bool CanWithdraw(decimal amount, out string reason);

        // ABSTRACTION: each account type describes its own features.
        public abstract string GetAccountDetails();

        // ABSTRACTION: each account type has its own month-end rules.
        // (Called polymorphically from Program - see Week 2.)
        public abstract string ProcessMonthEnd();

        // ACCESS SPECIFIER - protected: child classes need to post fees and
        // interest, but outside code must not change balances directly.
        // Positive amount = money in, negative = money out.
        protected void PostTransaction(TransactionType type, decimal amount, string description)
        {
            _balance += amount;
            _transactions.Add(new Transaction(type, amount, _balance, description));
        }

        // ACCESS SPECIFIER - private: only used inside this class.
        private static void ValidateAmount(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.");
            }
            if (decimal.Round(amount, 2) != amount)
            {
                throw new ArgumentException("Amount can't have more than two decimal places.");
            }
        }
    }
}

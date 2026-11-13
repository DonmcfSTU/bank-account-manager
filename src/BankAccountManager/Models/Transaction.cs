/*
 * Name:    Donavan D. McFadden
 * Date:    11/29/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 3 - Abstraction, Constructors, & Access Specifiers
 *
 * Purpose: A Transaction is one entry in an account's history: the date,
 *          the type (deposit, withdrawal, fee, interest...), the amount, the
 *          balance after it posted, and a short description. Every Account
 *          keeps a list of these (COMPOSITION). Transactions are read-only
 *          once they are created, the same way a real bank statement can't
 *          be edited.
 */

namespace BankAccountManager.Models
{
    public class Transaction
    {
        // ACCESS SPECIFIERS: every property is public get-only. Outside code
        // can read a transaction but can never change one after it's made.
        public DateTime Date { get; }
        public TransactionType Type { get; }
        public decimal Amount { get; }
        public decimal BalanceAfter { get; }
        public string Description { get; }

        // CONSTRUCTOR (main): sets every field. Marked internal so only code
        // inside this project (the Account class) can create transactions -
        // nobody can make up a fake transaction from outside the model.
        internal Transaction(DateTime date, TransactionType type, decimal amount,
                             decimal balanceAfter, string description)
        {
            Date = date;
            Type = type;
            Amount = amount;
            BalanceAfter = balanceAfter;
            Description = description;
        }

        // CONSTRUCTOR (overload): uses the current date/time. It chains to the
        // main constructor with ": this(...)" so the assignments live in one place.
        internal Transaction(TransactionType type, decimal amount, decimal balanceAfter, string description)
            : this(DateTime.Now, type, amount, balanceAfter, description)
        {
        }
    }
}

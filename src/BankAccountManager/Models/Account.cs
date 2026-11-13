/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: Account is the BASE CLASS for every type of bank account in the
 *          application. It holds the data that all accounts share (account
 *          number, owner, open date, and balance) and the basic Deposit and
 *          Withdraw behavior. CheckingAccount, SavingsAccount, and
 *          MoneyMarketAccount all inherit from this class.
 */

namespace BankAccountManager.Models
{
    // INHERITANCE: this is the base (parent) class.
    public class Account
    {
        public string AccountNumber { get; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime DateOpened { get; }

        // The balance can be read from anywhere, but only this class and
        // its child classes can change it (protected set).
        public decimal Balance { get; protected set; }

        // Each child class sets its own type name, e.g. "Checking".
        public string AccountType { get; protected set; } = "General";

        public Account(string accountNumber, decimal openingBalance, DateTime dateOpened)
        {
            if (openingBalance < 0)
            {
                throw new ArgumentException("Opening balance cannot be negative.");
            }

            AccountNumber = accountNumber;
            Balance = openingBalance;
            DateOpened = dateOpened;
        }

        // Adds money to the account.
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be greater than zero.");
            }
            Balance += amount;
        }

        // Removes money from the account if there is enough available.
        // Child classes can change this rule (for example, checking allows
        // a small overdraft).
        public virtual void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.");
            }
            if (amount > Balance)
            {
                throw new InvalidOperationException("Insufficient funds for this withdrawal.");
            }
            Balance -= amount;
        }

        // Extra details that are specific to each account type. The base
        // class has none, so child classes fill this in.
        public virtual string GetAccountDetails()
        {
            return "Standard account";
        }
    }
}

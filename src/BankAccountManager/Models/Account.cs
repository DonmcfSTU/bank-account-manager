/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026 (updated for Week 2)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: Account is the BASE CLASS for every type of bank account in the
 *          application. It holds the data that all accounts share (account
 *          number, owner, open date, and balance) and the basic Deposit and
 *          Withdraw behavior. CheckingAccount, SavingsAccount, and
 *          MoneyMarketAccount all inherit from this class.
 *
 *          Week 2: added the virtual ProcessMonthEnd() method. Each child
 *          class overrides it with its own month-end rules, which is one of
 *          the ways this project demonstrates polymorphism. (In Week 3 this
 *          class will become abstract.)
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

        // Adds money to the account. Shared by every account type.
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be greater than zero.");
            }
            Balance += amount;
        }

        // POLYMORPHISM: virtual method - child classes can override the
        // withdrawal rules (overdraft for checking, minimum balance for
        // money market). The program always calls account.Withdraw(), and
        // C# runs the version that belongs to the actual object type.
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

        // POLYMORPHISM: each account type describes its own details.
        public virtual string GetAccountDetails()
        {
            return "Standard account";
        }

        // POLYMORPHISM: each account type has its own month-end rules
        // (fees, interest, etc.). The base version does nothing and just
        // reports that, so a plain Account still works if one is created.
        public virtual string ProcessMonthEnd()
        {
            return "No month-end activity";
        }
    }
}

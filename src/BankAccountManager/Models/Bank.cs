/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 2 - Software Design; Interfaces & Polymorphism
 *
 * Purpose: The Bank class is the "container" for the whole application. It
 *          holds the bank's name and its list of customers, and it provides
 *          methods to find customers and accounts. Moving this out of
 *          Program.cs keeps the menu code simple, and in Week 4 this is
 *          where the SQLite database will plug in.
 */

using BankAccountManager.Interfaces;

namespace BankAccountManager.Models
{
    // COMPOSITION: a Bank HAS customers (and each Customer has an Address
    // and Accounts), so the whole program is built from smaller objects.
    public class Bank
    {
        public string Name { get; }

        private readonly List<Customer> _customers = new List<Customer>();
        public IReadOnlyList<Customer> Customers => _customers;

        public Bank(string name)
        {
            Name = name;
        }

        public void AddCustomer(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }
            _customers.Add(customer);
        }

        public Customer? FindCustomer(int customerId)
        {
            return _customers.Find(c => c.CustomerId == customerId);
        }

        public Account? FindAccount(string accountNumber)
        {
            foreach (Account account in GetAllAccounts())
            {
                if (account.AccountNumber.Equals(accountNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return account;
                }
            }
            return null;
        }

        // Every account at the bank, as a list of the BASE type Account.
        // The list really holds CheckingAccount, SavingsAccount, and
        // MoneyMarketAccount objects (POLYMORPHISM).
        public List<Account> GetAllAccounts()
        {
            var all = new List<Account>();
            foreach (Customer customer in _customers)
            {
                all.AddRange(customer.Accounts);
            }
            return all;
        }

        // POLYMORPHISM through the INTERFACE: returns only the accounts that
        // implement IInterestBearing. The caller doesn't need to know whether
        // each one is a SavingsAccount or a MoneyMarketAccount.
        public List<IInterestBearing> GetInterestBearingAccounts()
        {
            var list = new List<IInterestBearing>();
            foreach (Account account in GetAllAccounts())
            {
                if (account is IInterestBearing interestAccount)
                {
                    list.Add(interestAccount);
                }
            }
            return list;
        }
    }
}

/*
 * Name:    Donavan D. McFadden
 * Date:    12/06/2026 (updated for Week 4)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 4 - Database Interactions
 *
 * Purpose: The Bank class holds all customers and is the main entry point for
 *          finding, adding, and removing customers and accounts.
 *          Week 3: added a default constructor, automatic customer IDs and
 *          account numbers (kept private), and methods to open and close
 *          accounts.
 *          Week 4: added RemoveCustomer() so a customer can be deleted (the
 *          Bank and the database both refuse if the customer has accounts).
 */

using BankAccountManager.Interfaces;

namespace BankAccountManager.Models
{
    // COMPOSITION: a Bank HAS customers.
    public class Bank
    {
        private const string DefaultName = "McFadden Community Bank";

        public string Name { get; }

        // ACCESS SPECIFIERS - private: the customer list and the next-ID
        // counter are only managed inside Bank, so IDs can't be duplicated.
        private readonly List<Customer> _customers = new List<Customer>();
        private int _nextCustomerId = 1001;

        // Every account number ever handed out, including closed accounts,
        // so a closed account's number is never reused.
        private readonly HashSet<string> _usedAccountNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<Customer> Customers => _customers;

        // CONSTRUCTOR (parameterized)
        public Bank(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim();
        }

        // CONSTRUCTOR (default / parameterless): chains to the one above.
        public Bank() : this(DefaultName)
        {
        }

        // Gives out the next customer ID (used when adding a new customer).
        public int GetNextCustomerId() => _nextCustomerId;

        public void AddCustomer(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }
            if (FindCustomer(customer.CustomerId) != null)
            {
                throw new InvalidOperationException($"Customer ID {customer.CustomerId} already exists.");
            }
            _customers.Add(customer);
            foreach (Account account in customer.Accounts)
            {
                _usedAccountNumbers.Add(account.AccountNumber);
            }
            if (customer.CustomerId >= _nextCustomerId)
            {
                _nextCustomerId = customer.CustomerId + 1;
            }
        }

        // Removes a customer. Only allowed when all of their accounts are
        // closed, so no money or account history is ever left without an owner.
        public void RemoveCustomer(Customer customer)
        {
            if (!_customers.Contains(customer))
            {
                throw new InvalidOperationException("That customer is not in this bank.");
            }
            if (customer.Accounts.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{customer.FullName} still has {customer.Accounts.Count} open account(s). Close them first (option 10).");
            }
            _customers.Remove(customer);
        }

        // Builds the next unused account number for a customer, following the
        // pattern PREFIX-<customer ID><2-digit sequence>, e.g. "SAV-100503".
        public string GenerateAccountNumber(Customer customer, string prefix)
        {
            int sequence = customer.Accounts.Count + 1;
            string number = BuildAccountNumber(prefix, customer.CustomerId, sequence);
            while (FindAccount(number) != null || _usedAccountNumbers.Contains(number))
            {
                sequence++;
                number = BuildAccountNumber(prefix, customer.CustomerId, sequence);
            }
            _usedAccountNumbers.Add(number);
            return number;
        }

        // ACCESS SPECIFIER - private static: formatting helper used only here.
        private static string BuildAccountNumber(string prefix, int customerId, int sequence)
        {
            return $"{prefix}-{customerId}{sequence:00}";
        }

        public Customer? FindCustomer(int customerId)
        {
            return _customers.Find(c => c.CustomerId == customerId);
        }

        public Account? FindAccount(string accountNumber)
        {
            foreach (Account account in GetAllAccounts())
            {
                if (account.AccountNumber.Equals(accountNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return account;
                }
            }
            return null;
        }

        public Customer? FindOwner(Account account)
        {
            return _customers.Find(c => c.OwnsAccount(account));
        }

        public List<Account> GetAllAccounts()
        {
            var all = new List<Account>();
            foreach (Customer customer in _customers)
            {
                all.AddRange(customer.Accounts);
            }
            return all;
        }

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

/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: The Customer class represents one bank customer. A customer has
 *          a name, phone number, email, an Address, and a list of bank
 *          accounts. This class is my main demonstration of COMPOSITION:
 *          instead of putting address fields and account fields directly in
 *          Customer, the class is "composed of" an Address object and a
 *          collection of Account objects.
 */

namespace BankAccountManager.Models
{
    public class Customer
    {
        public int CustomerId { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // COMPOSITION: a Customer HAS-AN Address.
        // The Address object is created along with the Customer and is part
        // of the Customer's data.
        public Address MailingAddress { get; set; }

        // COMPOSITION: a Customer HAS bank accounts.
        // The list is private so outside code can't replace it; accounts are
        // added through the AddAccount method below.
        private readonly List<Account> _accounts = new List<Account>();

        // Read-only view of the accounts so other classes can display them
        // without being able to change the list directly.
        public IReadOnlyList<Account> Accounts => _accounts;

        public Customer(int customerId, string firstName, string lastName,
                        string phone, string email, Address mailingAddress)
        {
            CustomerId = customerId;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            Email = email;
            MailingAddress = mailingAddress;
        }

        public string FullName => $"{FirstName} {LastName}";

        // Adds an account to this customer and links the account back to
        // the customer so the account knows who owns it.
        public void AddAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            account.OwnerName = FullName;
            _accounts.Add(account);
        }

        // Adds up the balances of every account this customer owns.
        public decimal GetTotalBalance()
        {
            decimal total = 0m;
            foreach (Account account in _accounts)
            {
                total += account.Balance;
            }
            return total;
        }
    }
}

/*
 * Name:    Donavan D. McFadden
 * Date:    11/29/2026 (updated for Week 3)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 3 - Abstraction, Constructors, & Access Specifiers
 *
 * Purpose: The Customer class represents one bank customer. It is my main
 *          COMPOSITION class: a Customer is made of an Address and a list of
 *          Accounts.
 *          Week 3: contact properties now have private setters and are
 *          changed through UpdateContactInfo()/UpdateAddress(), which validate
 *          the input. Added a second constructor and the ability to close
 *          (remove) an account.
 */

namespace BankAccountManager.Models
{
    public class Customer
    {
        // ACCESS SPECIFIERS: CustomerId never changes (get-only). The other
        // values are public to read but private to set, so every change has
        // to go through the update methods below.
        public int CustomerId { get; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        // COMPOSITION: a Customer HAS-AN Address.
        public Address MailingAddress { get; private set; }

        // COMPOSITION: a Customer HAS Accounts. Private list, read-only view.
        private readonly List<Account> _accounts = new List<Account>();
        public IReadOnlyList<Account> Accounts => _accounts;

        public string FullName => $"{FirstName} {LastName}";

        // CONSTRUCTOR (main): takes an Address object. It stores a COPY made
        // with Address's copy constructor so the customer owns its address.
        public Customer(int customerId, string firstName, string lastName,
                        string phone, string email, Address mailingAddress)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer ID must be positive.");
            }
            if (mailingAddress == null)
            {
                throw new ArgumentNullException(nameof(mailingAddress));
            }

            CustomerId = customerId;
            UpdateContactInfo(firstName, lastName, phone, email);
            MailingAddress = new Address(mailingAddress);
        }

        // CONSTRUCTOR (overload): takes the address pieces directly and builds
        // the Address itself. Used by the "Add a new customer" menu option.
        // Chains to the main constructor with ": this(...)".
        public Customer(int customerId, string firstName, string lastName, string phone, string email,
                        string street, string city, string state, string zipCode)
            : this(customerId, firstName, lastName, phone, email, new Address(street, city, state, zipCode))
        {
        }

        // Public: the "Update customer" menu needs it. Validates everything
        // first, so a bad value doesn't leave the customer half-updated.
        public void UpdateContactInfo(string firstName, string lastName, string phone, string email)
        {
            string first = Require(firstName, "First name");
            string last = Require(lastName, "Last name");
            string cleanPhone = Require(phone, "Phone");
            string cleanEmail = Require(email, "Email");

            if (!cleanEmail.Contains('@') || !cleanEmail.Contains('.'))
            {
                throw new ArgumentException("Email must look like name@example.com.");
            }

            FirstName = first;
            LastName = last;
            Phone = cleanPhone;
            Email = cleanEmail;

            // Keep each account's owner name in sync if the name changed.
            foreach (Account account in _accounts)
            {
                account.OwnerName = FullName;
            }
        }

        public void UpdateAddress(Address newAddress)
        {
            MailingAddress = new Address(newAddress ?? throw new ArgumentNullException(nameof(newAddress)));
        }

        public void AddAccount(Account account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            account.OwnerName = FullName;
            _accounts.Add(account);
        }

        // Closes an account: pays out the balance and removes it from the
        // customer. Returns the amount paid out.
        public decimal CloseAccount(Account account)
        {
            if (!_accounts.Contains(account))
            {
                throw new InvalidOperationException("That account does not belong to this customer.");
            }
            if (account.Balance < 0)
            {
                throw new InvalidOperationException(
                    $"It is overdrawn by {-account.Balance:C} - bring it to $0.00 first.");
            }
            decimal payout = account.CloseOut();
            _accounts.Remove(account);
            return payout;
        }

        public bool OwnsAccount(Account account) => _accounts.Contains(account);

        public decimal GetTotalBalance()
        {
            decimal total = 0m;
            foreach (Account account in _accounts)
            {
                total += account.Balance;
            }
            return total;
        }

        // ACCESS SPECIFIER - private static: input check used only in this class.
        private static string Require(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{fieldName} is required.");
            }
            return value.Trim();
        }
    }
}

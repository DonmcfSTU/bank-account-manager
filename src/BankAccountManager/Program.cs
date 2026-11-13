/*
 * Name:    Donavan D. McFadden
 * Date:    11/29/2026 (updated for Week 3)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 3 - Abstraction, Constructors, & Access Specifiers
 *
 * Purpose: Program is the starting point of the Bank Account Management
 *          application. It shows the header and welcome message, creates the
 *          Bank with its sample customers, and runs the main menu loop until
 *          the user chooses to quit.
 *          Week 3: added options 8-12 (add customer, open account, close
 *          account, update customer, transaction history). These use the new
 *          constructor overloads and the update methods that replaced the
 *          old public setters.
 */

using System.Globalization;
using BankAccountManager.Data;
using BankAccountManager.Interfaces;
using BankAccountManager.Models;
using BankAccountManager.UI;

namespace BankAccountManager
{
    // ACCESS SPECIFIERS: only Main is public. Every helper below is private
    // because nothing outside Program calls them.
    public class Program
    {
        private static Bank _bank = new Bank();   // default constructor

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            ConsoleUI.ShowHeader();
            ConsoleUI.ShowWelcome();
            _bank = SampleData.CreateBank();

            bool firstTime = true;
            bool running = true;
            while (running)
            {
                if (!firstTime)
                {
                    ConsoleUI.ClearScreen();
                    ConsoleUI.ShowHeader();
                }
                firstTime = false;

                ConsoleUI.ShowMainMenu();
                string choice = ConsoleUI.Prompt("Enter your choice: ").ToUpper();

                switch (choice)
                {
                    case "1": ConsoleUI.ShowCustomerTable(_bank.Customers); break;
                    case "2": ShowAllAccounts(); break;
                    case "3": ViewOneCustomer(); break;
                    case "4": MakeDeposit(); break;
                    case "5": MakeWithdrawal(); break;
                    case "6": ConsoleUI.ShowInterestReport(_bank.GetInterestBearingAccounts()); break;
                    case "7": RunMonthEnd(); break;
                    case "8": AddCustomer(); break;
                    case "9": OpenAccount(); break;
                    case "10": CloseAccount(); break;
                    case "11": UpdateCustomer(); break;
                    case "12": ViewTransactionHistory(); break;
                    case "Q":
                    case "QUIT":
                    case "EXIT":
                        running = false;
                        continue;
                    default:
                        ConsoleUI.ShowError($"\"{choice}\" is not a menu option. Please choose 1-12 or Q.");
                        break;
                }
                ConsoleUI.Pause();
            }

            Console.WriteLine();
            Console.WriteLine($"Thank you for using the {_bank.Name} Account Manager. Goodbye!");
        }

        private static void ShowAllAccounts()
        {
            ConsoleUI.ShowAccountTable(_bank.GetAllAccounts());
        }

        private static void ViewOneCustomer()
        {
            Customer? customer = AskForCustomer();
            if (customer != null)
            {
                ConsoleUI.ShowCustomerDetails(customer);
            }
        }

        private static void MakeDeposit()
        {
            Account? account = AskForAccount();
            if (account == null) return;

            decimal amount = ConsoleUI.PromptForAmount("Deposit amount: $");
            try
            {
                account.Deposit(amount);
                ConsoleUI.ShowSuccess($"Deposited {amount:C}. New balance for {account.AccountNumber}: {account.Balance:C}");
            }
            catch (ArgumentException ex)
            {
                ConsoleUI.ShowError(ex.Message);
            }
        }

        private static void MakeWithdrawal()
        {
            Account? account = AskForAccount();
            if (account == null) return;

            decimal amount = ConsoleUI.PromptForAmount("Withdrawal amount: $");
            try
            {
                // ABSTRACTION: Withdraw() is written once in Account, but it
                // asks the child class's CanWithdraw() whether it's allowed.
                account.Withdraw(amount);
                ConsoleUI.ShowSuccess($"Withdrew {amount:C}. New balance for {account.AccountNumber}: {account.Balance:C}");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ConsoleUI.ShowError($"Withdrawal declined: {ex.Message}");
            }
        }

        // POLYMORPHISM (Week 2) + ABSTRACTION (Week 3): ProcessMonthEnd() is
        // abstract in Account, so every account type is guaranteed to have it.
        private static void RunMonthEnd()
        {
            if (!ConsoleUI.PromptYesNo("\nThis will apply fees and interest to every account. Continue?"))
            {
                ConsoleUI.ShowError("Month-end processing was canceled. No balances were changed.");
                return;
            }

            var results = new List<(Account Account, decimal Before, string Result)>();
            foreach (Account account in _bank.GetAllAccounts())
            {
                decimal before = account.Balance;
                string result = account.ProcessMonthEnd();
                results.Add((account, before, result));
            }
            ConsoleUI.ShowMonthEndResults(results);
            ConsoleUI.ShowSuccess("Month-end processing complete.");
        }

        // Option 8 - uses Customer's CONSTRUCTOR overload that builds the
        // Address from its pieces.
        private static void AddCustomer()
        {
            int newId = _bank.GetNextCustomerId();
            Console.WriteLine($"\nNew customer (Customer ID will be {newId}). All fields are required.");
            try
            {
                var customer = new Customer(newId,
                    ConsoleUI.PromptRequired("First name"),
                    ConsoleUI.PromptRequired("Last name"),
                    ConsoleUI.PromptRequired("Phone"),
                    ConsoleUI.PromptRequired("Email"),
                    ConsoleUI.PromptRequired("Street"),
                    ConsoleUI.PromptRequired("City"),
                    ConsoleUI.PromptRequired("State (2 letters)"),
                    ConsoleUI.PromptRequired("ZIP code"));
                _bank.AddCustomer(customer);
                ConsoleUI.ShowSuccess($"Customer {customer.CustomerId} - {customer.FullName} was added.");
                ConsoleUI.ShowSuccess("Use option 9 to open an account for this customer.");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ConsoleUI.ShowError($"Customer was not added: {ex.Message}");
            }
        }

        // Option 9 - uses each account type's short CONSTRUCTOR overload
        // (today's date and standard terms).
        private static void OpenAccount()
        {
            Customer? customer = AskForCustomer();
            if (customer == null) return;

            Console.WriteLine("\nAccount type:");
            Console.WriteLine("  1. Checking      ($5.00/mo fee, $250.00 overdraft)");
            Console.WriteLine("  2. Savings       (4.00% APY)");
            Console.WriteLine("  3. Money Market  (4.65% APY, $2,500.00 minimum)");
            string type = ConsoleUI.Prompt("Choose 1-3: ");
            if (type != "1" && type != "2" && type != "3")
            {
                ConsoleUI.ShowError("Please choose 1, 2, or 3. No account was opened.");
                return;
            }

            decimal deposit = ConsoleUI.PromptForAmount("Opening deposit: $");
            try
            {
                Account account = type switch
                {
                    "1" => new CheckingAccount(_bank.GenerateAccountNumber(customer, "CHK"), deposit),
                    "2" => new SavingsAccount(_bank.GenerateAccountNumber(customer, "SAV"), deposit),
                    _ => new MoneyMarketAccount(_bank.GenerateAccountNumber(customer, "MMA"), deposit)
                };
                customer.AddAccount(account);
                ConsoleUI.ShowSuccess($"Opened {account.AccountType} account {account.AccountNumber} for {customer.FullName} with {deposit:C}.");
            }
            catch (ArgumentException ex)
            {
                ConsoleUI.ShowError($"Account was not opened: {ex.Message}");
            }
        }

        // Option 10
        private static void CloseAccount()
        {
            Account? account = AskForAccount();
            if (account == null) return;
            Customer? owner = _bank.FindOwner(account);
            if (owner == null) return;

            Console.WriteLine($"\n{account.AccountNumber} ({account.AccountType}) - {owner.FullName} - balance {account.Balance:C}");
            if (!ConsoleUI.PromptYesNo("Are you sure you want to close this account?"))
            {
                ConsoleUI.ShowError("The account was not closed.");
                return;
            }
            try
            {
                decimal payout = owner.CloseAccount(account);
                ConsoleUI.ShowSuccess($"Account {account.AccountNumber} was closed. {payout:C} will be paid to {owner.FullName} by check.");
            }
            catch (InvalidOperationException ex)
            {
                ConsoleUI.ShowError($"Account was not closed: {ex.Message}");
            }
        }

        // Option 11 - shows each current value; Enter keeps it. Changes go
        // through Customer's public update methods because the properties
        // have private setters (ACCESS SPECIFIERS).
        private static void UpdateCustomer()
        {
            Customer? c = AskForCustomer();
            if (c == null) return;

            Console.WriteLine($"\nUpdating customer {c.CustomerId}. Press Enter to keep the value in [brackets].");
            try
            {
                string first = ConsoleUI.PromptWithDefault("First name", c.FirstName);
                string last = ConsoleUI.PromptWithDefault("Last name", c.LastName);
                string phone = ConsoleUI.PromptWithDefault("Phone", c.Phone);
                string email = ConsoleUI.PromptWithDefault("Email", c.Email);
                string street = ConsoleUI.PromptWithDefault("Street", c.MailingAddress.Street);
                string city = ConsoleUI.PromptWithDefault("City", c.MailingAddress.City);
                string state = ConsoleUI.PromptWithDefault("State", c.MailingAddress.State);
                string zip = ConsoleUI.PromptWithDefault("ZIP code", c.MailingAddress.ZipCode);

                // Build the new Address first so a bad ZIP or state stops the
                // update before anything is changed.
                var newAddress = new Address(street, city, state, zip);
                c.UpdateContactInfo(first, last, phone, email);
                c.UpdateAddress(newAddress);
                ConsoleUI.ShowSuccess("Customer information was updated:");
                ConsoleUI.ShowCustomerDetails(c);
            }
            catch (ArgumentException ex)
            {
                ConsoleUI.ShowError($"Customer was not updated: {ex.Message}");
            }
        }

        // Option 12
        private static void ViewTransactionHistory()
        {
            Account? account = AskForAccount();
            if (account != null)
            {
                ConsoleUI.ShowTransactionHistory(account);
            }
        }

        private static Customer? AskForCustomer()
        {
            ConsoleUI.ShowCustomerTable(_bank.Customers);
            string input = ConsoleUI.Prompt("\nEnter a Customer ID: ");
            Customer? customer = int.TryParse(input, out int id) ? _bank.FindCustomer(id) : null;
            if (customer == null)
            {
                ConsoleUI.ShowError("No customer was found with that ID.");
            }
            return customer;
        }

        private static Account? AskForAccount()
        {
            ShowAllAccounts();
            Account? account = _bank.FindAccount(ConsoleUI.Prompt("\nEnter the account number: "));
            if (account == null)
            {
                ConsoleUI.ShowError("No account was found with that number.");
            }
            return account;
        }
    }
}

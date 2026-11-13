/*
 * Name:    Donavan D. McFadden
 * Date:    12/13/2026 (final version)
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Bank Account Manager - Final Project (Week 5)
 *
 * Purpose: Program is the starting point of the Bank Account Management
 *          application. It shows the header and welcome message, creates the
 *          Bank with its sample customers, and runs the main menu loop until
 *          the user chooses to quit.
 *          Week 3: added options 8-12 (add customer, open account, close
 *          account, update customer, transaction history). These use the new
 *          constructor overloads and the update methods that replaced the
 *          old public setters.
 *          Final version: the header shows the application name, and
 *          option 14 can also show the retired numbers/IDs table.
 *          Week 4: all data is now stored in an SQLite database (bank.db)
 *          through the DatabaseManager class. On startup the program loads
 *          the Bank from the database (or creates the database and saves the
 *          sample data the first time). Every change is written to the
 *          database right away:
 *            Create - option 8 (customer), option 9 (account)
 *            Read   - startup load, options 1-3, 6, 12, and option 14, which
 *                     runs SELECT queries directly on the database tables
 *            Update - options 4, 5, 7 (balances + transactions), 11 (customer)
 *            Delete - option 10 (account + its transactions), 13 (customer)
 */

using System.Globalization;
using BankAccountManager.Data;
using BankAccountManager.Interfaces;
using BankAccountManager.Models;
using BankAccountManager.UI;
using Microsoft.Data.Sqlite;

namespace BankAccountManager
{
    // ACCESS SPECIFIERS: only Main is public. Every helper below is private
    // because nothing outside Program calls them.
    public class Program
    {
        private static Bank _bank = new Bank();   // default constructor

        // CONSTRUCTOR (default): DatabaseManager() uses the "bank.db" file.
        // ACCESS SPECIFIER - private: only Program uses the database object.
        private static readonly DatabaseManager _db = new DatabaseManager();

        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            ConsoleUI.ShowHeader();
            ConsoleUI.ShowWelcome();
            if (!ConnectToDatabase())
            {
                return;
            }

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
                    case "13": DeleteCustomer(); break;
                    case "14": ViewDatabaseRecords(); break;
                    case "Q":
                    case "QUIT":
                    case "EXIT":
                        running = false;
                        continue;
                    default:
                        ConsoleUI.ShowError($"\"{choice}\" is not a menu option. Please choose 1-14 or Q.");
                        break;
                }
                ConsoleUI.Pause();
            }

            Console.WriteLine();
            Console.WriteLine($"All changes were already saved to {_db.FileName} as you made them.");
            Console.WriteLine($"Thank you for using the {_bank.Name} Account Manager. Goodbye!");
        }

        // READ (startup): creates the tables if needed. A brand-new database
        // gets the sample data (CREATE); otherwise the Bank is loaded from it.
        private static bool ConnectToDatabase()
        {
            try
            {
                bool isNew = _db.InitializeDatabase();
                if (isNew)
                {
                    _bank = SampleData.CreateBank();
                    _db.SaveNewBank(_bank);
                }
                else
                {
                    _bank = _db.LoadBank();
                }
                var counts = _db.GetTableCounts();
                Console.WriteLine();
                ConsoleUI.ShowSuccess(isNew
                    ? $"New SQLite database {_db.FileName} was created and filled with the sample data."
                    : $"Connected to the SQLite database {_db.FileName} and loaded the saved data.");
                ConsoleUI.ShowSuccess($"Database contains {counts.Customers} customers, {counts.Accounts} accounts, and {counts.Transactions} transactions.");
                return true;
            }
            catch (Exception ex) when (ex is SqliteException || ex is ArgumentException || ex is InvalidOperationException || ex is FormatException)
            {
                ConsoleUI.ShowError($"The database could not be opened: {ex.Message}");
                return false;
            }
        }

        // Runs one database step and reports a database error instead of
        // crashing. Returns true if the save worked.
        private static bool SaveToDatabase(Action saveStep)
        {
            try
            {
                saveStep();
                return true;
            }
            catch (Exception ex) when (ex is SqliteException || ex is InvalidOperationException)
            {
                ConsoleUI.ShowError($"Database error - the change was not saved: {ex.Message}");
                return false;
            }
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
                // UPDATE: new balance + the deposit transaction row.
                if (SaveToDatabase(() => _db.SaveAccountActivity(account)))
                {
                    ConsoleUI.ShowSuccess("Saved to the database (Accounts row updated, Transactions row added).");
                }
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
                // UPDATE: new balance + the withdrawal transaction row.
                if (SaveToDatabase(() => _db.SaveAccountActivity(account)))
                {
                    ConsoleUI.ShowSuccess("Saved to the database (Accounts row updated, Transactions row added).");
                }
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
            // UPDATE: every account's balance and new fee/interest rows are
            // saved in one SQL transaction (all or nothing).
            if (SaveToDatabase(() => _db.SaveAccountActivity(_bank.GetAllAccounts())))
            {
                ConsoleUI.ShowSuccess($"Saved {results.Count} updated accounts to the database.");
            }
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
                    ConsoleUI.PromptValidated("State (2 letters)",
                        v => v.Length == 2 && v.All(char.IsLetter), "State must be a 2-letter abbreviation (example: VA)."),
                    ConsoleUI.PromptValidated("ZIP code",
                        v => v.Length == 5 && v.All(char.IsDigit), "ZIP code must be 5 digits."));
                // CREATE: INSERT into the Customers table first. If the
                // database refuses it, the customer is not added in memory.
                if (!SaveToDatabase(() => _db.InsertCustomer(customer)))
                {
                    return;
                }
                _bank.AddCustomer(customer);
                ConsoleUI.ShowSuccess($"Customer {customer.CustomerId} - {customer.FullName} was added and saved to the database.");
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
                // CREATE: INSERT the Accounts row and its opening Transactions row.
                if (!SaveToDatabase(() => _db.InsertAccount(customer.CustomerId, account)))
                {
                    return;
                }
                customer.AddAccount(account);
                ConsoleUI.ShowSuccess($"Opened {account.AccountType} account {account.AccountNumber} for {customer.FullName} with {deposit:C}.");
                ConsoleUI.ShowSuccess("Saved to the database (Accounts and Transactions rows added).");
            }
            catch (ArgumentException ex)
            {
                ConsoleUI.ShowError($"Account was not opened: {ex.Message}");
            }
        }

        // Option 10 - DELETE: closes the account, then deletes its Accounts row
        // (ON DELETE CASCADE also deletes its Transactions rows). The customer
        // keeps any other accounts - e.g. close a money market account but
        // keep checking and savings.
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
                if (SaveToDatabase(() => _db.DeleteAccount(account.AccountNumber)))
                {
                    ConsoleUI.ShowSuccess("Deleted from the database (Accounts row and its Transactions rows).");
                }
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
                // UPDATE: the Customers row.
                if (!SaveToDatabase(() => _db.UpdateCustomer(c)))
                {
                    return;
                }
                ConsoleUI.ShowSuccess("Customer information was updated and saved to the database:");
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

        // Option 13 - DELETE: removes a customer who has no open accounts.
        private static void DeleteCustomer()
        {
            Customer? c = AskForCustomer();
            if (c == null) return;

            Console.WriteLine($"\nCustomer {c.CustomerId} - {c.FullName} ({c.Accounts.Count} open account(s))");
            if (!ConsoleUI.PromptYesNo("Are you sure you want to delete this customer?"))
            {
                ConsoleUI.ShowError("The customer was not deleted.");
                return;
            }
            try
            {
                _bank.RemoveCustomer(c);   // refuses if accounts are still open
            }
            catch (InvalidOperationException ex)
            {
                ConsoleUI.ShowError($"Customer was not deleted: {ex.Message}");
                return;
            }
            if (SaveToDatabase(() => _db.DeleteCustomer(c.CustomerId)))
            {
                ConsoleUI.ShowSuccess($"Customer {c.CustomerId} - {c.FullName} was deleted from the database.");
            }
        }

        // Option 14 - READ: runs SELECT queries straight on the database file so
        // the user can see exactly what is stored in each table.
        private static void ViewDatabaseRecords()
        {
            try
            {
                var counts = _db.GetTableCounts();
                Console.WriteLine($"\nDatabase file: {_db.FileName}");
                Console.WriteLine($"  Customers: {counts.Customers} rows   Accounts: {counts.Accounts} rows   Transactions: {counts.Transactions} rows");
                Console.WriteLine("\nWhich table do you want to see?");
                Console.WriteLine("  1. Customers");
                Console.WriteLine("  2. Accounts");
                Console.WriteLine("  3. Transactions for one account");
                Console.WriteLine("  4. Retired account numbers and customer IDs");
                switch (ConsoleUI.Prompt("Choose 1-4: "))
                {
                    case "1":
                        ConsoleUI.ShowRows("SELECT * FROM Customers",
                            new[] { ("ID", 6), ("Name", 18), ("Phone", 16), ("Email", 26), ("Address", 43) },
                            _db.GetCustomerRows());
                        break;
                    case "2":
                        ConsoleUI.ShowRows("SELECT * FROM Accounts",
                            new[] { ("AccountNumber", 14), ("CustID", 7), ("AccountType", 13), ("Balance", 11), ("DateOpened", 11),
                                    ("Fee", 6), ("Overdraft", 10), ("Rate", 7), ("Minimum", 8) },
                            _db.GetAccountRows());
                        break;
                    case "3":
                        string number = ConsoleUI.Prompt("Account number: ");
                        ConsoleUI.ShowRows($"SELECT * FROM Transactions WHERE AccountNumber = '{number.ToUpper()}'",
                            new[] { ("ID", 5), ("TransDate", 17), ("TransType", 15), ("Amount", 11), ("BalanceAfter", 13), ("Description", 34) },
                            _db.GetTransactionRows(number));
                        break;
                    case "4":
                        ConsoleUI.ShowRows("SELECT * FROM RetiredIds",
                            new[] { ("IdType", 10), ("IdValue", 14), ("RetiredOn", 17) },
                            _db.GetRetiredRows());
                        break;
                    default:
                        ConsoleUI.ShowError("Please choose 1, 2, 3, or 4.");
                        break;
                }
            }
            catch (SqliteException ex)
            {
                ConsoleUI.ShowError($"Database error: {ex.Message}");
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

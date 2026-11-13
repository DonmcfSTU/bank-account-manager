/*
 * Name:    Donavan D. McFadden
 * Date:    11/12/2026
 * Course:  SDC320L - Advanced Object-Oriented Programming Using C#
 * Project: Week 1 - Inheritance, Composition, and User Interactions
 *
 * Purpose: Program is the starting point of the Bank Account Management
 *          application. It shows the header and welcome message, loads the
 *          sample customers, and runs the main menu loop until the user
 *          chooses to quit. Each menu choice calls a small helper method.
 */

using System.Globalization;
using BankAccountManager.Data;
using BankAccountManager.Models;
using BankAccountManager.UI;

namespace BankAccountManager
{
    public class Program
    {
        private static List<Customer> _customers = new List<Customer>();

        public static void Main(string[] args)
        {
            // Use U.S. formatting so currency always shows as $1,234.56
            // no matter what region the computer is set to.
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            ConsoleUI.ShowHeader();
            ConsoleUI.ShowWelcome();
            bool firstTime = true;

            // Instantiate the classes with realistic (sample) information.
            _customers = SampleData.CreateCustomers();

            bool running = true;
            while (running)
            {
                // After the first pass, clear the screen so each menu
                // action starts on a clean screen with the header at the top.
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
                    case "1":
                        ConsoleUI.ShowCustomerTable(_customers);
                        ConsoleUI.Pause();
                        break;
                    case "2":
                        ShowAllAccounts();
                        ConsoleUI.Pause();
                        break;
                    case "3":
                        ViewOneCustomer();
                        ConsoleUI.Pause();
                        break;
                    case "4":
                        MakeDeposit();
                        ConsoleUI.Pause();
                        break;
                    case "5":
                        MakeWithdrawal();
                        ConsoleUI.Pause();
                        break;
                    case "Q":
                    case "QUIT":
                    case "EXIT":
                        running = false;
                        break;
                    default:
                        ConsoleUI.ShowError($"\"{choice}\" is not a menu option. Please choose 1-5 or Q.");
                        ConsoleUI.Pause();
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Thank you for using the Bank Account Manager. Goodbye!");
        }

        // Gathers every account from every customer into one table.
        private static void ShowAllAccounts()
        {
            var allAccounts = new List<Account>();
            foreach (Customer customer in _customers)
            {
                allAccounts.AddRange(customer.Accounts);
            }
            ConsoleUI.ShowAccountTable(allAccounts);
        }

        private static void ViewOneCustomer()
        {
            ConsoleUI.ShowCustomerTable(_customers);
            Customer? customer = FindCustomer(ConsoleUI.Prompt("\nEnter a Customer ID: "));
            if (customer == null)
            {
                ConsoleUI.ShowError("No customer was found with that ID.");
                return;
            }
            ConsoleUI.ShowCustomerDetails(customer);
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
                // Each account type applies its own withdrawal rule here
                // (overdraft for checking, minimum balance for money market).
                account.Withdraw(amount);
                ConsoleUI.ShowSuccess($"Withdrew {amount:C}. New balance for {account.AccountNumber}: {account.Balance:C}");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                ConsoleUI.ShowError($"Withdrawal declined: {ex.Message}");
            }
        }

        // Shows the account list and asks the user to pick one by number.
        private static Account? AskForAccount()
        {
            ShowAllAccounts();
            string accountNumber = ConsoleUI.Prompt("\nEnter the account number: ").ToUpper();
            foreach (Customer customer in _customers)
            {
                foreach (Account account in customer.Accounts)
                {
                    if (account.AccountNumber == accountNumber)
                    {
                        return account;
                    }
                }
            }
            ConsoleUI.ShowError("No account was found with that number.");
            return null;
        }

        private static Customer? FindCustomer(string input)
        {
            if (!int.TryParse(input, out int id)) return null;
            return _customers.Find(c => c.CustomerId == id);
        }
    }
}
